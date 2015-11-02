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

class DotvvmIntRangeValidator extends DotvvmValidatorBase {
    public isValid(context: DotvvmValidationContext): boolean {
        var val = context.valueToValidate;
        var from = context.parameters[0];
        var to = context.parameters[1];
        return val % 1 === 0 && val >= from && val <= to;
    }
}

class DotvvmRangeValidator extends DotvvmValidatorBase {
    public isValid(context: DotvvmValidationContext): boolean {
        var val = context.valueToValidate;
        var from = context.parameters[0];
        var to = context.parameters[1];
        return val >= from && val <= to;
    }
}

class ValidationError {
    public errorMessage = ko.observable("");
    public isValid = ko.computed(() => this.errorMessage());
    public $targetCollection: any;

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
    [name: string]: (element: any, errorMessage: string, param: any) => void;
}

class DotvvmValidation
{
    public rules: IDotvvmValidationRules = {
        "required": new DotvvmRequiredValidator(),
        "regularExpression": new DotvvmRegularExpressionValidator(),
        "intrange": new DotvvmIntRangeValidator(),
        "range": new DotvvmRangeValidator(),
        //"numeric": new DotvvmNumericValidator(),
        //"datetime": new DotvvmDateTimeValidator(),
    }
    
    public errors = ko.observableArray([]);

    public events = {
        validationErrorsChanged: new DotvvmEvent<DotvvmEventArgs>("dotvvm.extensions.validation.events.validationErrorsChanged")
    };

    public elementUpdateFunctions: IDotvvmValidationElementUpdateFunctions = {
        
        // shows the element when it is valid
        hideWhenValid(element: any, errorMessage: string, param: any) {
            if (errorMessage) {
                element.style.display = "";
            } else {
                element.style.display = "none";
            }
        },
        
        // adds a CSS class when the element is not valid
        invalidCssClass(element: HTMLElement, errorMessage: string, param: any) {
            if (errorMessage) {
                element.className += " " + param;
            } else {
                element.className = element.className.split(' ').filter(c => c != param).join(' ');
            }
        },

        // sets the error message as the title attribute
        setToolTipText(element: any, errorMessage: string, param: any) {
            if (errorMessage) {
                element.title = errorMessage;
            } else {
                element.title = "";
            }
        },

        // displays the error message
        showErrorMessageText(element: any, errorMessage: string, param: any) {
            element[element.innerText ? "innerText" : "textContent"] = errorMessage;
        }
    }

