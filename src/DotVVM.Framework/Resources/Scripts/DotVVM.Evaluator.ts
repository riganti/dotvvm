class DotvvmEvaluator {

    public evaluateOnViewModel(context, expression) {
        var result;
        if (context && context.$data) {
            result = eval("(function ($context) { with($context) { with ($data) { return " + expression + "; } } })")(context);
        } else {
            result = eval("(function ($context) { with($context) { return " + expression + "; } })")(context);
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

    public buildClientId(element: HTMLElement, fragments: any[]) {
        var id = "";
        for (var i = 0; i < fragments.length; i++) {
            if (id.length > 0) {
                id += "_";
            }
            id += ko.unwrap(fragments[i]);
        }
        return id;
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

}