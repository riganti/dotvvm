export function wrapObservable<T>(obj: T): KnockoutObservable<T> {
    if (!ko.isObservable(obj)) {
        return ko.observable(obj);
    }
    return <KnockoutObservable<T>> <any> obj;
}

export function wrapObservableObjectOrArray<T>(obj: T): KnockoutObservable<T> | KnockoutObservableArray<T> {
    return Array.isArray(obj)
        ? ko.observableArray(obj)
        : ko.observable(obj);
}

export function isObservableArray(target: any): boolean {
    return ko.isObservable(target) && "removeAll" in target;
}