    /// Validates the specified view model
    public validateViewModel(viewModel: any) {
        if (!viewModel || !dotvvm.viewModels['root'].validationRules) return;

        // find validation rules
        var type = ko.unwrap(viewModel.$type);
        if (!type) return;
        var rulesForType = dotvvm.viewModels['root'].validationRules[type] || {};

        // validate all properties
        for (var property in viewModel) {
            if (!viewModel.hasOwnProperty(property) || property.indexOf("$") === 0) continue;

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
            }
        }
    }
    
    // merge validation rules
    public mergeValidationRules(args: DotvvmAfterPostBackEventArgs) {
        if (args.serverResponseObject.validationRules) {
            var existingRules = dotvvm.viewModels[args.viewModelName].validationRules;
            if (typeof existingRules === "undefined") {
                dotvvm.viewModels[args.viewModelName].validationRules = {};
                existingRules = dotvvm.viewModels[args.viewModelName].validationRules;
            }
            for (var type in args.serverResponseObject) {
                if (!args.serverResponseObject.hasOwnProperty(type)) continue;
                existingRules[type] = args.serverResponseObject[type];
            }
        }
    }

    public clearValidationErrors(viewModel: any) {
        this.clearValidationErrorsCore(viewModel);

        var errors = this.errors();
        for (var i = 0; i < errors.length; i++) {
            errors[i].errorMessage("");
        }
        this.errors.removeAll();
    }

    private clearValidationErrorsCore(viewModel: any) {
        viewModel = ko.unwrap(viewModel);
        if (!viewModel || !viewModel.$type) return;
        
        // clear validation errors
        if (viewModel.$validationErrors) {
            viewModel.$validationErrors.removeAll();
        } else {
            viewModel.$validationErrors = ko.observableArray([]);
        }

        // validate all properties
        for (var property in viewModel) {
            if (!viewModel.hasOwnProperty(property) || property.indexOf("$") === 0) continue;
            var viewModelProperty = viewModel[property];
            if (!viewModelProperty || !ko.isObservable(viewModelProperty)) continue;
            var value = viewModel[property]();

            if (value) {
                if (Array.isArray(value)) {
                    // handle collections
                    for (var i = 0; i < value.length; i++) {
                        this.clearValidationErrorsCore(value[i]);
                    }
                }
                else if (value.$type) {
                    // handle nested objects
                    this.clearValidationErrorsCore(value);
                }
            }
        }
    }

    // get validation errors
    public getValidationErrors(viewModel, recursive) {
        viewModel = ko.unwrap(viewModel);
        if (!viewModel || !viewModel.$type || !viewModel.$validationErrors) return [];

        var errors = viewModel.$validationErrors();

        if (recursive) {
            // get child validation errors
            for (var property in viewModel) {
                if (!viewModel.hasOwnProperty(property) || property.indexOf("$") === 0) continue;
                var viewModelProperty = viewModel[property];
                if (!viewModelProperty || !ko.isObservable(viewModelProperty)) continue;
                var value = viewModel[property]();

                if (value) {
                    if (Array.isArray(value)) {
                        // handle collections
                        for (var i = 0; i < value.length; i++) {
                            errors = errors.concat(this.getValidationErrors(value[i], recursive));
                        }
                    } else if (value.$type) {
                        // handle nested objects
                        errors = errors.concat(this.getValidationErrors(value, recursive));
                    }
                }
            }
        }
        return errors;
    }

    // shows the validation errors from server
    public showValidationErrorsFromServer(args: DotvvmAfterPostBackEventArgs) {
        // resolve validation target
        var context = ko.contextFor(args.sender);
        var validationTarget = dotvvm.evaluateOnViewModel(context, args.validationTargetPath);
        validationTarget = ko.unwrap(validationTarget);

        // add validation errors
        this.clearValidationErrors(args.viewModel);
        var modelState = args.serverResponseObject.modelState;
        for (var i = 0; i < modelState.length; i++) {
            // find the observable property
            var propertyPath = modelState[i].propertyPath;
            var observable = dotvvm.evaluateOnViewModel(validationTarget, propertyPath);
            var parentPath = propertyPath.substring(0, propertyPath.lastIndexOf("."));
            var parent = parentPath ? dotvvm.evaluateOnViewModel(validationTarget, parentPath) : validationTarget;
            parent = ko.unwrap(parent);
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
            this.errors.push(error);
        }
    }
};
interface IDotvvmExtensions {
    validation?: DotvvmValidation;
}
interface DotvvmViewModel {
    validationRules?;
}
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
        dotvvm.extensions.validation.clearValidationErrors(args.viewModel);
        dotvvm.extensions.validation.validateViewModel(validationTarget);
        if (dotvvm.extensions.validation.errors().length > 0) {
            console.log("validation failed: postback aborted; errors: ", dotvvm.extensions.validation.errors()); 
            args.cancel = true;
            args.clientValidationFailed = true;
        }
    }
    dotvvm.extensions.validation.events.validationErrorsChanged.trigger(args);
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

    dotvvm.extensions.validation.events.validationErrorsChanged.trigger(args);
});

// add knockout binding handler
ko.bindingHandlers["dotvvmValidation"] = {
    init (element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
        var observableProperty = valueAccessor();
        if (ko.isObservable(observableProperty)) {
            // try to get the options
            var options = allBindingsAccessor.get("dotvvmValidationOptions");
            var updateFunction = function (element, errorMessage) {
                for (var option in options) {
                    if (options.hasOwnProperty(option)) {
                        dotvvm.extensions.validation.elementUpdateFunctions[option](element, errorMessage, options[option]);
                    }
                }
            }
            
            // subscribe to the observable property changes
            var validationError = ValidationError.getOrCreate(observableProperty);
            validationError.errorMessage.subscribe(newValue => updateFunction(element, newValue));
            updateFunction(element, validationError.errorMessage());
        }
    }
};

