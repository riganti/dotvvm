import * as eventHub from './eventHub';
import { deserialize } from '../serialization/deserialize';
import { logError, logWarning } from '../utils/logging';

type ApiComputed<T> =
    KnockoutObservable<T | null> & {
        refreshValue: (throwOnError?: boolean) => PromiseLike<any> | undefined
    };

type Result<T> =
    { type: 'error', error: any } |
    { type: 'result', result: T };

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
    const cache = element ? ((<any> element)["apiCachedValues"] || ((<any> element)["apiCachedValues"] = {})) : cachedValues;
    const cachedValue = cache[sharingKeyValue] || (cache[sharingKeyValue] = ko.observable<any>(null));

    const load: () => Result<PromiseLike<any>> = () => {
        try {
            const result: PromiseLike<any> = Promise.resolve(ko.ignoreDependencies(callback));
            return { type: 'result', result: result.then((val) => {
                if (val) {
                    cachedValue(ko.unwrap(deserialize(val)));
                    cachedValue.notifySubscribers();
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

    const cmp = <ApiComputed<T>> <any> ko.pureComputed(() => cachedValue());

    cmp.refreshValue = (throwOnError) => {
        let promise: Result<PromiseLike<any>> = <any> cachedValue["promise"];
        if (!cachedValue["isLoading"]) {
            cachedValue["isLoading"] = true;
            promise = load();
            cachedValue["promise"] = promise;
        }
        if (promise.type == 'error') {
            cachedValue["isLoading"] = false;
            if (throwOnError) {
                throw promise.error;
            } else {
                return;
            }
        } else {
            promise.result.then(p => cachedValue["isLoading"] = false, p => cachedValue["isLoading"] = false);
            return promise.result;
        }
    };
    if (!cachedValue.peek()) {
        cmp.refreshValue();
    }
    ko.computed(() => refreshTriggers(args).map(f => typeof f == "string" ? eventHub.get(f)() : f())).subscribe(p => cmp.refreshValue());
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
