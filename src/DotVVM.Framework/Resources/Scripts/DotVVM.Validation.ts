/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout.mapper/knockout.mapper.d.ts" />

class DotvvmValidationContext { 
    constructor(public valueToValidate: any, public parentViewModel: any, public parameters: any[]) {
    }
}

class DotvvmValidatorBase {
    public isValid(context: DotvvmValidationContext): boolean {
        return false;
    }
    public isEmpty(value: string): boolean {
        return value == null || value.trim() == "";
    }
}

class DotvvmRequiredValidator extends DotvvmValidatorBase {
    public isValid(context: DotvvmValidationContext): boolean {
        var value = context.valueToValidate;
        return !this.isEmpty(value);
    }
}
class DotvvmRegularExpressionValidator extends DotvvmValidatorBase {
    public isValid(context: DotvvmValidationContext): boolean {
        var value = context.valueToValidate;
        var expr = context.parameters[0];
        return this.isEmpty(value) || new RegExp(expr).test(value);
    }
}

class ValidationError {
    public errorMessage = ko.observable("");
    public isValid = ko.computed(() => this.errorMessage());
    
    constructor(public targetObservable: KnockoutObservable<any>) {
    }

    public static getOrCreate(targetObservable: KnockoutObservable<any>): ValidationError {
        if (!targetObservable["validationError"]) {
            targetObservable["validationError"] = new ValidationError(targetObservable);
        }
        return <ValidationError>targetObservable["validationError"];
    }
}

interface IDotvvmValidationRules {
    [name: string]: DotvvmValidatorBase;
}
interface IDotvvmValidationElementUpdateFunctions {
    [name: string]: (element: any, errorMessage: string, options: any) => void;
}

class DotvvmValidation
{
    public rules: IDotvvmValidationRules = {
        "required": new DotvvmRequiredValidator(),
        "regularExpression": new DotvvmRegularExpressionValidator()
        //"numeric": new DotvvmNumericValidator(),
        //"datetime": new DotvvmDateTimeValidator(),
        //"range": new DotvvmRangeValidator()
    }

    public errors = ko.observableArray<ValidationError>([]);

    public elementUpdateFunctions: IDotvvmValidationElementUpdateFunctions = {
        
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
        addCssClass(element: HTMLElement, errorMessage: string, options: any) {
            var cssClass = (options && options.cssClass) ? options.cssClass : "field-validation-error";
            if (errorMessage) {
                element.className += " " + cssClass;
            } else {
                element.className = element.className.split(' ').filter(c => c != cssClass).join(' ');
            }
        },

        // displays the error message
        displayErrorMessage(element: any, errorMessage: string, options: any) {
            element[element.innerText ? "innerText" : "textContent"] = errorMessage;
        }
    }

    /// Validates the specified view model
    public validateViewModel(viewModel: any) {
        if (!viewModel || !viewModel.$type || !dotvvm.viewModels.root.validationRules) return;

        // find validation rules
        var type = ko.unwrap(viewModel.$type);
        if (!type) return;
        var rulesForType = dotvvm.viewModels.root.validationRules[type];
        if (!rulesForType) return;

        // validate all properties
        for (var property in viewModel) {
            if (!viewModel.hasOwnProperty(property) || property.indexOf("$") >= 0) continue;

            var viewModelProperty = viewModel[property];
            if (!viewModelProperty || !ko.isObservable(viewModelProperty)) continue;

            var value = viewModel[property]();

            // run validation rules
            if (rulesForType.hasOwnProperty(property)) {
                this.validateProperty(viewModel, viewModelProperty, value, rulesForType[property]);
            }

            if (value) {
                if (Array.isArray(value))
                {
                    // handle collections
                    for (var i = 0; i < value.length; i++) {
                        this.validateViewModel(value[i]);
                    }
                }
                else if (value.$type) {
                    // handle nested objects
                    this.validateViewModel(value);
                }
            }
        }
    }

    /// Validates the specified property in the viewModel
    public validateProperty(viewModel: any, property: KnockoutObservable<any>, value: any, rulesForProperty: any[]) {
        for (var i = 0; i < rulesForProperty.length; i++) {
            // validate the rules
            var rule = rulesForProperty[i];
            var ruleTemplate = this.rules[rule.ruleName];
            var context = new DotvvmValidationContext(value, viewModel, rule.parameters);

            var validationError = ValidationError.getOrCreate(property);
            if (!ruleTemplate.isValid(context)) {
                // add error message
                validationError.errorMessage(rule.errorMessage);
                this.addValidationError(viewModel, validationError);
            } else {
                // remove
                this.removeValidationError(viewModel, validationError);
                validationError.errorMessage("");
            }
        }
    }

