/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="DotVVM.ts" />

type ApiComputed<T> = KnockoutObservable<T | null> & { refreshValue: () => void; }
interface DotVVM {
    invokeApiFn<T>(callback: () => PromiseLike<T>): ApiComputed<T>;
    apiRefreshOn<T>(value: KnockoutObservable<T>, refreshOn: KnockoutObservable<any>) : KnockoutObservable<T>;
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
    var auth = sessionStorage.getItem("someAuth") || (() => {
        var a = prompt("You credentials for " + (input["url"] || input)) || "";
        sessionStorage.setItem("someAuth", a);
        return a;
    })();
    if (init == null) init = {}
    if (init.headers == null) init.headers = {};
    if (init.headers['Authorization'] == null) init.headers["Authorization"] = 'Basic ' + btoa(auth);
    return window.fetch(input, init);
}

(function () {

    let cachedValues: { [key: string]: KnockoutObservable<any> } = {};
    DotVVM.prototype.invokeApiFn = function <T>(callback: () => PromiseLike<T>, refreshTriggers: (KnockoutObservable<any> | string)[] = [], notifyTriggers: string[] = [], commandId = callback.toString()) {
        let cachedValue = cachedValues[commandId] || (cachedValues[commandId] = ko.observable<any>(null));

        const load = () => {
            try {
                var result = window["Promise"].resolve(ko.ignoreDependencies(callback));
                result.then((val) => {
                    if (val) {
                        dotvvm.serialization.deserialize(val, cachedValue);
                        cachedValue.notifySubscribers();
                    }
                    for (var t of notifyTriggers)
                        dotvvm.eventHub.notify(t);
                }, console.warn);
            }
            catch (e) {
                console.warn(e);
            }
        };

        const cmp = <ApiComputed<T>><any>ko.pureComputed(() => cachedValue());

        cmp.refreshValue = () => {
            if (cachedValue["isLoading"]) return;
            cachedValue["isLoading"] = true;
            setTimeout(() => {
                cachedValue["isLoading"] = false;
                load();
            }, 10);
        };
        if (!cachedValue.peek()) cmp.refreshValue();
        ko.computed(() => refreshTriggers.map(f => typeof f == "string" ? dotvvm.eventHub.get(f)() : f())).subscribe(p => cmp.refreshValue());
        return cmp;
    }
    DotVVM.prototype.apiRefreshOn = function <T>(value: KnockoutObservable<T> & { refreshValue? : () => void }, refreshOn: KnockoutObservable<any>) {
        refreshOn.subscribe(() => value.refreshValue && value.refreshValue())
        return value;
    }
    DotVVM.prototype.api = {}
    
    DotVVM.prototype.eventHub = new DotvvmEventHub();
}());