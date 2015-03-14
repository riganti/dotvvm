/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout.mapper/knockout.mapper.d.ts" />
/// <reference path="Redwood.ts" />

class RedwoodValidationContext {
    constructor(public valueToValidate: any, public parentViewModel: any, public parameters: any[], public viewModelName: string, public path: string[], public constraints: ValidationConstraints) {
    }
}

class RedwoodValidatorBase {
    public isValid(context: RedwoodValidationContext): boolean {
        return false;
    }
    public isEmpty(value: string): boolean {
        return value == null || /^\s*$/.test(value);
    }
    public handlesConstraints: boolean = false;
}

class RedwoodRequiredValidator extends RedwoodValidatorBase {
    public isValid(context: RedwoodValidationContext): boolean {
        var value = context.valueToValidate;
        return !this.isEmpty(value);
    }
}
class RedwoodRegularExpressionValidator extends RedwoodValidatorBase {
    public isValid(context: RedwoodValidationContext): boolean {
        var value = context.valueToValidate;
        var expr = context.parameters[0];
        return this.isEmpty(value) || new RegExp(expr).test(value);
    }
}
class RedwoodPropertyValidator extends RedwoodValidatorBase {
    constructor(public validation: RedwoodValidation) { super(); this.handlesConstraints = true; }
    isValid(context: RedwoodValidationContext) {
        var val = context.valueToValidate;
        var type = context.parameters[0];
        return val == null || this.validation.validateTypedObject(val, type, context.viewModelName, context.path, context.constraints);
    }
}
class RedwoodCollectionValidator extends RedwoodValidatorBase {
    constructor(public validation: RedwoodValidation) { super(); this.handlesConstraints = true; }
    isValid(context: RedwoodValidationContext) {
        var col = <Array<any>>context.valueToValidate;
        var type = context.parameters[0];
        return !col || col.every((item, index) => this.validation.validateTypedObject(item, type, context.viewModelName, context.path.concat(["[" + index + "]"]), context.constraints));
    }
}

class ValidationError {
    public errorMessage = ko.observable("");
    public isValid = ko.computed(() => !!this.errorMessage());

    constructor(public targetObservable: KnockoutObservable<any>) {
    }

    public static getOrCreate(targetObservable: KnockoutObservable<any>): ValidationError {
        if (!targetObservable["validationError"]) {
            targetObservable["validationError"] = new ValidationError(targetObservable);
        }
        return <ValidationError>targetObservable["validationError"];
    }
}

interface RedwoodExtensions {
    validation?: RedwoodValidation;
}
interface ValidationRule {
    propertyName: string;
    ruleName: string;
    errorMessage: string;
    parameters: any[];
    groups: string;
}
interface RedwoodViewModel {
    validationRules?: {
        rootRules: Array<ValidationRule>;
        types: { [name: string]: Array<ValidationRule> };
        actionGroups: { [name: string]: Array<string> }
    }
}
interface IRedwoodValidationRules {
    [name: string]: RedwoodValidatorBase;
}
interface IRedwoodValidationElementUpdateFunctions {
    [name: string]: (element: any, errorMessage: string, options: any) => void;
}

class RedwoodValidation {
    public rules: IRedwoodValidationRules = {
        "required": new RedwoodRequiredValidator(),
        "regularExpression": new RedwoodRegularExpressionValidator(),
        //"numeric": new RedwoodNumericValidator(),
        //"datetime": new RedwoodDateTimeValidator(),
        //"range": new RedwoodRangeValidator()

        "validate": new RedwoodPropertyValidator(this),
        "collection": new RedwoodCollectionValidator(this),
    }

    public errors = ko.observableArray<ValidationError>([]);

    public elementUpdateFunctions: IRedwoodValidationElementUpdateFunctions = {
        
        // shows the element when it is not valid
        hideWhenValid(element: any, errorMessage: string, options: any) {
            if (errorMessage) {
                element.style.display = "";
                element.title = "";
            } else {
                element.style.display = "none";
                element.title = errorMessage;
            }
        },

        // adds a CSS class when the element is not valid
        addCssClass(element: any, errorMessage: string, options: any) {
            var cssClass = (options && options.cssClass) ? options.cssClass : "field-validation-error";
            if (errorMessage) {
                element.className += " " + cssClass;
            } else {
                element.className = element.className.replace(new RegExp("\\b" + cssClass + "\\b", "g"), "");
            }
        },

        // displays the error message
        displayErrorMessage(element: any, errorMessage: string, options: any) {
            element[element.innerText ? "innerText" : "textContent"] = errorMessage;
        }
    }

