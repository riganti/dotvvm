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
        for (var i = 0; i < value.length; i++) {
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

const ErrorsPropertyName = "validationErrors";

class ValidationError {

    private constructor(public errorMessage: string, public validatedObservable: KnockoutObservable<any>) {
    }

    static attach(errorMessage: string, observable: KnockoutObservable<any>): ValidationError {
        if (!errorMessage) {
            throw new Error(`String "${errorMessage}" is not a valid ValidationError message.`);
        }
        if (!observable) {
            throw new Error(`ValidationError cannot be attached to "${observable}".`);
        }

        if ("wrappedProperty" in observable) {
            observable = observable["wrappedProperty"]();
        }
        if (!observable.hasOwnProperty(ErrorsPropertyName)) {
            observable[ErrorsPropertyName] = [];
        }
        let error = new ValidationError(errorMessage, observable);
        observable[ErrorsPropertyName].push(error);
        dotvvm.validation.errors.push(error);
        return error;
    }

    detach(): void {
        let observableIndex = this.validatedObservable.validationErrors.indexOf(this);
        this.validatedObservable.validationErrors.splice(observableIndex, 1);

        let arrayIndex = dotvvm.validation.errors.indexOf(this);
        dotvvm.validation.errors.splice(arrayIndex, 1);
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

type ValidationSummaryBinding = {
    target: KnockoutObservable<any>,
    includeErrorsFromChildren: boolean,
    includeErrorsFromTarget: boolean,
    hideWhenValid: boolean
}

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

    public errors: ValidationError[] = [];

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
            let classes = className.split(/\s+/).filter(c => c.length > 0);
            for (let i = 0; i < classes.length; i++) {
                let className = classes[i];

                if (errorMessages.length > 0) {
                    element.classList.add(className);
                } else {
                    element.classList.remove(className);
                }
            }
        },

        // sets the error message as the title attribute
        setToolTipText(element: HTMLElement, errorMessages: string[], param: any) {
            if (errorMessages.length > 0) {
                element.title = errorMessages.join(" ");
            } else {
                element.title = "";
            }
        },

        // displays the error message
        showErrorMessageText(element: any, errorMessages: string[], param: any) {
            element[element.innerText ? "innerText" : "textContent"] = errorMessages.join(" ");
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

                    this.detachAllErrors();
                    this.validateViewModel(validationTarget);
                    this.events.validationErrorsChanged.trigger({ viewModel: options.viewModel });
                    if (this.errors.length > 0) {
                        console.log("Validation failed: postback aborted; errors: ", this.errors);
                        return Promise.reject({ type: "handler", handler: this, message: "Validation failed" })
                    }
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
                    this.detachAllErrors();
                    this.showValidationErrorsFromServer(args);
                    this.events.validationErrorsChanged.trigger(args);
                    args.isHandled = true;
                }
            }

        });

        dotvvm.events.spaNavigating.subscribe(args => {
            this.detachAllErrors();
            this.events.validationErrorsChanged.trigger({ viewModel: args.viewModel });
        });

        // Validator
        ko.bindingHandlers["dotvvmValidation"] = {
            init: (element: HTMLElement, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor) => {
                dotvvm.validation.events.validationErrorsChanged.subscribe(e => {
                    this.applyValidatorOptions(element, valueAccessor(), allBindingsAccessor.get("dotvvmValidationOptions"));
                });
            },
            update: (element: HTMLElement, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor) => {
                this.applyValidatorOptions(element, valueAccessor(), allBindingsAccessor.get("dotvvmValidationOptions"));
            }
        };

        // ValidationSummary
        ko.bindingHandlers["dotvvm-validationSummary"] = {
            init: function (element: HTMLElement, valueAccessor: () => ValidationSummaryBinding) {
                let binding = valueAccessor();
                dotvvm.validation.events.validationErrorsChanged.subscribe(e => {
                    element.innerHTML = "";
                    let errors = dotvvm.validation.getValidationErrors(
                        binding.target,
                        binding.includeErrorsFromChildren,
                        binding.includeErrorsFromTarget
                    );
                    for (let error of errors) {
                        let item = document.createElement("li");
                        item.innerText = error.errorMessage;
                        element.appendChild(item);
                    }

                    if (binding.hideWhenValid) {
                        if (errors.length > 0) {
                            element.style.display = "";
                        }
                        else {
                            element.style.display = "none";
                        }
                    }
                });
            }
        }
    }

    /**
     * Validates the specified view model
    */
    public validateViewModel(viewModel: any): void {
        if (ko.isObservable(viewModel)) {
            viewModel = ko.unwrap(viewModel);
        }
        if (!viewModel) {
            return;
        }

        // find validation rules for the property type
        let rootRules = dotvvm.viewModels['root'].validationRules || {};
        let type = ko.unwrap(viewModel.$type);
        let rules = rootRules![type] || {};

        // validate all properties
        for (let propertyName in viewModel) {
            if (!viewModel.hasOwnProperty(propertyName) || propertyName.indexOf("$") === 0) {
                continue;
            }

            let observable = viewModel[propertyName];
            if (!observable || !ko.isObservable(observable)) {
                continue;
            }

            let propertyValue = observable();

            // run validators
            if (rules.hasOwnProperty(propertyName)) {
                this.validateProperty(viewModel, observable, propertyValue, rules[propertyName]);
            }

            // check the value is even valid for the given type
            let options = viewModel[propertyName + "$options"];
            if (options
                && options.type
                && !DotvvmValidation.hasErrors(observable)
                && !dotvvm.serialization.validateType(propertyValue, options.type)) {
                ValidationError.attach(`The value of property ${propertyName} (${propertyValue}) is invalid value for type ${options.type}.`, observable);
            }

            if (!propertyValue) {
                continue;
            }

            // recurse
            if (Array.isArray(propertyValue)) {
                // handle collections
                for (var item of propertyValue) {
                    this.validateViewModel(item);
                }
            }
            else if (propertyValue && propertyValue instanceof Object) {
                // handle nested objects
                this.validateViewModel(propertyValue);
            }
        }
    }

    // validates the specified property in the viewModel
    public validateProperty(viewModel: any, property: KnockoutObservable<any>, value: any, propertyRules: IDotvvmPropertyValidationRuleInfo[]) {
        for (var rule of propertyRules) {
            // validate the rules
            var validator = this.rules[rule.ruleName];
            var context = new DotvvmValidationContext(value, viewModel, rule.parameters);

            if (!validator.isValid(context, property)) {
                ValidationError.attach(rule.errorMessage, property);
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
    public clearValidationErrors(observable: KnockoutObservable<any>): void {
        if (!observable || !ko.isObservable(observable)) {
            return;
        }
        if (observable[ErrorsPropertyName]) {
            // clone the array as `detach` mutates it
            const errors = observable[ErrorsPropertyName].concat([]);
            for (var error of errors) {
                error.detach();
            }
        }

        var validatedObject = ko.unwrap(observable);
        if (!validatedObject) {
            return;
        }

        if (Array.isArray(validatedObject)) {
            // element recursion
            for (var item of validatedObject) {
                this.clearValidationErrors(item);
            }
        }

        for (var propertyName in validatedObject) {
            // property recursion
            if (!validatedObject.hasOwnProperty(propertyName) || propertyName.indexOf("$") === 0) {
                continue;
            }
            var property = validatedObject[propertyName];
            this.clearValidationErrors(property);
        }
    }

    public detachAllErrors(): void {
        while (this.errors.length > 0) {
            this.errors[0].detach();
        }
    }

    /**
     * Gets validation errors from the passed object and its children.
     * @param validationTargetObservable Object that is supposed to contain the errors or properties with the errors
     * @param includeErrorsFromGrandChildren Is called "IncludeErrorsFromChildren" in ValidationSummary.cs
     * @returns By default returns only errors from the viewModel's immediate children
     */
    public getValidationErrors(validationTargetObservable: KnockoutObservable<any>,
        includeErrorsFromGrandChildren: boolean,
        includeErrorsFromTarget: boolean,
        includeErrorsFromChildren = true): ValidationError[] {

        if (!validationTargetObservable) {
            return [];
        }

        var errors: ValidationError[] = [];

        if (includeErrorsFromTarget && validationTargetObservable.hasOwnProperty(ErrorsPropertyName)) {
            errors = errors.concat(validationTargetObservable[ErrorsPropertyName]);
        }

        if (!includeErrorsFromChildren) {
            return errors;
        }

        var validationTarget = ko.unwrap(validationTargetObservable);
        if (Array.isArray(validationTarget)) {
            for (var item of validationTarget) {
                // the next children are grandchildren
                errors = errors.concat(this.getValidationErrors(
                    item,
                    includeErrorsFromGrandChildren,
                    true,
                    includeErrorsFromGrandChildren));
            }
            return errors;
        }
        for (var propertyName in validationTarget) {
            if (!validationTarget.hasOwnProperty(propertyName) || propertyName.indexOf("$") === 0) {
                continue;
            }

            var property = validationTarget[propertyName];
            if (!property || !ko.isObservable(property)) {
                continue;
            }

            // consider nested properties to be children
            errors = errors.concat(this.getValidationErrors(
                property,
                includeErrorsFromGrandChildren,
                true,
                includeErrorsFromGrandChildren));
        }
        return errors;
    }

    /**
     * Adds validation errors from the server to the appropriate arrays
     */
    public showValidationErrorsFromServer(args: DotvvmAfterPostBackEventArgs) {
        // resolve validation target
        var context = ko.contextFor(args.sender);
        var validationTarget = <KnockoutObservable<any>>dotvvm.evaluator.evaluateOnViewModel(
            context,
            args.postbackOptions.additionalPostbackData.validationTargetPath);
        if (!validationTarget) {
            return;
        }

        // add validation errors
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

            ValidationError.attach(modelState[i].errorMessage, property);
        }
    }

    private static hasErrors(observable: KnockoutObservable<any>): boolean {
        if ("wrappedProperty" in observable) {
            observable = observable["wrappedProperty"]();
        }
        return observable.hasOwnProperty(ErrorsPropertyName)
            && observable[ErrorsPropertyName].length > 0;
    }

    private applyValidatorOptions(validator: HTMLElement,
        observable: any,
        validatorOptions: any): void {

        if (!ko.isObservable(observable)) {
            return;
        }
        if ("wrappedProperty" in observable) {
            observable = observable["wrappedProperty"]();
        }

        let errors = <ValidationError[]>observable[ErrorsPropertyName] || [];
        let errorMessages = errors.map(v => v.errorMessage);
        for (const option in validatorOptions) {
            if (validatorOptions.hasOwnProperty(option)) {
                dotvvm.validation.elementUpdateFunctions[option](
                    validator,
                    errorMessages,
                    validatorOptions[option]);
            }
        }
    }
};

declare var dotvvm: DotVVM;