    // clears validation errors
    public clearValidationErrors() {
        var errors = this.errors();
        for (var i = 0; i < errors.length; i++) {
            errors[i].errorMessage("");
        }
        this.errors.removeAll();
    }

    // merge validation rules
    public mergeValidationRules(args: DotvvmAfterPostBackEventArgs) {
        if (args.serverResponseObject.validationRules) {
            var existingRules = dotvvm.viewModels[args.viewModelName].validationRules;

            for (var type in args.serverResponseObject) {
                if (!args.serverResponseObject.hasOwnProperty(type)) continue;
                existingRules[type] = args.serverResponseObject[type];
            }
        }
    }

    // shows the validation errors from server
    public showValidationErrorsFromServer(args: DotvvmAfterPostBackEventArgs) {
        // resolve validation target
        var context = ko.contextFor(args.sender);
        var validationTarget = dotvvm.evaluateOnViewModel(context, args.validationTargetPath);

        // add validation errors
        this.clearValidationErrors();
        var modelState = args.serverResponseObject.modelState;
        for (var i = 0; i < modelState.length; i++) {
            // find the observable property
            var propertyPath = modelState[i].propertyPath;
            var observable = dotvvm.evaluateOnViewModel(validationTarget, propertyPath);
            var parentPath = propertyPath.substring(0, propertyPath.lastIndexOf("."));
            var parent = parentPath ? dotvvm.evaluateOnViewModel(validationTarget, parentPath) : validationTarget;
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
        this.removeValidationError(viewModel, error);
        viewModel.$validationErrors.push(error);
        this.errors.push(error);
    }

    private removeValidationError(viewModel: any, error: ValidationError) {
        var errorMessage = error.errorMessage();
        viewModel.$validationErrors.remove(e => e.errorMessage() === errorMessage);
        this.errors.remove(error);
    }
};

// init the plugin
declare var dotvvm: DotVVM;
if (!dotvvm) {
    throw "DotVVM.js is required!";
}
dotvvm.extensions.validation = dotvvm.extensions.validation || new DotvvmValidation();

// perform the validation before postback
dotvvm.events.beforePostback.subscribe(args => {
    if (args.validationTargetPath) {
        // resolve target
        var context = ko.contextFor(args.sender);
        var validationTarget = dotvvm.evaluateOnViewModel(context, args.validationTargetPath);
        
        // validate the object
        dotvvm.extensions.validation.clearValidationErrors();
        dotvvm.extensions.validation.validateViewModel(validationTarget);
        if (dotvvm.extensions.validation.errors().length > 0) {
            args.cancel = true;
            args.clientValidationFailed = true;
        }
    }
});

dotvvm.events.afterPostback.subscribe(args => {
    if (!args.wasInterrupted && args.serverResponseObject) {
        if (args.serverResponseObject.action === "successfulCommand") {
            // merge validation rules from postback with those we already have (required when a new type appears in the view model)
            dotvvm.extensions.validation.mergeValidationRules(args);
            args.isHandled = true;
        } else if (args.serverResponseObject.action === "validationErrors") {
            // apply validation errors from server
            dotvvm.extensions.validation.showValidationErrorsFromServer(args);
            args.isHandled = true;
        }
    }
});

// add knockout binding handler
ko.bindingHandlers["dotvvmalidation"] = {
    init (element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
        var observableProperty = valueAccessor();
        if (ko.isObservable(observableProperty)) {
            // try to get the options
            var options = allBindingsAccessor.get("dotvvmalidationOptions");
            var mode = (options && options.mode) ? options.mode : "addCssClass";
            var updateFunction = dotvvm.extensions.validation.elementUpdateFunctions[mode];

            // subscribe to the observable property changes
            var validationError = ValidationError.getOrCreate(observableProperty);
            validationError.errorMessage.subscribe(newValue => updateFunction(element, newValue, options));
            updateFunction(element, validationError.errorMessage(), options);
        }
    }
};


