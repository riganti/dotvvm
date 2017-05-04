/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="DotVVM.ts" />

type ApiComputed<T> = KnockoutObservable<T | null> & { refreshValue: () => void; }
interface DotVVM {
    invokeApiFn<T>(callback: () => PromiseLike<T>): ApiComputed<T>;
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

(function () {

    let cachedValues: { [key: string]: KnockoutObservable<any> } = {};
    DotVVM.prototype.invokeApiFn = function <T>(callback: () => PromiseLike<T>, refreshTriggers: KnockoutObservable<any>[] = [], commandId = callback.toString()) {
        let cachedValue = cachedValues[commandId] || (cachedValues[commandId] = ko.observable<any>(null));

        const load = () => {
            var result = window["Promise"].resolve(ko.ignoreDependencies(callback));
            result.then((val) => {
                if (val) {
                    cachedValue(val);
                }
            }, console.warn);
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
        if (!cachedValue.peek()) load();
        ko.computed(() => refreshTriggers.map(ko.unwrap)).subscribe(p => cmp.refreshValue());
        return cmp;
    }
    DotVVM.prototype.api = {}
    
    DotVVM.prototype.eventHub = new DotvvmEventHub();
}());