    /**
    * Validates the specified view model
    * @param viewModel viewModel for validation
    * @param name of viewModel root (default "root")
    * @param groups array of active groups (default is only group '*')
    */
    public validateViewModel(viewModel: any, viewModelName: string = "root", path: string[] = [], constraints = new ValidationConstraints(["*"], [])) {

        var rules = redwood.viewModels[viewModelName].validationRules.rootRules;
        rules.forEach(rule => {
            if (!constraints.shouldValidate(rule, path)) return;

            var viewModelProperty = viewModel[rule.propertyName];
            if (!viewModelProperty || !ko.isObservable(viewModelProperty)) return;

            this.validateProperty(viewModel, viewModelProperty, rule, viewModelName, path.concat(rule.propertyName), constraints);
        });
    }

    public validateTypedObject(obj: any, type: string, viewModelName: string, path: string[], constraints: ValidationConstraints): boolean {
        if (!obj) return;
        if (!type) return;
        var ecount = this.errors.length;
        var rulesForType = redwood.viewModels[viewModelName].validationRules.types[type];
        if (!rulesForType) return;

        // validate all properties
        rulesForType.forEach(rule => {
            if (!constraints.shouldValidate(rule, path)) return;
            var value;
            if (rule.propertyName) {
                if (!obj.hasOwnProperty(rule.propertyName)) return;
                value = obj[rule.propertyName];
            }
            else value = obj;

            this.validateProperty(obj, value, rule, viewModelName, path.concat([rule.propertyName]), constraints);
        });
        return ecount == this.errors.length;
    }

    /**
    * Validates the specified property in the viewModel
    */
    public validateProperty(viewModel: any, property: any, rule: ValidationRule, viewModelName: string, path: string[], constraints: ValidationConstraints) {
        var ruleTemplate = this.rules[rule.ruleName];
        var context = new RedwoodValidationContext(ko.isObservable(property) ? property() : property, viewModel, rule.parameters, viewModelName, path, constraints);

        var validationError = ValidationError.getOrCreate(property);
        viewModel.$validationErrors.remove(validationError);
        this.errors.remove(validationError);
        if (!ruleTemplate.isValid(context)) {
            // add error message
            validationError.errorMessage(rule.errorMessage);
            this.addValidationError(viewModel, validationError);
        } else {
            // remove
            validationError.errorMessage("");
        }
    }

    public clearValidationErrors(validationTarget: any) {
        if (!validationTarget.$validationErrors || !ko.isObservable(validationTarget.$validationErrors)) return;
        var errors = validationTarget.$validationErrors();
        for (var i = 0; i < errors.length; i++) {
            errors[i].errorMessage("");
        }
        this.errors.removeAll(errors);
    }

    public mergeValidationRules(args: RedwoodAfterPostBackEventArgs) {
        // this.clearValidationErrors(args.viewModel);
        if (args.serverResponseObject.validationRules) {
            var existingRules = redwood.viewModels[args.viewModelName].validationRules;

            for (var type in args.serverResponseObject) {
                if (!args.serverResponseObject.hasOwnProperty(type)) continue;
                existingRules[type] = args.serverResponseObject[type];
            }
        }
    }

    public showValidationErrorsFromServer(args: RedwoodAfterPostBackEventArgs) {
        // resolve validation target
        var context = ko.contextFor(args.sender);
        var validationTarget = redwood.evaluateOnViewModel(context, args.validationTargetPath);

        // add validation errors
        this.clearValidationErrors(redwood.evaluateOnViewModel(ko.contextFor(args.sender), args.validationTargetPath));
        var modelState = args.serverResponseObject.modelState;
        for (var i = 0; i < modelState.length; i++) {
            // find the observable property
            var propertyPath = modelState[i].propertyPath;
            var observable = redwood.evaluateOnViewModel(validationTarget, propertyPath);
            var parent = redwood.evaluateOnViewModel(context, propertyPath.substring(0, propertyPath.lastIndexOf(".")) || "$data");
            if (!ko.isObservable(observable) || !parent || !parent.$validationErrors) {
                throw "Invalid validation path!";
            }

            // add the error to appropriate collections
            var error = ValidationError.getOrCreate(observable);
            error.errorMessage(modelState[i].errorMessage);
            this.addValidationError(parent, error);
        }
    }

