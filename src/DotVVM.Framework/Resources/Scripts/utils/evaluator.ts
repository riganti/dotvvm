import { isObservableArray } from "./knockout";
import { logError } from "./logging";

const shouldBeConvertedFromDataContext = (currentLevel: any, remainingParts: string[]): boolean => {
    if (currentLevel["$data"] == undefined) {
        return false;
    }

    return remainingParts.filter(i => i.startsWith("$")).length === 0;
};


export function evaluateValidationPath(context: any, expression: string): any {

    expression = transformExpression(expression);

    var parts = expression.split(/[/[\]]+/);
    var currentLevel = context;
    var currentPath = "";
    for (var i = 0; i < parts.length; i++) {
        if (shouldBeConvertedFromDataContext(currentLevel, parts.slice(i))) {
            currentLevel = context["$data"];
        }
        let expressionPart = parts[i];
        if (expressionPart === "")
            continue;

        var currentLevelExpanded = currentLevel instanceof Function ? currentLevel() : currentLevel;

        var nextNode = currentLevelExpanded[expressionPart];
        if (nextNode==undefined) {
            throw `Validation error could not been applied to property specified by propertyPath ${expression}. Property with name ${expressionPart} does not exist on ${currentPath}.`;
        }
        currentPath += "/"+expressionPart;
        currentLevel = nextNode;
    }

    return currentLevel;
}

export function transformExpression(expression: string) {

    if (expression === '$rawData') {
        expression = '/';
    }
    expression = expression.replace(".", "/");

    return expression;
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
        logError("evaluator", `Cannot write a value to ko.computed because the expression '${getObservable}' does not return a writable observable.`);
    } else {
        result(value);
    }
}

function updateObservableArray(getObservableArray: () => KnockoutObservableArray<any>, fnName: string, args: any[]) {
    const result = getExpressionResult(getObservableArray);

    if (!isObservableArray(result)) {
        logError("evaluator", `Cannot execute '${fnName}' function on ko.computed because the expression '${getObservableArray}' does not return an observable array.`);
    } else {
        result[fnName].apply(result, args);
    }
}

export const unwrapComputedProperty = (obs: any) =>
    ko.isComputed(obs) && "wrappedProperty" in obs ?
    obs["wrappedProperty"]() : // workaround for dotvvm-with-control-properties handler
    obs;

function getExpressionResult(func: () => any) {
    return unwrapComputedProperty(func());
}
