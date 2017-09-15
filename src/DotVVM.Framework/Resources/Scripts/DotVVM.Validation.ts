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
    public isValid(context: DotvvmValidationContext, property: any): boolean {
        return false;
    }
    public isEmpty(value: any): boolean {
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

interface ValidationResult {
    [property: string]: ValidationResult | ValidationError[]
}

type KnockoutValidatedObservable<T> = KnockoutObservable<T> & { validationErrors?: KnockoutObservableArray<ValidationError> }

class ValidationError {
    constructor(public validator: ((value: any) => boolean | string | null) | null, public errorMessage: string) {
    }

    public static getOrCreate(validatedObservable: KnockoutValidatedObservable<any> & {wrappedProperty?: any}): KnockoutObservable<ValidationError[]> {
        if (validatedObservable.wrappedProperty) {
            var wrapped = validatedObservable.wrappedProperty();
            if (ko.isObservable(wrapped)) validatedObservable = wrapped;
        }

        if (!validatedObservable.validationErrors) {
            return ko.observableArray<ValidationError>([]);
        }
        return validatedObservable.validationErrors;
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

    private unwrapValidationTarget(validationTarget) : { unwrappedTarget?: any, targetUpdate?: (updater: StateUpdate<any>) => void } {
        if (validationTarget == null) return {};
        // TODO: replace this hack with a knockout-less variant
        // It will just reuire a change to dotvvm server to send obsevable-less validation targets
        if (!validationTarget["__unwrapped_data"] && validationTarget.viewModel) validationTarget = validationTarget.viewModel
        if (!validationTarget["__unwrapped_data"]) validationTarget = ko.unwrap(validationTarget)
        if (validationTarget == null) return {};
        const unwrappedTarget = ko.unwrap(validationTarget["__unwrapped_data"])
        const targetUpdate: (updater: StateUpdate<any>) => void = ko.unwrap(validationTarget["__update_function"])
        return {unwrappedTarget, targetUpdate}
    }

    constructor(dotvvm: DotVVM) {
        // perform the validation before postback
        dotvvm.events.beforePostback.subscribe(args => {
            if (args.validationTargetPath) {
                // clear previous errors
                dotvvm.rootRenderer.update(this.clearValidationErrors.bind(this))

                // resolve target
                var context = ko.contextFor(args.sender);
                var validationTarget = dotvvm.evaluator.evaluateOnViewModel(context, args.validationTargetPath)
                const { unwrappedTarget, targetUpdate } = this.unwrapValidationTarget(validationTarget)
                if (unwrappedTarget == null || targetUpdate == null) return;
                if (!unwrappedTarget || typeof unwrappedTarget != "object") throw new Error();
                // validate the object
                const validation = this.validateViewModel(unwrappedTarget);

                if (validation != this.validObjectResult) {
                    console.log("Validation failed: postback aborted; errors: ", validation);
                    args.cancel = true;
                    args.clientValidationFailed = true;
                    targetUpdate(vm => this.applyValidationErrors(vm, validation))
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

    public validObjectResult : ValidationResult = {}
    /**
     * Validates the specified view model
    */
    public validateViewModel(viewModel: object & { $type?: string }): ValidationResult {
        if (!viewModel || !dotvvm.viewModels['root'].validationRules) return this.validObjectResult;

        // find validation rules
        const type = viewModel.$type;
        if (!type) return this.validObjectResult;
        const rulesForType = dotvvm.viewModels['root'].validationRules![type] || {};

        let validationResult : ValidationResult | null = null

        // validate all properties
        for (const property in viewModel) {
            if (!viewModel.hasOwnProperty(property) || property.indexOf("$") === 0) continue;

            const value = viewModel[property];
            // run validation rules
            let errors = rulesForType.hasOwnProperty(property) ?
                this.validateProperty(viewModel, value, rulesForType[property]) :
                null;

            const options = viewModel[property + "$options"];
            if (options && options.type && errors == null && !dotvvm.serialization.validateType(value, options.type)) {
                var error = new ValidationError(
                    val => dotvvm.serialization.validateType(val, options.type),
                    `The value of property ${property} (${value}) is invalid value for type ${options.type}.`);
                errors = [ error ]
            }

            if (typeof value == "object" && value != null) {
                if (Array.isArray(value)) {
                    // handle collections
                    const a = this.validateArray(value)
                    if (a != this.validObjectResult) {
                        if (!validationResult) validationResult = {}
                        validationResult[property] = a
                    }
                }
                else if (value.$type) {
                    // handle nested objects
                    const a = this.validateViewModel(value)
                    if (a != this.validObjectResult) {
                        if (!validationResult) validationResult = {}
                        validationResult[property] = a
                    }
                }
            }

            if (errors) {
                if (validationResult && validationResult[property]) {
                    validationResult[property][""] = errors
                } else {
                    if (!validationResult) validationResult = {}
                    validationResult[property] = errors
                }
            }
        }
        return validationResult || this.validObjectResult;
    }

    public validateArray(array: any[]): ValidationResult {
        let validationResult : ValidationResult | null = null

        for (let index = 0; index < array.length; index++) {
            const a = this.validateViewModel(array[index])
            if (a != this.validObjectResult) {
                if (!validationResult) validationResult = {}
                validationResult[index] = a
            }
        }
        return validationResult || this.validObjectResult;
    }

    // validates the specified property in the viewModel
    public validateProperty(viewModel: any, value: any, rulesForProperty: IDotvvmPropertyValidationRuleInfo[]) {
        let errors : ValidationError[] | null = null
        for (var rule of rulesForProperty) {
            // validate the rules
            var ruleTemplate = this.rules[rule.ruleName];
            var context = new DotvvmValidationContext(value, viewModel, rule.parameters);

            if (!ruleTemplate.isValid(context, value)) {
                // add error message
                if (!errors) errors = []
                errors.push(new ValidationError(value, rule.errorMessage))
            }
        }
        return errors;
    }

    // merge validation rules
    public mergeValidationRules(args: DotvvmAfterPostBackEventArgs) {
        const newRules = args.serverResponseObject.validationRules;
        if (newRules) {
            const existingRules = dotvvm.receivedViewModel.validationRules ||
                (dotvvm.receivedViewModel.validationRules = {})
            for (const type in newRules) if (hasOwnProperty(newRules, type)) {
                existingRules![type] = newRules[type];
            }
        }
    }

    public applyValidationErrors<T>(object: T, errors: ValidationResult): T {
        if (typeof object != "object" || object == null || errors == this.validObjectResult) return object;

        // Do the same for every object in the array
        if (Array.isArray(object)) {
            return RendererInitializer.immutableMap(object, (a, i) => {
                if (i in errors) {
                    const e = errors[i]
                    if (Array.isArray(e)) throw new Error(`Arrays can't contain values with validation errors`);
                    else return this.applyValidationErrors(a, e)
                } else {
                    return a
                }
            }) as any as T
        } else {
        let result: any = {...<any>object};
        // Do the same for every subordinate property
            for (var prop in errors) {
                if (!Object.prototype.hasOwnProperty.call(errors, prop)) continue;
                const validationProp = prop + "$validation"
                const err = errors[prop]
                if (Array.isArray(err)) {
                    if (validationProp in object) {
                        // clone ...$validation field
                        result[validationProp] = {
                            ...object[validationProp],
                            errors: Array.prototype.concat(object[validationProp].errors || [], err)
                        }
                    } else {
                        result[validationProp] = { errors: err }
                    }
                } else {
                    result[prop] = this.applyValidationErrors(object[prop], err)
                }
            }
            return { ...<any>object, ...result }
        }
    }

    /**
      * Clears validation errors from the passed viewModel including its children
    */
    public clearValidationErrors<T>(validatedObject: T): T {
        if (typeof validatedObject != "object" || validatedObject == null) return validatedObject

        // Do the same for every object in the array
        if (Array.isArray(validatedObject)) {
            return RendererInitializer.immutableMap(validatedObject, this.clearValidationErrors.bind(this)) as any as T
        }
        let result: any = null;
        // Do the same for every subordinate property
        for (var propertyName in validatedObject) {
            if (!validatedObject.hasOwnProperty(propertyName) || propertyName.indexOf("$") === 0) continue;
            if (propertyName.substr(-11, 11) == "$validation") {
                // remove ..$validation fields
                if (result == null) result = {...<any>validatedObject}
                delete result[propertyName]
            } else if (propertyName.indexOf('$') < 0) {
                // update children
                const r = this.clearValidationErrors(validatedObject[propertyName])
                if (r !== validatedObject[propertyName]) {
                    if (result == null) result = {...<any>validatedObject}
                    result[propertyName] = r
                }
            }
        }
        return result || validatedObject;
    }

    /**
     * Gets validation errors from the passed object and its children.
     * @param target Object that is supposed to contain the errors or properties with the errors
     * @param includeErrorsFromGrandChildren Is called "IncludeErrorsFromChildren" in ValidationSummary.cs
     * @param includeErrorsFromChildren Sets whether to include errors from children at all
     * @returns By default returns only errors from the viewModel's immediate children
     */
    public getValidationErrors(validationTargetObservable: KnockoutValidatedObservable<any>, includeErrorsFromGrandChildren, includeErrorsFromTarget, includeErrorsFromChildren = true): ValidationError[] {
        // WORKAROUND: sometimes, this it called with `dotvvm.viewModelObservables` in parameter...
        if (validationTargetObservable == dotvvm.viewModelObservables['root']) validationTargetObservable = <any>validationTargetObservable.viewModel
        // Check the passed viewModel
        if (!validationTargetObservable) return [];

        let errors: ValidationError[] = [];

        // Include errors from the validation target
        if (includeErrorsFromTarget) {
            // TODO: not supported
        }

        if (includeErrorsFromChildren) {
            let validationTarget = ko.unwrap(validationTargetObservable)
            if (validationTarget && validationTarget["__unwrapped_data"]) validationTarget = ko.unwrap(validationTarget["__unwrapped_data"])
            if (typeof validationTarget != "object" || validationTarget == null) return errors;
            if (Array.isArray(validationTarget)) {
                for (var item of validationTarget) {
                    // This is correct because in the next children and further all children are grandchildren
                    errors = errors.concat(this.getValidationErrors(
                        item,
                        includeErrorsFromGrandChildren,
                        false,
                        includeErrorsFromGrandChildren));
                }
            }
            else {
                for (const propertyName in validationTarget) {
                    if (!validationTarget.hasOwnProperty(propertyName) || propertyName.indexOf("$") === 0) continue;
                    const property = validationTarget[propertyName];
                    const val = ko.unwrap(validationTarget[propertyName + "$validation"])
                    if (val && val.errors) {
                        errors = errors.concat(dotvvm.serialization.serialize(val.errors))
                    }
                    if (includeErrorsFromGrandChildren) {
                        errors = errors.concat(this.getValidationErrors(
                            property,
                            true,
                            false,
                            true));
                    }
                }
            }
        }

        return errors;
    }

    /**
     * Adds validation errors from the server to the appropriate arrays
     */
    public showValidationErrorsFromServer(args: DotvvmAfterPostBackEventArgs) {
        dotvvm.rootRenderer.update(this.clearValidationErrors.bind(this))

        // resolve target
        const context = ko.contextFor(args.sender);
        let validationTarget = dotvvm.evaluator.evaluateOnViewModel(context, args.validationTargetPath)
        if (!validationTarget) return;

        const { unwrappedTarget, targetUpdate } = this.unwrapValidationTarget(validationTarget)
        if (unwrappedTarget == null || targetUpdate == null) return;

        if (!unwrappedTarget) throw new Error();

        // add validation errors
        var modelState = args.serverResponseObject.modelState;
        for (var i = 0; i < modelState.length; i++) {
            // find the property
            var propertyPath = modelState[i].propertyPath;
            // TODO: add a new way of reporting property path and remove this hackery
            this.addErrorToProperty(validationTarget, propertyPath, modelState[i].errorMessage)
        }
    }

    private addErrorToProperty(target: KnockoutObservable<any>, propertyPath: string, error: string) {
        if (!propertyPath) throw new Error("Adding validation errors to validation target is not supported.");

        let [prop, objectPath] = (() => {
            const match = /(\w|\d|_|\$)*$/.exec(propertyPath)
            return [match![0], propertyPath.substr(0, match!.index)]
        })();
        if (objectPath.lastIndexOf('.') == objectPath.length - 1)
            objectPath = objectPath.substr(0, objectPath.length - 1);

        if (!prop) throw new Error();
        const {targetUpdate} = this.unwrapValidationTarget(target)

        targetUpdate!(vm => {
            const validationProp = prop + "$validation"
            const newErrors = [new ValidationError(null, error)]
            if (validationProp in vm) {
                return {...vm, [validationProp]: {
                    ...vm[validationProp],
                    errors: Array.prototype.concat(vm[validationProp].errors || [], newErrors)
                }};
            } else {
                return {...vm, [validationProp]: {
                    errors: newErrors
                }};
            }
        });
    }
};

declare var dotvvm: DotVVM;