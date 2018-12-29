/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="DotVVM.ts" />

class DotvvmValidationContext {
    constructor(public valueToValidate: any, public parentViewModel: any, public parameters: any[]) {
    }
}

class DotvvmValidationObservableMetadata {
    public elementsMetadata: DotvvmValidationElementMetadata[];
}
class DotvvmValidationElementMetadata {
    public element: HTMLElement;
    public dataType: string;
    public format: string;
    public domNodeDisposal: boolean;
    public elementValidationState: boolean = true;

}
class DotvvmValidatorBase {
    public isValid(context: DotvvmValidationContext, property: KnockoutObservable<any>): boolean {
        return false;
    }
    public isEmpty(value: string): boolean {
        return value == null || (typeof value == "string" && value.trim() === "");
    }
    public getValidationMetadata(property: KnockoutObservable<any>): DotvvmValidationObservableMetadata {
        return (<any>property).dotvvmMetadata;
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

class DotvvmEnforceClientFormatValidator extends DotvvmValidatorBase {
    public isValid(context: DotvvmValidationContext, property: KnockoutObservable<any>): boolean {
        // parameters order: AllowNull, AllowEmptyString, AllowEmptyStringOrWhitespaces
        var valid = true;
        if (!context.parameters[0] && context.valueToValidate == null) // AllowNull
        {
            valid = false;
        }
        if (!context.parameters[1] && context.valueToValidate.length === 0) // AllowEmptyString
        {
            valid = false;
        }
        if (!context.parameters[2] && this.isEmpty(context.valueToValidate)) // AllowEmptyStringOrWhitespaces
        {
            valid = false;
        }

        var metadata = this.getValidationMetadata(property);
        if (metadata && metadata.elementsMetadata) {
            for (var metaElement of metadata.elementsMetadata) {
                if (!metaElement.elementValidationState) {
                    valid = false;
                }
            }
        }
        return valid;
    }
}

class DotvvmRangeValidator extends DotvvmValidatorBase {
    public isValid(context: DotvvmValidationContext, property: KnockoutObservable<any>): boolean {
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

class DotvvmEmailAddressValidator extends DotvvmValidatorBase {
    public isValid(context: DotvvmValidationContext): boolean {
        var value = context.valueToValidate;
        if (value == null) return true;

        if (typeof value !== "string") return false;

        var found = false;
        for (var i = 0; i < value.length; i++)
        {
            if (value[i] == '@') {
                if (found || i == 0 || i == value.length - 1) {
                    return false;
                }
                found = true;
            }
        }

        return found;
    }
}

type KnockoutValidatedObservable<T> = KnockoutObservable<T> & { validationErrors?: KnockoutObservableArray<ValidationError> }

class ValidationError {
    constructor(public validatedObservable: KnockoutValidatedObservable<any>, public errorMessage: string) {
    }

    public static getOrCreate(validatedObservable: KnockoutValidatedObservable<any> & {wrappedProperty?: any}): KnockoutObservableArray<ValidationError> {
        if (validatedObservable.wrappedProperty) {
            var wrapped = validatedObservable.wrappedProperty();
            if (ko.isObservable(wrapped)) validatedObservable = wrapped;
        }
        if (!validatedObservable.validationErrors) {
            validatedObservable.validationErrors = ko.observableArray<ValidationError>();
        }
        return validatedObservable.validationErrors;
    }

    public static isValid(validatedObservable: KnockoutValidatedObservable<any>) {
        return !validatedObservable.validationErrors || validatedObservable.validationErrors().length === 0;
    }

    public clear(validation: DotvvmValidation) {
        var localErrors = this.validatedObservable.validationErrors as KnockoutObservableArray<ValidationError>;
        localErrors.remove(this);
        validation.errors.remove(this);
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

type DotvvmValidationRules = { [name: string]: DotvvmValidatorBase };

type DotvvmValidationElementUpdateFunctions = {
    [name: string]: (element: HTMLElement, errorMessages: string[], param: any) => void;
};

class DotvvmValidation {
    public rules: DotvvmValidationRules = {
        "required": new DotvvmRequiredValidator(),
        "regularExpression": new DotvvmRegularExpressionValidator(),
        "intrange": new DotvvmIntRangeValidator(),
        "range": new DotvvmRangeValidator(),
        "emailAddress": new DotvvmEmailAddressValidator(),
        "notnull": new DotvvmNotNullValidator(),
        "enforceClientFormat": new DotvvmEnforceClientFormatValidator()
    }

    public errors = ko.observableArray<ValidationError>([]);

    public events = {
        validationErrorsChanged: new DotvvmEvent<DotvvmEventArgs>("dotvvm.validation.events.validationErrorsChanged")
    };

    public elementUpdateFunctions: DotvvmValidationElementUpdateFunctions = {
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
                var classNames = className.split(' ');
                element.className = element.className.split(' ').filter(c => classNames.indexOf(c) < 0).join(' ');
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
        const createValidationHandler = (path: string) => ({
            execute: <T>(callback: () => Promise<T>, options: PostbackOptions) => {
                if (path) {
                    options.additionalPostbackData.validationTargetPath = path;
                    // resolve target
                    var context = ko.contextFor(options.sender);
                    var validationTarget = dotvvm.evaluator.evaluateOnViewModel(context, path);

                    this.errors([]);
                    this.clearValidationErrors(dotvvm.viewModelObservables[options.viewModelName || 'root']);
                    this.validateViewModel(validationTarget);
                    if (this.errors().length > 0) {
                        console.log("Validation failed: postback aborted; errors: ", this.errors());
                        return Promise.reject({type: "handler", handler: this, message: "Validation failed"})
                    }
                    this.events.validationErrorsChanged.trigger({viewModel: options.viewModel});
                }
                return callback()
            }
        })
        dotvvm.postbackHandlers["validate"] = (opt) => createValidationHandler(opt.path);
        dotvvm.postbackHandlers["validate-root"] = () => createValidationHandler("dotvvm.viewModelObservables['root']");
        dotvvm.postbackHandlers["validate-this"] = () => createValidationHandler("$data");

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

        dotvvm.events.spaNavigating.subscribe(args => {
            this.clearValidationErrors(dotvvm.viewModelObservables[args.viewModelName]);
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

    /**
     * Validates the specified view model
    */
    public validateViewModel(viewModel: any) {
        if (ko.isObservable(viewModel)) {
            viewModel = ko.unwrap(viewModel);
        }
        if (!viewModel) return;

        // find validation rules
        var type = ko.unwrap(viewModel.$type);

        // Event if there is no validation rules, there can be invalid value for given type
        var validationRules = dotvvm.viewModels['root'].validationRules || {};
        var rulesForType = validationRules![type] || {};

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
                var error = new ValidationError(viewModelProperty, `The value of property ${property} (${value}) is invalid value for type ${options.type}.`);
                this.addValidationError(viewModelProperty, error);
            }

            if (value) {
                if (Array.isArray(value)) {
                    // handle collections
                    for (var item of value) {
                        this.validateViewModel(item);
                    }
                }
                else if (value && value instanceof Object) {
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

            if (!ruleTemplate.isValid(context, property)) {
                var validationErrors = ValidationError.getOrCreate(property);
                // add error message
                var validationError = new ValidationError(property, rule.errorMessage);
                this.addValidationError(property, validationError);
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
            for (var type in args.serverResponseObject.validationRules) {
                if (!args.serverResponseObject.validationRules.hasOwnProperty(type)) continue;
                existingRules![type] = args.serverResponseObject.validationRules[type];
            }
        }
    }

    /**
      * Clears validation errors from the passed viewModel, from its children
      * and from the DotvvmValidation.errors array
    */
    public clearValidationErrors(validatedObservable: KnockoutValidatedObservable<any>) {
        if (!validatedObservable || !ko.isObservable(validatedObservable)) return;
        if (validatedObservable.validationErrors) {
            for (var error of validatedObservable.validationErrors()) {
                error.clear(this);
            }
        }

        var validatedObject = validatedObservable();
        if (!validatedObject) return;
        // Do the same for every object in the array
        if (Array.isArray(validatedObject)) {
            for (var item of validatedObject) {
                this.clearValidationErrors(item);
            }
        }
        // Do the same for every subordinate property
        for (var propertyName in validatedObject) {
            if (!validatedObject.hasOwnProperty(propertyName) || propertyName.indexOf("$") === 0) continue;
            var property = validatedObject[propertyName];
            this.clearValidationErrors(property);
        }
    }

    /**
     * Gets validation errors from the passed object and its children.
     * @param target Object that is supposed to contain the errors or properties with the errors
     * @param includeErrorsFromGrandChildren Is called "IncludeErrorsFromChildren" in ValidationSummary.cs
     * @param includeErrorsFromChildren Sets whether to include errors from children at all
     * @returns By default returns only errors from the viewModel's immediate children
     */
    public getValidationErrors(validationTargetObservable: KnockoutValidatedObservable<any>, includeErrorsFromGrandChildren, includeErrorsFromTarget, includeErrorsFromChildren = true): ValidationError[] {
        // Check the passed viewModel
        if (!validationTargetObservable) return [];

        var errors: ValidationError[] = [];

        // Include errors from the validation target
        if (includeErrorsFromTarget) {
            errors = errors.concat(ValidationError.getOrCreate(validationTargetObservable)());
        }

        if (includeErrorsFromChildren) {
            var validationTarget = ko.unwrap(validationTargetObservable);
            if (Array.isArray(validationTarget)) {
                for (var item of validationTarget) {
                    // This is correct because in the next children and further all children are grandchildren
                    errors = errors.concat(this.getValidationErrors(
                        item,
                        includeErrorsFromGrandChildren,
                        true,
                        includeErrorsFromGrandChildren));
                }
            }
            else {
                for (var propertyName in validationTarget) {
                    if (!validationTarget.hasOwnProperty(propertyName) || propertyName.indexOf("$") === 0) continue;
                    var property = validationTarget[propertyName];
                    if (!property || !ko.isObservable(property)) continue;
                    // Nested properties are children too
                    errors = errors.concat(this.getValidationErrors(
                        property,
                        includeErrorsFromGrandChildren,
                        true,
                        includeErrorsFromGrandChildren));
                }
            }
        }
        return errors;
    }

    /**
     * Adds validation errors from the server to the appropriate arrays
     */
    public showValidationErrorsFromServer(args: DotvvmAfterPostBackEventArgs) {
        // resolve validation target
        var context = ko.contextFor(args.sender);
        var validationTarget: KnockoutValidatedObservable<any>
            = dotvvm.evaluator.evaluateOnViewModel(context, args.postbackOptions.additionalPostbackData.validationTargetPath);
        if (!validationTarget) return;

        // add validation errors
        this.clearValidationErrors(dotvvm.viewModelObservables[args.viewModelName]);
        var modelState = args.serverResponseObject.modelState;
        for (var i = 0; i < modelState.length; i++) {
            // find the property
            var propertyPath = modelState[i].propertyPath;
            var property;
            if (propertyPath) {
                if (ko.isObservable(validationTarget)) {
                    validationTarget = ko.unwrap(validationTarget);
                }
                property = dotvvm.evaluator.evaluateOnViewModel(validationTarget, propertyPath);
            }
            else {
                property = validationTarget
            }

            // add the error to appropriate collections
            var error = new ValidationError(property, modelState[i].errorMessage);
            this.addValidationError(property, error);
        }
    }

    private addValidationError(validatedProperty: KnockoutValidatedObservable<any>, error: ValidationError) {
        var errors = ValidationError.getOrCreate(validatedProperty);
        if (errors.indexOf(error) < 0) {
            validatedProperty.validationErrors!.push(error);
            this.errors.push(error);
        }
    }
};

declare var dotvvm: DotVVM;
