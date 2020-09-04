/// Holds the dotvvm viewmodel

import { createArray, hasOwnProperty, symbolOrDollar, isPrimitive, keys } from "./utils/objects";
import { DotvvmEvent } from "./events";
import { serializeDate } from "./serialization/date";
import { func } from "../../node_modules/fast-check/lib/types/fast-check";
import { extendToObservableArrayIfRequired, isOptionsProperty } from "./serialization/deserialize"


export const currentStateSymbol = Symbol("currentState")
const notifySymbol = Symbol("notify")

const internalPropCache = Symbol("internalPropCache")
const updateSymbol = Symbol("update")
const updatePropertySymbol = Symbol("updateProperty")

let isViewModelUpdating: boolean = false;

export function getIsViewModelUpdating() {
    return isViewModelUpdating;
}

export type UpdatableObjectExtensions<T> = {
    [notifySymbol]: (newValue: T) => void
    [currentStateSymbol]: T
    [updateSymbol]?: UpdateDispatcher<T>
}

export type DeepKnockoutWrapped<T> =
    (T extends (infer R)[] ? DeepKnockoutWrappedArray<R> :
    T extends object ? KnockoutObservable<DeepKnockoutWrappedObject<T>> :
    KnockoutObservable<T>) & UpdatableObjectExtensions<T>;

export type DeepKnockoutWrappedArray<T> = KnockoutObservableArray<DeepKnockoutWrapped<T>>

export type DeepKnockoutWrappedObject<T> = {
    readonly [P in keyof T]: DeepKnockoutWrapped<T[P]>;
};


export type StateUpdate<TViewModel> = (initial: TViewModel) => Readonly<TViewModel>
export type UpdateDispatcher<TViewModel> = (update: StateUpdate<TViewModel>) => void
type RenderContext<TViewModel> = {
    // timeFromStartGetter: () => number
    // secondsTimeGetter: () => Date
    update: (updater: StateUpdate<TViewModel>) => void
    dataContext: TViewModel
    parentContext?: RenderContext<any>
    // replacableControls?: { [id: string] : RenderFunction<any> }
    "@extensions"?: { [name: string]: any }
}
// type RenderFunction<TViewModel> = (context: RenderContext<TViewModel>) => virtualDom.VTree;
class TwoWayBinding<T> {
    constructor(
        public readonly update: (updater: StateUpdate<T>) => void,
        public readonly value: T
    ) { }
}

export class StateManager<TViewModel> {
    public readonly stateObservable: DeepKnockoutWrapped<TViewModel>;
    private _state: TViewModel
    public get state() {
        return this._state
    }
    private _isDirty: boolean = false;
    public get isDirty() {
        return this._isDirty
    }
    private _currentFrameNumber : number | null = 0;

    constructor(
        initialState: TViewModel,
        public stateUpdateEvent: DotvvmEvent<TViewModel>
    ) {
        this._state = initialState
        this.stateObservable = createWrappedObservable(initialState, u => this.update(u))
        this.dispatchUpdate()
    }

    public dispatchUpdate() {
        if (!this._isDirty) {
            this._isDirty = true;
            this._currentFrameNumber = window.requestAnimationFrame(this.rerender.bind(this))
        }
    }

    public doUpdateNow() {
        if (this._currentFrameNumber !== null)
            window.cancelAnimationFrame(this._currentFrameNumber);
        this.rerender(performance.now());
    }

    private startTime: number | null = null
    private rerender(time: number) {
        if (this.startTime === null) this.startTime = time
        const realStart = performance.now()
        this._isDirty = false

        this.stateUpdateEvent.trigger(this._state)
        isViewModelUpdating = true
        ko.delaySync.pause()
        try {
            this.stateObservable[notifySymbol](this._state)
        } finally {
            isViewModelUpdating = false
            ko.delaySync.resume()
        }
        // console.log("New state dispatched, t = ", performance.now() - time, "; t_cpu = ", performance.now() - realStart)
    }

    public setState(newState: TViewModel) {
        if (newState == null) throw new Error("State can't be null or undefined.")
        if (newState == this._state) return
        this.dispatchUpdate();
        return this._state = newState
    }

    public update(updater: StateUpdate<TViewModel>) {
        return this.setState(updater(this._state))
    }
}

class FakeObservableObject<T extends object> implements UpdatableObjectExtensions<T> {
    public [currentStateSymbol]: T
    public [updateSymbol]: UpdateDispatcher<T>
    public [notifySymbol](newValue: T) {
        console.assert(newValue)
        this[currentStateSymbol] = newValue

        const c = this[internalPropCache]
        for (const p of keys(c)) {
            const observable = c[p]
            if (observable) {
                observable[notifySymbol]((newValue as any)[p])
            }
        }
    }
    public [internalPropCache]: { [name: string]: (KnockoutObservable<any> & UpdatableObjectExtensions<any>) | null } = {}

    public [updatePropertySymbol](propName: keyof T, valUpdate: StateUpdate<any>) {
        this[updateSymbol](vm => Object.freeze({ ...vm, [propName]: valUpdate(vm[propName]) }))
    }

