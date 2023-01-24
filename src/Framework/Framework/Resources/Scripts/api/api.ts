import * as eventHub from './eventHub';
import { deserialize } from '../serialization/deserialize';
import { logError, logWarning } from '../utils/logging';
import { StateManager, unmapKnockoutObservables } from '../state-manager';
import { DotvvmEvent } from '../events';
import { keys } from '../utils/objects';

type ApiComputed<T> =
    KnockoutComputed<T | null> & {
        refreshValue: () => PromiseLike<any>
    };

class CachedValue {
    _stateManager?: StateManager<any>
    _isLoading?: boolean
    _promise?: PromiseLike<any>
    _elements: Set<HTMLElement> = new Set()

    constructor(public _cache: Cache) {
    }

    _registerElement(element: HTMLElement, sharingKeyValue: string) {
        if (!this._elements.has(element)) {
            this._elements.add(element);

            ko.utils.domNodeDisposal.addDisposeCallback(element, () => {
                this._unregisterElement(element, sharingKeyValue);
            });
        }
    }

    _unregisterElement(element: HTMLElement, sharingKeyValue: string) {
        this._elements.delete(element);
        if (!this._elements.size) {
            delete this._cache[sharingKeyValue];
        }
    }
}

type Cache = { [k: string]: CachedValue }

const cachedValues: Cache = {};

export function invoke<T>(
    target: any,
    methodName: string,
    argsProvider: () => any[],
    refreshTriggers: (args: any[]) => Array<KnockoutObservable<any> | string>,
    notifyTriggers: (args: any[]) => string[],
    cacheElement: HTMLElement,
    sharingKeyProvider: (args: any[]) => string[],
    lifetimeElement: HTMLElement
): ApiComputed<T> {
    const cache: Cache = cacheElement ? ((<any>cacheElement)["apiCachedValues"] ??= {}) : cachedValues;
    const $type: TypeDefinition = { type: "dynamic" }

    let args: any[];
    let sharingKeyValue: string;
    let cachedValue: CachedValue;
    let stateManager = ko.observable() as KnockoutObservable<StateManager<any>>;

    function refreshArgs() {
        args = ko.ignoreDependencies(argsProvider);
        // the function gets re-evaluated when the observable changes - thus we need to cache the values
        // GET requests can be cached globally, POST and other request must be cached on per-element scope
        let oldKey = sharingKeyValue
        sharingKeyValue = methodName + ":" + sharingKeyProvider(args)
        const oldCached = cachedValue
        cachedValue = cache[sharingKeyValue] ??= new CachedValue(cache)
        if (cachedValue === oldCached) {
            return
        }

        cachedValue._registerElement(lifetimeElement, sharingKeyValue);
        if (oldCached) {
            oldCached._unregisterElement(lifetimeElement, oldKey);
        }

        if (cachedValue._stateManager == null)
        {
            const updateEvent = new DotvvmEvent("apiObject.newState")
            cachedValue._stateManager = new StateManager<any>({ data: null, $type }, updateEvent)
            reloadApi()
        }
        stateManager(cachedValue._stateManager)
    }
    
    function reloadApi(): PromiseLike<any> {
        if (!cachedValue._isLoading) {
            cachedValue._isLoading = true
            cachedValue._promise = load()
            cachedValue._promise.then(p => {
                cachedValue._isLoading = false
            }, err => {
                cachedValue._isLoading = false
                logWarning("rest-api", err)
            })
        }
        return cachedValue._promise!
    }
    async function load(): Promise<any> {
        let val = await ko.ignoreDependencies(() => target[methodName].apply(target, args))
        if (val) {
            const s = stateManager().setState({ data: unmapKnockoutObservables(deserialize(val)), $type })
            val = s.data
        }
        for (const t of notifyTriggers(args)) {
            eventHub.notify(t)
        }
        return val;
    }

    function refreshValue() {
        refreshArgs()
        return reloadApi()
    }

    refreshArgs()
    ko.computed(
            () => refreshTriggers(args).map(trigger => typeof trigger == "string" ? eventHub.get(trigger)() : trigger()),
            null,
            { disposeWhenNodeIsRemoved: lifetimeElement }
        )
        .subscribe(_ => refreshValue());    

    const cmp = <ApiComputed<T>> <any> ko.pureComputed(() => stateManager().stateObservable().data());
    cmp.refreshValue = refreshValue
    return cmp;
}

export function refreshOn<T>(
    value: ApiComputed<T>,
    watch: KnockoutObservable<any>
): ApiComputed<T> {
    if (typeof value.refreshValue != "function") {
        logError("rest-api", `The object is not refreshable.`);
    }
    const subs = watch.subscribe(() => {
        if (value.getSubscriptionsCount()) {
            value.refreshValue();
        } else {
            subs.dispose()
        }
    });
    return value;
}

export function clearApiCachedValues() {
    for (let key of keys(cachedValues)) {
        delete cachedValues[key];
    }
}
