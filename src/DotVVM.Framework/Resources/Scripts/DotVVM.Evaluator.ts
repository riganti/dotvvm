class DotvvmEvaluator {

    public evaluateOnViewModel(context, expression) {
        var result;
        if (context && context.$data) {
            result = eval("(function ($context) { with($context) { with ($data) { return " + expression + "; } } })")(context);
        }
        else {
            result = eval("(function ($context) { var $data=$context; with($context) { return " + expression + "; } })")(context);
        }
        if (result && result.$data) {
            result = result.$data;
        }
        return result;
    }

    public evaluateOnContext(context, expression: string) {
        var startsWithProperty = false;
        for (var prop in context) {
            if (expression.indexOf(prop) === 0) {
                startsWithProperty = true;
                break;
            }
        }
        if (!startsWithProperty) expression = "$data." + expression;
        return this.evaluateOnViewModel(context, expression);
    }

    public getDataSourceItems(viewModel: any) {
        var value = ko.unwrap(viewModel);
        if (typeof value === "undefined" || value == null) return [];
        return ko.unwrap(value.Items || value);
    }

    public tryEval(func: () => any): any {
        try {
            return func();
        }
        catch (error) {
            return null;
        }
    }

    public isObservableArray(instance: any): instance is KnockoutObservableArray<any> {
        if (ko.isComputed(instance)) {
            return Array.isArray(instance.peek());
        }
        else if (ko.isObservable(instance)) {
            return "push" in instance;
        }

        return false;
    }

    public wrapObservable(func: () => any, isArray?: boolean): KnockoutComputed<any> {
        let wrapper = ko.pureComputed({
            read: () => ko.unwrap(this.getExpressionResult(func)),
            write: value => this.updateObservable(func, value)
        });

        if (isArray) {
            wrapper.push = (...args) => this.updateObservableArray(func, "push", args);
            wrapper.pop = (...args) => this.updateObservableArray(func, "pop", args);
            wrapper.unshift = (...args) => this.updateObservableArray(func, "unshift", args);
            wrapper.shift = (...args) => this.updateObservableArray(func, "shift", args);
            wrapper.reverse = (...args) => this.updateObservableArray(func, "reverse", args);
            wrapper.sort = (...args) => this.updateObservableArray(func, "sort", args);
            wrapper.splice = (...args) => this.updateObservableArray(func, "splice", args);
            wrapper.slice = (...args) => this.updateObservableArray(func, "slice", args);
            wrapper.replace = (...args) => this.updateObservableArray(func, "replace", args);
            wrapper.indexOf = (...args) => this.updateObservableArray(func, "indexOf", args);
            wrapper.remove = (...args) => this.updateObservableArray(func, "remove", args);
            wrapper.removeAll = (...args) => this.updateObservableArray(func, "removeAll", args);
            wrapper = wrapper.extend({ trackArrayChanges: true });
        }

        return wrapper.extend({ notify: "always" });
    }

    private updateObservable(getObservable: () => KnockoutObservable<any>, value) {
        const result = this.getExpressionResult(getObservable);

        if (!ko.isWriteableObservable(result)) {
            console.error(`Cannot write a value to ko.computed because the expression '${getObservable}' does not return a writable observable.`);
        }
        else {
            result(value);
        }
    }

    private updateObservableArray(getObservableArray: () => KnockoutObservableArray<any>, fnName: string, args: any[]) {
        const result = this.getExpressionResult(getObservableArray);

        if (!this.isObservableArray(result)) {
            console.error(`Cannot execute '${fnName}' function on ko.computed because the expression '${getObservableArray}' does not return an observable array.`);
        }
        else {
            result[fnName].apply(result, args);
        }
    }

    private getExpressionResult(func: () => any) {
        let result = func();

        if (ko.isComputed(result) && "wrappedProperty" in result) {
            result = result["wrappedProperty"](); // workaround for dotvvm_withControlProperties handler
        }

        return result;
    }

}
