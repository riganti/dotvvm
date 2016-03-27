/// <reference path="typings/knockout/knockout.d.ts" />

class DotvvmValidationContext {
    constructor(public valueToValidate: any, public parentViewModel: any, public parameters: any[]) {
    }
}

class DotvvmValidatorBase {
    public isValid(context: DotvvmValidationContext): boolean {
        return false;
    }
    public isEmpty(value: string): boolean {
        return value == null || (typeof value == "string" && value.trim() == "");
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

class DotvvmNotNullValidator extends DotvvmValidatorBase {
    public isValid(context: DotvvmValidationContext) {
        return context.valueToValidate !== null && context.valueToValidate !== undefined;
    }
}

class ValidationError {

    constructor(public targetObservable: KnockoutObservable<any>, public errorMessage: string) {
    }

    public static getOrCreate(targetObservable: KnockoutObservable<any> & { validationErrors?: KnockoutObservableArray<ValidationError> }): KnockoutObservableArray<ValidationError> {
        if (!targetObservable.validationErrors) {
            targetObservable.validationErrors = ko.observableArray<ValidationError>();
        }
        return targetObservable.validationErrors;
    }

    public static isValid(observable: KnockoutObservable<any> & { validationErrors?: KnockoutObservableArray<ValidationError> }) {
        return !observable.validationErrors || observable.validationErrors.length == 0;
    }

    public static clear(observable: KnockoutObservable<any> & { validationErrors?: KnockoutObservableArray<ValidationError> }) {
        if (observable.validationErrors != null) {
            observable.validationErrors.removeAll();
        }
    } 
}
interface IDotvvmViewModelInfo {
    validationRules?: { [typeName: string]: { [propertyName: string]: IDotvvmPropertyValidationRuleInfo[] } }
}

interface IDotvvmPropertyValidationRuleInfo {
    ruleName: string;
    errorMessage: string;
    parameters: any[];
}

interface IDotvvmValidationRules {
    [name: string]: DotvvmValidatorBase;
}
interface IDotvvmValidationElementUpdateFunctions {
    [name: string]: (element: HTMLElement, errorMessages: string[], param: any) => void;
}

class DotvvmValidation {
    public rules: IDotvvmValidationRules = {
        "required": new DotvvmRequiredValidator(),
        "regularExpression": new DotvvmRegularExpressionValidator(),
        "intrange": new DotvvmIntRangeValidator(),
        "range": new DotvvmRangeValidator(),
        "notnull": new DotvvmNotNullValidator()
        //"numeric": new DotvvmNumericValidator(),
        //"datetime": new DotvvmDateTimeValidator(),
    }

    public errors = ko.observableArray<ValidationError>([]);

    public events = {
        validationErrorsChanged: new DotvvmEvent<DotvvmEventArgs>("dotvvm.validation.events.validationErrorsChanged")
    };

    public elementUpdateFunctions: IDotvvmValidationElementUpdateFunctions = {
        
        // shows the element when it is valid
        hideWhenValid(element: HTMLElement, errorMessages: string[], param: any) {
            if (errorMessages.length > 0) {
                element.style.display = "";
            } else {
                element.style.display = "none";
            }
        },
        
        // adds a CSS class when the element is not valid
        invalidCssClass(element: HTMLElement, errorMessages: string[], className: string) {
            if (errorMessages.length > 0) {
                element.className += " " + className;
            } else {
                element.className = element.className.split(' ').filter(c => c != className).join(' ');
            }
        },

        // sets the error message as the title attribute
        setToolTipText(element: HTMLElement, errorMessages: string[], param: any) {
            if (errorMessages.length > 0) {
                element.title = errorMessages.join(", ");
            } else {
                element.title = "";
            }
        },

        // displays the error message
        showErrorMessageText(element: any, errorMessages: string[], param: any) {
            element[element.innerText ? "innerText" : "textContent"] = errorMessages.join(", ");
        }
    }

    constructor(dotvvm: DotVVM) {
        // perform the validation before postback
        dotvvm.events.beforePostback.subscribe(args => {
            if (args.validationTargetPath) {
                // resolve target
                var context = ko.contextFor(args.sender);
                var validationTarget = dotvvm.evaluator.evaluateOnViewModel(context, args.validationTargetPath);
        
                // validate the object
                this.clearValidationErrors(args.viewModel);
                this.validateViewModel(validationTarget);
                if (this.errors().length > 0) {
                    console.log("Validation failed: postback aborted; errors: ", this.errors());
                    args.cancel = true;
                    args.clientValidationFailed = true;
                }
            }
            this.events.validationErrorsChanged.trigger(args);
        });

        dotvvm.events.afterPostback.subscribe(args => {
            if (!args.wasInterrupted && args.serverResponseObject) {
                if (args.serverResponseObject.action === "successfulCommand") {
                    // merge validation rules from postback with those we already have (required when a new type appears in the view model)
                    this.mergeValidationRules(args);
                    args.isHandled = true;
                } else if (args.serverResponseObject.action === "validationErrors") {
                    // apply validation errors from server
                    this.showValidationErrorsFromServer(args);
                    args.isHandled = true;
                }
            }

            this.events.validationErrorsChanged.trigger(args);
        });

        // add knockout binding handler
        ko.bindingHandlers["dotvvmValidation"] = {
            init: (element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) => {
                var observableProperty = valueAccessor();
                if (ko.isObservable(observableProperty)) {
                    // try to get the options
                    var options = allBindingsAccessor.get("dotvvmValidationOptions");
                    var updateFunction = (element, errorMessages: ValidationError[]) => {
                        for (var option in options) {
                            if (options.hasOwnProperty(option)) {
                                this.elementUpdateFunctions[option](element, errorMessages.map(v => v.errorMessage), options[option]);
                            }
                        }
                    }
            
                    // subscribe to the observable property changes
                    var validationErrors = ValidationError.getOrCreate(observableProperty);
                    validationErrors.subscribe(newValue => updateFunction(element, newValue));
                    updateFunction(element, validationErrors());
                }
            }
        };
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

            var options = viewModel[property + "$options"];
            if (options && options.type && ValidationError.isValid(viewModelProperty) && !dotvvm.serialization.validateType(value, options.type)) {
                var error = new ValidationError(viewModelProperty, `${value} is invalid value for type ${options.type}`);
                ValidationError.getOrCreate(viewModelProperty).push(error);
                this.addValidationError(viewModel, error);
            }

            if (value) {
                if (Array.isArray(value)) {
                    // handle collections
                    for (var item of value) {
                        this.validateViewModel(ko.unwrap(item));
                    }
                }
                else if (value.$type) {
                    // handle nested objects
                    this.validateViewModel(value);
                }
            }
        }
    }

    // validates the specified property in the viewModel
    public validateProperty(viewModel: any, property: KnockoutObservable<any>, value: any, rulesForProperty: IDotvvmPropertyValidationRuleInfo[]) {
        for (var rule of rulesForProperty) {
            // validate the rules
            var ruleTemplate = this.rules[rule.ruleName];
            var context = new DotvvmValidationContext(value, viewModel, rule.parameters);

            if (!ruleTemplate.isValid(context)) {
                var validationErrors = ValidationError.getOrCreate(property);
                // add error message
                var validationError = new ValidationError(property, rule.errorMessage);
                validationErrors.push(validationError);
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
            ValidationError.clear(viewModel[property]);

            var value = viewModel[property]();
            if (value) {
                if (Array.isArray(value)) {
                    // handle collections
                    for (var i = 0; i < value.length; i++) {
                        this.clearValidationErrorsCore(ko.unwrap(value[i]));
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

        var errors = viewModel.$validationErrors() as ValidationError[];

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
                        for (var item of value) {
                            errors = errors.concat(this.getValidationErrors(ko.unwrap(item), recursive));
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
        var validationTarget = dotvvm.evaluator.evaluateOnViewModel(context, args.validationTargetPath);
        validationTarget = ko.unwrap(validationTarget);

        // add validation errors
        this.clearValidationErrors(args.viewModel);
        var modelState = args.serverResponseObject.modelState;
        for (var i = 0; i < modelState.length; i++) {
            // find the observable property
            var propertyPath = modelState[i].propertyPath;
            var observable = dotvvm.evaluator.evaluateOnViewModel(validationTarget, propertyPath);
            var parentPath = propertyPath.substring(0, propertyPath.lastIndexOf("."));
            var parent = parentPath ? dotvvm.evaluator.evaluateOnViewModel(validationTarget, parentPath) : validationTarget;
            parent = ko.unwrap(parent);
            if (!ko.isObservable(observable) || !parent || !parent.$validationErrors) {
                throw "Invalid validation path!";
            }

            // add the error to appropriate collections
            var errors = ValidationError.getOrCreate(observable);
            var error = new ValidationError(observable, modelState[i].errorMessage)
            errors.push(error);
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

declare var dotvvm: DotVVM;