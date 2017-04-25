/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="DotVVM.ts" />
interface DotVVM {
    invokeApiFn<T>(callback: () => T): T;
    api: { [name: string]: any }
}
DotVVM.prototype.invokeApiFn = function <T>(callback: () => T) {
    var cachedValue = ko.observable(null);

    const load = () => {
        var result = window["Promise"].resolve(callback());
        result.then((val) => {
            if (val) {
                cachedValue(val);
            }
        }, console.warn);
    };

    var cmp = ko.pureComputed(() => cachedValue());

    cmp["refreshValue"] = () => {
        load();
    };
    load();
    return cmp;
}
DotVVM.prototype.api = {}
