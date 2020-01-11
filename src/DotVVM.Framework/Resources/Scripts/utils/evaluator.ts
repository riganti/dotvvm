import { isObservableArray } from "./knockout";

export function evaluateOnViewModel(context: any, expression: string): any {
    // todo: reimplement
    let result;
    if (context && context.$data) {
        result = new Function("$context", "with($context) { with ($data) { return " + expression + "; } }")(context);
    } else {
        result = new Function("$context", "var $data=$context; with($context) { return " + expression + "; }")(context);
    }
    if (result && result.$data) {
        result = result.$data;
    }
    return result;
}

export function getDataSourceItems(viewModel: any): Array<KnockoutObservable<any>> {
    const value = ko.unwrap(viewModel);
    if (value == null) {
        return [];
    }
    return ko.unwrap(value.Items || value);
}

export function wrapObservable(func: () => any, isArray?: boolean): KnockoutComputed<any> {
    let wrapper = ko.pureComputed({
        read: () => ko.unwrap(getExpressionResult(func)),
        write: value => updateObservable(func, value)
    });

    if (isArray) {
        for (const i of ["push", "pop", "unshift", "shift", "reverse", "sort", "splice", "slice", "replace", "indexOf", "remove", "removeAll"]) {
            wrapper[i] = (...args: any) => updateObservableArray(func, i, args);
        }
        wrapper = wrapper.extend({ trackArrayChanges: true });
    }

    return wrapper.extend({ notify: "always" });
}

function updateObservable(getObservable: () => KnockoutObservable<any>, value: any) {
    const result = getExpressionResult(getObservable);

    if (!ko.isWriteableObservable(result)) {
        console.error(`Cannot write a value to ko.computed because the expression '${getObservable}' does not return a writable observable.`);
    } else {
        result(value);
    }
}

function updateObservableArray(getObservableArray: () => KnockoutObservableArray<any>, fnName: string, args: any[]) {
    const result = getExpressionResult(getObservableArray);

    if (!isObservableArray(result)) {
        console.error(`Cannot execute '${fnName}' function on ko.computed because the expression '${getObservableArray}' does not return an observable array.`);
    } else {
        result[fnName].apply(result, args);
    }
}

function getExpressionResult(func: () => any) {
    let result = func();

    if (ko.isComputed(result) && "wrappedProperty" in result) {
        result = result["wrappedProperty"](); // workaround for dotvvm_withControlProperties handler
    }

    return result;
}