    constructor(initialValue: T, updater: UpdateDispatcher<T>, properties: string[]) {
        this[currentStateSymbol] = initialValue
        this[updateSymbol] = updater

        for (const p of properties) {
            this[internalPropCache][p] = null
            // TODO: remove condition, options will not exist
            if (isOptionsProperty(p)) {
                // options are not wrapped in observables
                Object.defineProperty(this, p, {
                    enumerable: true,
                    configurable: false,
                    get() {
                        return this[currentStateSymbol][p]
                    },
                    set(newVal) {
                        console.warn(`Setting ${p} is not supported, please do not do that`, newVal)
                    }
                })
            } else {
                Object.defineProperty(this, p, {
                    enumerable: true,
                    configurable: false,
                    get() {
                        const cached = this[internalPropCache][p]
                        if (cached) return cached

                        const currentState = this[currentStateSymbol]
                        const newObs = createWrappedObservable(
                            currentState[p],
                            u => this[updatePropertySymbol](p, u)
                        )

                        const options = currentState[p + "$options"]
                        if (options && options.clientExtenders) {
                            for (const e of options.clientExtenders) {
                                (ko.extenders as any)[e.name](newObs, e.parameter)
                            }
                        }

                        this[internalPropCache][p] = newObs
                        return newObs
                    }
                })
            }
        }
        Object.seal(this)
    }
}

export function unmapKnockoutObservables(viewModel: any): any {
    viewModel = ko.unwrap(viewModel)
    if (isPrimitive(viewModel)) {
        return viewModel
    }

    if (viewModel instanceof Date) {
        // return serializeDate(viewModel)
        return viewModel
    }

    // This is a bad idea as it does not register in the knockout dependency tracker and the caller is not triggered on change

    // if (currentStateSymbol in viewModel) {
    //     return viewModel[currentStateSymbol]
    // }

    if (viewModel instanceof Array) {
        return viewModel.map(unmapKnockoutObservables)
    }

    const result: any = {};
    for (const prop of keys(viewModel)) {
        const value = ko.unwrap(viewModel[prop])
        if (typeof value != "function") {
            result[prop] = unmapKnockoutObservables(value)
        }
    }
    return result
}

function createObservableObject<T extends object>(initialObject: T, update: ((updater: StateUpdate<any>) => void)) {
    const properties = keys(initialObject)

    // TODO: temporary hack until types are checked and enforced
    // adds properties that are missing but have the Prop$options defined
    const pSet = new Set(properties)
    const optionsOnlyProperties =
        properties.filter(isOptionsProperty)
                  .map(p => p.substring(0, p.length - "$options".length))
                  .filter(p => !pSet.has(p))

    return new FakeObservableObject(initialObject, update, properties.concat(optionsOnlyProperties)) as FakeObservableObject<T> & DeepKnockoutWrappedObject<T>
}

function type(o: any) {
    const k = keys(o)
    k.sort()
    return k.join("|")
}

function createWrappedObservable<T>(initialValue: T, updater: UpdateDispatcher<T>): DeepKnockoutWrapped<T> {

    let isUpdating = false

    const rr = initialValue instanceof Array ? ko.observableArray() : ko.observable() as any
    rr[updateSymbol] = updater

    let updatedObservable = false

    rr.subscribe((newVal: any) => {
        if (isUpdating) { return }
        updatedObservable = true
        updater(_ => unmapKnockoutObservables(newVal))
    })

    function notify(newVal: any) {
        const currentValue = rr[currentStateSymbol]
        if (newVal === currentValue) { return }
        rr[currentStateSymbol] = newVal
        const observableWasSetFromOutside = updatedObservable
        updatedObservable = false

        let newContents
        const oldContents = rr.peek()
        if (isPrimitive(newVal) || newVal instanceof Date) {
            // primitive value
            newContents = newVal
        }
        else if (newVal instanceof Array) {
            extendToObservableArrayIfRequired(rr)

            // when the observable is updated from the outside, we have to rebuild it to make sure that it contains
            // notifiable observables
            // otherwise, we want to skip the big update whenever possible - Knockout tends to update everything in the DOM when
            // we update the observableArray
            const skipUpdate = !observableWasSetFromOutside && oldContents instanceof Array && oldContents.length == newVal.length

            if (!skipUpdate) {
                // take at most newVal.length from the old value
                newContents = oldContents instanceof Array ? oldContents.slice(0, newVal.length) : []
                // then append (potential) new values into the array
                for (let index = 0; index < newVal.length; index++) {
                    if (newContents[index] && newContents[index][notifySymbol as any]) {
                        continue
                    }
                    if (newContents[index]) {
                        // TODO: remove eventually
                        console.warn(`Replacing old knockout observable with a new one, just because it is not created by DotVVM. Please do not assign objects into the knockout tree directly. The object is `, unmapKnockoutObservables(newContents[index]))
                    }
                    const indexForClosure = index
                    newContents[index] = createWrappedObservable(newVal[index], update => updater((viewModelArray: any) => {
                        const newElement = update(viewModelArray![indexForClosure])
                        const newArray = createArray(viewModelArray!)
                        newArray[indexForClosure] = newElement
                        return Object.freeze(newArray) as any
                    }))
                }
            }
            else {
                newContents = oldContents
            }

            // notify child objects
            for (let index = 0; index < newContents.length; index++) {
                newContents[index][notifySymbol as any](newVal[index])
            }

            if (skipUpdate) {
                return
            }
        }
        else if (oldContents && oldContents[notifySymbol] && type(currentValue) == type(newVal)) {
            // smart object, supports the notification by itself
            oldContents[notifySymbol as any](newVal)

            // don't update the observable itself
            return
        }
        else {
            // create new object and replace

            console.debug("Creating new KO object for", newVal)
            newContents = createObservableObject(newVal, updater)
        }

        try {
            isUpdating = true
            rr(newContents)
        }
        finally {
            isUpdating = false
        }
    }

    rr[notifySymbol] = notify
    notify(initialValue)
    return rr
}
