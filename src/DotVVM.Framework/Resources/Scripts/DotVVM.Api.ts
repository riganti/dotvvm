/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="DotVVM.ts" />

type ApiComputed<T> = KnockoutObservable<T | null> & { refreshValue: (throwOnError?: boolean) => PromiseLike<any> | undefined }
type Result<T> = { type: 'error', error: any } | { type: 'result', result: T }
interface DotVVM {
    invokeApiFn<T>(callback: () => PromiseLike<T>): ApiComputed<T>;
    apiRefreshOn<T>(value: KnockoutObservable<T>, refreshOn: KnockoutObservable<any>) : KnockoutObservable<T>;
    apiStore<T>(value: KnockoutObservable<T>, targetProperty: KnockoutObservable<any>) : KnockoutObservable<T>;
    api: { [name: string]: any };
    eventHub: DotvvmEventHub;
}
class DotvvmEventHub {
    private map: { [key: string]: KnockoutObservable<number> } = {};

    public notify(id: string) {
        if (id in this.map) this.map[id].notifySubscribers();
        else this.map[id] = ko.observable(0);
    }

    public get(id: string) {
        return this.map[id] || (this.map[id] = ko.observable(0));
    }
}

function basicAuthenticatedFetch(input: RequestInfo, init: RequestInit) {
    function requestAuth() {
        var a = prompt("You credentials for " + (input["url"] || input)) || "";
        sessionStorage.setItem("someAuth", a);
        return a;
    }
    var auth = sessionStorage.getItem("someAuth");
    if (auth != null)
    {
        if (init == null) init = {}
        if (init.headers == null) init.headers = {};
        if (init.headers['Authorization'] == null) init.headers["Authorization"] = 'Basic ' + btoa(auth);
    }
    if (init == null) init = {}
    if (!init.cache) init.cache = "no-cache";
    return window.fetch(input, init).then(response => {
        if (response.status === 401 && auth == null) {
            if (sessionStorage.getItem("someAuth") == null) requestAuth();
            return basicAuthenticatedFetch(input, init);
        } else {
            return response;
        }
    });
}

(function () {

    let cachedValues: { [key: string]: KnockoutObservable<any> } = {};
    DotVVM.prototype.invokeApiFn = function <T>(callback: () => PromiseLike<T>, refreshTriggers: (KnockoutObservable<any> | string)[] = [], notifyTriggers: string[] = [], commandId = callback.toString()) {
        let cachedValue = cachedValues[commandId] || (cachedValues[commandId] = ko.observable<any>(null));

        const load : () => Result<PromiseLike<any>> = () => {
            try {
                var result : PromiseLike<any> = window["Promise"].resolve(ko.ignoreDependencies(callback));
                return { type: 'result', result: result.then((val) => {
                    if (val) {
                        cachedValue(ko.unwrap(dotvvm.serialization.deserialize(val, cachedValue)));
                        cachedValue.notifySubscribers();
                    }
                    for (var t of notifyTriggers)
                        dotvvm.eventHub.notify(t);
                    return val;
                }, console.warn) };
            }
            catch (e) {
                console.warn(e);
                return { type: 'error', error: e };
            }
        };

        const cmp = <ApiComputed<T>><any>ko.pureComputed(() => cachedValue());

        cmp.refreshValue = (throwOnError) => {
            let promise: Result<PromiseLike<any>> = <any>cachedValue["promise"];
            if (!cachedValue["isLoading"])
            {
                cachedValue["isLoading"] = true;
                promise = load();
                cachedValue["promise"] = promise;
            }
            if (promise.type == 'error')
            {
                cachedValue["isLoading"] = false;
                if (throwOnError) throw promise.error;
                else return;
            }
            else
            {
                promise.result.then(p => cachedValue["isLoading"] = false, p => cachedValue["isLoading"] = false);
                return promise.result;
            }
        };
        if (!cachedValue.peek()) cmp.refreshValue();
        ko.computed(() => refreshTriggers.map(f => typeof f == "string" ? dotvvm.eventHub.get(f)() : f())).subscribe(p => cmp.refreshValue());
        return cmp;
    }
    DotVVM.prototype.apiRefreshOn = function <T>(value: KnockoutObservable<T> & { refreshValue? : () => void }, refreshOn: KnockoutObservable<any>) {
        if (typeof value.refreshValue != "function") console.error(`The object is not refreshable`);
        refreshOn.subscribe(() => {
            if (typeof value.refreshValue != "function") console.error(`The object is not refreshable`);
            value.refreshValue && value.refreshValue();
        });
        return value;
    }
    DotVVM.prototype.api = {}
    
    DotVVM.prototype.eventHub = new DotvvmEventHub();
}());
