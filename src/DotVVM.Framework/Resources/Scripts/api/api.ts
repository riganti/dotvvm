import * as eventHub from './eventHub';
import { deserialize } from '../serialization/deserialize';
import { logError, logWarning } from '../utils/logging';
import { StateManager, unmapKnockoutObservables } from '../state-manager';
import { DotvvmEvent } from '../events';
import { callViewModuleCommand } from '../viewModules/viewModuleManager';

type ApiComputed<T> =
    KnockoutObservable<T | null> & {
        refreshValue: (throwOnError?: boolean) => PromiseLike<any> | undefined
    };

type Result<T> =
    { type: 'error', error: any } |
    { type: 'result', result: T };

type CachedValue = {
    stateManager?: StateManager<any>
    isLoading?: boolean
    promise?: Result<PromiseLike<any>>
}

type Cache = { [k: string]: CachedValue }

const cachedValues: {
    [key: string]: KnockoutObservable<any>
} = {};

export function invoke<T>(
    target: any,
    methodName: string,
    argsProvider: () => any[],
    refreshTriggers: (args: any[]) => Array<KnockoutObservable<any> | string>,
    notifyTriggers: (args: any[]) => string[],
    element: HTMLElement,
    sharingKeyProvider: (args: any[]) => string[]
): ApiComputed<T> {

    const args = ko.ignoreDependencies(argsProvider);
    const callback = () => target[methodName].apply(target, args);

    // the function gets re-evaluated when the observable changes - thus we need to cache the values
    // GET requests can be cached globally, POST and other request must be cached on per-element scope
    const sharingKeyValue = methodName + ":" + sharingKeyProvider(args);
    const cache: Cache =
        element ? ((<any> element)["apiCachedValues"] || ((<any> element)["apiCachedValues"] = {}))
                : cachedValues;
    const cachedValue = cache[sharingKeyValue] || (cache[sharingKeyValue] = {});

    const isNew = cachedValue.stateManager == null
    if (cachedValue.stateManager == null)
    {
        const updateEvent = new DotvvmEvent("apiObject.newState")
        cachedValue.stateManager = new StateManager<any>({ data: null, $type: { type: "dynamic" } }, updateEvent)
    }
    const stateManager: StateManager<any> = cachedValue.stateManager

    const load: () => Result<PromiseLike<any>> = () => {
        try {
            const result: PromiseLike<any> = Promise.resolve(ko.ignoreDependencies(callback));
            return { type: 'result', result: result.then((val) => {
                if (val) {
                    const s = stateManager.setState({ data: unmapKnockoutObservables(val) });
                    console.log("loaded API data: ", s)
                }
                for (const t of notifyTriggers(args)) {
                    eventHub.notify(t);
                }
                return val;
            }, e => logWarning("rest-api", e)) };
        } catch (e) {
            logWarning("rest-api", e);
            return { type: 'error', error: e };
        }
    };


    function refreshValue(throwOnError?: boolean) {
        let promise = cachedValue.promise!;
        if (!cachedValue.isLoading) {
            cachedValue.isLoading = true;
            promise = load();
            cachedValue.promise = promise;
        }
        if (promise.type == 'error') {
            cachedValue.isLoading = false;
            if (throwOnError) {
                throw promise.error;
            } else {
                return;
            }
        } else {
            promise.result.then(p => cachedValue.isLoading = false, err => {
                cachedValue.isLoading = false
                logWarning("rest-api", err)
            });
            return promise.result;
        }
    }
    if (isNew) {
        refreshValue();
    }
    ko.computed(() =>
        refreshTriggers(args).map(trigger => typeof trigger == "string" ? eventHub.get(trigger)() : trigger())
    )
        .subscribe(_ => refreshValue());


    const cmp = <ApiComputed<T>> <any> ko.pureComputed(() => stateManager.stateObservable().data());
    cmp.refreshValue = refreshValue
    cmp.subscribe(d => console.log("new data yeye", d))
    stateManager.stateUpdateEvent.subscribe(args => console.log("new data event", args))
    return cmp;
}

export function refreshOn<T>(
    value: ApiComputed<T>,
    watch: KnockoutObservable<any>
): ApiComputed<T> {
    if (typeof value.refreshValue != "function") {
        logError("rest-api", `The object is not refreshable.`);
    }
    watch.subscribe(() => {
        if (typeof value.refreshValue != "function") {
            logError("rest-api", `The object is not refreshable.`);
        }
        value.refreshValue();
    });
    return value;
}