    private addValidationError(viewModel: any, error: ValidationError) {
        if (viewModel.$validationErrors.indexOf(error) < 0) {
            viewModel.$validationErrors.push(error);
        }
        this.errors.push(error);
    }

    private removeValidationError(viewModel: any, error: ValidationError) {
        viewModel.$validationErrors.remove(error);
        this.errors.remove(error);
    }

    static findActionNameInCommand(command: string): string {
        if (!command) return "";
        var rgx = /([A-Za-z_]\w*)\(.*/;
        var match = rgx.exec(command);
        if (match) {
            return match[1];
        }
        return "";
    }
};

class ValidationConstraints {
    protected activeGroups: {
        [name: string]: boolean
    } = {};
    constructor(activeGroups: string[], protected pathPrefix: string[]) {
        this.activeGroups["**"] = true;
        if (activeGroups) activeGroups.forEach(g => this.activeGroups[g] = true);
        else this.activeGroups["*"] = true;
    }
    shouldValidate(rule: ValidationRule, path: string[]): boolean {
        return redwood.extensions.validation.rules[rule.ruleName].handlesConstraints ||
            (this.matchGroups(rule.groups) && this.matchPathPrefix(path));
    }

    protected matchPathPrefix(path: string[]): boolean {
        return path.length >= this.pathPrefix.length && this.pathPrefix.every((val, i) => path[i] == val);
    }

    protected matchGroups(groupString: string): boolean {
        if (groupString == null || groupString == "") groupString = "*";

        // TODO: accept some more complicated group strings
        var s = groupString.split(",");
        return s.some(g => this.activeGroups[g]);
    }
}

// init the plugin
declare var redwood: Redwood;
if (!redwood) {
    throw "Redwood.js is required!";
}
redwood.extensions.validation = redwood.extensions.validation || new RedwoodValidation();

// perform the validation before postback
redwood.events.beforePostback.subscribe(args => {
    if (args.validationTargetPath) {

        var actionName = RedwoodValidation.findActionNameInCommand(args.command);
        var groups = redwood.viewModels[args.viewModelName].validationRules.actionGroups[actionName];
        var valPath = redwood.combinePaths(redwood.getPath(args.sender), redwood.spitPath(args.validationTargetPath));
        // validate the object
        redwood.extensions.validation.clearValidationErrors(args.viewModel);
        redwood.extensions.validation.validateViewModel(args.viewModel, args.viewModelName, [], new ValidationConstraints(groups, valPath));
        if (redwood.extensions.validation.errors().length > 0) {
            args.cancel = true;
            return true;
        }
    }
    return false;
});

redwood.events.afterPostback.subscribe(args => {
    if (args.serverResponseObject.action === "successfulCommand") {
        // merge validation rules from postback with those we already have (required when a new type appears in the view model)
        redwood.extensions.validation.mergeValidationRules(args);
        return false;
    } else if (args.serverResponseObject.action === "validationErrors") {
        // apply validation errors from server
        redwood.extensions.validation.showValidationErrorsFromServer(args);
        return true;
    }
    return false;
});

// add knockout binding handler
ko.bindingHandlers["redwoodValidation"] = {
    init(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
        var observableProperty = valueAccessor();
        if (ko.isObservable(observableProperty)) {
            // try to get the options
            var options = allBindingsAccessor.get("redwoodValidationOptions");
            var mode = (options && options.mode) ? options.mode : "addCssClass";
            var updateFunction = redwood.extensions.validation.elementUpdateFunctions[mode];

            // subscribe to the observable property changes
            var validationError = ValidationError.getOrCreate(observableProperty);
            validationError.errorMessage.subscribe(newValue => updateFunction(element, newValue, options));
            updateFunction(element, validationError.errorMessage(), options);
        }
    }
};


