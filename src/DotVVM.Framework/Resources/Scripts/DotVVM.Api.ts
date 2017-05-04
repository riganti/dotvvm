/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="DotVVM.ts" />

type ApiComputed<T> = KnockoutObservable<T | null> & { refreshValue: () => void; }
interface DotVVM {
    invokeApiFn<T>(callback: () => PromiseLike<T>): ApiComputed<T>;
    api: { [name: string]: any }
}
DotVVM.prototype.invokeApiFn = function <T>(callback: () => PromiseLike<T>, refreshTriggers: KnockoutObservable<any>[] = []) {
    let cachedValue = ko.observable<T | null>(null);

    const load = () => {
        var result = window["Promise"].resolve(ko.ignoreDependencies(callback));
        result.then((val) => {
            if (val) {
                cachedValue(val);
            }
        }, console.warn);
    };

    const cmp = <ApiComputed<T>><any>ko.pureComputed(() => cachedValue());

    let isLoading = false;
    cmp.refreshValue = () => {
        if (isLoading) return;
        isLoading = true;
        setTimeout(() => {
            isLoading = false;
            load();
        }, 10);
    };
    load();
    ko.computed(() => refreshTriggers.map(ko.unwrap)).subscribe(p => cmp.refreshValue());
    return cmp;
}
DotVVM.prototype.api = {}
