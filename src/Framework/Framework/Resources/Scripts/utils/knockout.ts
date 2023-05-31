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


/** If ko.options.deferUpdates is on,
 * obs.notifySubscribers() will not invoke them if the value didn't change, because the equalityComparer is checked again when the notification runs.
 * This function removes the equalityComparer, waits for all tasks and returns it */
export function hackInvokeNotifySubscribers(obs: KnockoutObservable<any>) {
    const equalityComparer = obs.equalityComparer
    obs.equalityComparer = null! // this is equivalent to enabling notify: 'always' for the observable, we want to turn it off again after this notification
    if (obs.valueHasMutated)
        obs.valueHasMutated()
    else
        obs.notifySubscribers()
    ko.tasks.runEarly()
    obs.equalityComparer = equalityComparer
}
