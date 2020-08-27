/// Holds the dotvvm viewmodel

import { createArray, hasOwnProperty, symbolOrDollar, isPrimitive, keys } from "./utils/objects";
import { DotvvmEvent } from "./events";
import { serializeDate } from "./serialization/date";


const currentStateSymbol = Symbol("currentState")
const notifySymbol = Symbol("notify")

const internalPropCache = Symbol("internalPropCache")
const updateSymbol = Symbol("update")
const updatePropertySymbol = Symbol("updateProperty")

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
        ko.delaySync.pause()
        this.stateObservable[notifySymbol](this._state)
        ko.delaySync.resume()
        console.log("New state dispatched, t = ", performance.now() - time, "; t_cpu = ", performance.now() - realStart)
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
            Object.defineProperty(this, p, {
                enumerable: true,
                configurable: false,
                get() {
                    const cached = this[internalPropCache][p]
                    if (cached) return cached

                    const newObs = createWrappedObservable(
                        this[currentStateSymbol][p],
                        u => this[updatePropertySymbol](p, u)
                    )

                    this[internalPropCache][p] = newObs
                    return newObs
                }
            })
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

    if (currentStateSymbol in viewModel) {
        return viewModel[currentStateSymbol]
    }

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

    return new FakeObservableObject(initialObject, update, properties) as FakeObservableObject<T> & DeepKnockoutWrappedObject<T>
}


function createObservableArray<T>(initialValue: T[] | null, updater: UpdateDispatcher<T[] | null>): KnockoutObservableArray<any> & UpdatableObjectExtensions<T[] | null> {
    let isUpdating = false;

    const result = []
    const rr = ko.observableArray() as (KnockoutObservableArray<any> & UpdatableObjectExtensions<T[] | null>)
    rr[updateSymbol] = updater

    rr.subscribe((newVal) => {
        if (isUpdating) { return }
        updater(_ => unmapKnockoutObservables(newVal))
    })

    function notify(newVal: T[] | null) {
        const currentValue = rr[currentStateSymbol]
        if (newVal === currentValue) { return }
        if (!newVal || !currentValue || newVal.length != currentValue.length)
        {
            // must update the collection itself
            try {
                isUpdating = true;
                if (!newVal) rr(newVal);
                else {
                    const oldValue = rr.peek()
                    const result: any[] = [ ...oldValue]
                    for (let index = result.length; index < newVal.length; index++) {
                        const indexForClosure = index
                        result.push(createWrappedObservable(newVal[index], update => updater(viewModelArray => {
                            const newElement = update(viewModelArray![indexForClosure])
                            const newArray = createArray(viewModelArray!)
                            newArray[indexForClosure] = newElement
                            return Object.freeze(newArray)
                        })))
                    }
                    rr(result)
                }

            }
            finally {
                isUpdating = false;
            }
        }
        rr[currentStateSymbol] = newVal

        if (newVal)
        {
            // notify child objects
            const arr = rr.peek()
            for (let index = 0; index < arr.length; index++) {
                arr[index][notifySymbol as any](newVal[index])
            }
        }
    }

    rr[notifySymbol] = notify
    notify(initialValue)
    return rr
}

function createWrappedObservable<T>(value: T, updater: UpdateDispatcher<T>): DeepKnockoutWrapped<T> {

    if (value instanceof Array) {
        return createObservableArray(value, updater as any) as any
    }
    else {
        let isUpdating = false

        const rr = ko.observable() as any
        rr[updateSymbol] = updater

        rr.subscribe((newVal: any) => {
            if (isUpdating) { return }
            updater(_ => unmapKnockoutObservables(newVal))
        })

        function notify(newVal: T) {
            const currentValue = rr[currentStateSymbol]
            if (newVal === currentValue) { return }

            let newContents
            if (isPrimitive(newVal)) {
                // primitive value
                newContents = newVal
            } else {
                // object
                console.assert(!(newVal instanceof Array))
                console.assert(!isPrimitive(newVal))

                const oldContents = rr.peek()
                // TODO: change when type changes
                if (oldContents && oldContents[notifySymbol]) {
                    // smart object, supports the notification by itself
                    oldContents[notifySymbol as any](newVal)
                    return
                } else {
                    // create new object and replace

                    console.debug("Creating new KO object for", newVal)
                    newContents = createObservableObject(newVal as any, updater)
                }
            }

            try {
                isUpdating = true
                rr(newContents)
            } finally {
                isUpdating = false
            }
        }

        rr[notifySymbol] = notify
        notify(value)
        return rr
    }
}
