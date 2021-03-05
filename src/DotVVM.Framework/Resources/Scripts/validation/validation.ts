import * as evaluator from "../utils/evaluator"
import { validators } from './validators'
import { allErrors, detachAllErrors, ValidationError, getErrors } from "./error"
import { DotvvmEvent } from '../events'
import * as spaEvents from '../spa/events'
import { postbackHandlers } from "../postback/handlers"
import { DotvvmValidationContext, ErrorsPropertyName } from "./common"
import { isPrimitive, keys } from "../utils/objects"
import { elementActions } from "./actions"
import { DotvvmPostbackError } from "../shared-classes"
import { getObjectTypeInfo } from "../metadata/typeMap"
import { tryCoerce } from "../metadata/coercer"
import { primitiveTypes } from "../metadata/primitiveTypes"
import evaluateExpression = evaluator.evaluateExpression;
import evaluateExpression1 = evaluator.evaluateExpression;
import evaluateExpression2 = evaluator.evaluateExpression;

type ValidationSummaryBinding = {
    target: KnockoutObservable<any>,
    includeErrorsFromChildren: boolean,
    includeErrorsFromTarget: boolean,
    hideWhenValid: boolean
}

type DotvvmValidationErrorsChangedEventArgs = PostbackOptions & {
    readonly allErrors: ValidationError[]
}

const validationErrorsChanged = new DotvvmEvent<DotvvmValidationErrorsChangedEventArgs>("dotvvm.validation.events.validationErrorsChanged");

export const events = {
    validationErrorsChanged
};

export const globalValidationObject = {
    rules: validators,
    errors: allErrors,
    events
}

const createValidationHandler = (path: string) => ({
    name: "validate",
    execute: (callback: () => Promise<PostbackCommitFunction>, options: PostbackOptions) => {
        if (path) {
            options.validationTargetPath = path;
            // resolve target
            const context = ko.contextFor(options.sender);
            const validationTarget = evaluateExpression(context, path);

            watchAndTriggerValidationErrorChanged(options,
                () => {
                    detachAllErrors();
                    validateViewModel(validationTarget);
                });

            if (allErrors.length > 0) {
                console.log("Validation failed: postback aborted; errors: ", allErrors);
                return Promise.reject(new DotvvmPostbackError({
                    type: "handler",
                    handlerName: "validation",
                    message: "Validation failed"
                }))
            }
        }
        return callback();
    }
});

export function init() {
    postbackHandlers["validate"] = (opt) => createValidationHandler(opt.path);
    postbackHandlers["validate-root"] = () => createValidationHandler("/");
    postbackHandlers["validate-this"] = () => createValidationHandler("$data"); //TODO

    if (compileConstants.isSpa) {
        spaEvents.spaNavigating.subscribe(args => {
            watchAndTriggerValidationErrorChanged(args, () => {
                detachAllErrors();
            });
        });
    }

    // Validator
    ko.bindingHandlers["dotvvm-validation"] = {
        init: (element: HTMLElement, valueAccessor: () => any, allBindingsAccessor?: KnockoutAllBindingsAccessor) => {
            validationErrorsChanged.subscribe(_ => {
                applyValidatorActions(element, valueAccessor(), allBindingsAccessor!.get("dotvvm-validationOptions"));
            });
        },
        update: (element: HTMLElement, valueAccessor: () => any, allBindingsAccessor?: KnockoutAllBindingsAccessor) => {
            applyValidatorActions(element, valueAccessor(), allBindingsAccessor!.get("dotvvm-validationOptions"));
        }
    };

    // ValidationSummary
    ko.bindingHandlers["dotvvm-validationSummary"] = {
        init: (element: HTMLElement, valueAccessor: () => ValidationSummaryBinding) => {
            const binding = valueAccessor();
            const target = evaluateExpression1(ko.contextFor(element), ko.unwrap(binding.target));
            validationErrorsChanged.subscribe(_ => {
                element.innerHTML = "";
                const errors = getValidationErrors(
                    target,
                    binding.includeErrorsFromChildren,
                    binding.includeErrorsFromTarget
                );
                for (const error of errors) {
                    const item = document.createElement("li");
                    item.innerText = error.errorMessage;
                    element.appendChild(item);
                }
                
                if (binding.hideWhenValid) {
                    element.style.display = errors.length > 0 ? "" : "none";
                }
            });
        }
    }
}

function validateViewModel(viewModel: any): void {
    if (ko.isObservable(viewModel)) {
        viewModel = ko.unwrap(viewModel);
    }
    if (!viewModel || typeof viewModel !== "object") {
        return;
    }

    // find validation rules for the property type
    const typeId = ko.unwrap(viewModel.$type);
    let typeInfo;
    if (typeId) {
        typeInfo = getObjectTypeInfo(typeId);
    }

    // validate all properties
    for (const propertyName of keys(viewModel)) {
        if (propertyName[0] == '$') {
            continue;
        }

        const observable = viewModel[propertyName];
        if (!ko.isObservable(observable)) {
            continue;
        }

        const propertyValue = observable();

        // run validators
        const propInfo = typeInfo?.properties[propertyName];
    
        if (propInfo?.validationRules) {
            validateProperty(viewModel, observable, propertyValue, propInfo.validationRules);
        }
        
        validateRecursive(propertyName, observable, propertyValue, propInfo?.type || { type: "dynamic" });
    }
}

function validateRecursive(propertyName: string, observable: KnockoutObservable<any>, propertyValue: any, type: TypeDefinition) {
    if (Array.isArray(type)) {
        if (!propertyValue) return;
        let i = 0;
        for (const item of propertyValue) {
            validateRecursive("[" + i + "]", item, ko.unwrap(item), type[0]);
            i++;
        }
        
    } else if (typeof type === "string") {
        if (type in primitiveTypes) {
            validatePrimitiveType(propertyName, observable, propertyValue, type);
        } else {
            validateViewModel(propertyValue);
        }

    } else if (typeof type === "object") {
        if (type.type === "nullable") {
            validatePrimitiveType(propertyName, observable, propertyValue, type);

        } else if (type.type === "dynamic") {

            if (Array.isArray(propertyValue)) {
                let i = 0;
                for (const item of propertyValue) {
                    validateRecursive("[" + i + "]", item, ko.unwrap(item), { type: "dynamic" });
                    i++;
                }
            } else if (propertyValue && typeof propertyValue === "object") {
                if (propertyValue["$type"]) {
                    validateViewModel(propertyValue);
                } else {
                    for (const k of keys(propertyValue)) {
                        validateRecursive(k, ko.unwrap(propertyValue[k]), propertyValue[k], { type: "dynamic" });
                    }
                }
            }

        }
    }
}

function validatePrimitiveType(propertyName: string, observable: KnockoutObservable<any>, propertyValue: any, type: TypeDefinition) {
    if (getErrors(observable).length == 0 && !tryCoerce(propertyValue, type)) {
        ValidationError.attach(`The value of property ${propertyName} (${propertyValue}) is invalid value for type ${type}.`, observable);
        // TODO: we may not need to validate primitive types any more
    }
}

function validateArray(propertyValue: any[], type: TypeDefinition) {
    // handle collections
    for (const item of propertyValue) {
        validateViewModel(item);
    }
}

/** validates the specified property in the viewModel */
function validateProperty(viewModel: any, property: KnockoutObservable<any>, value: any, propertyRules: PropertyValidationRuleInfo[]) {
    for (const rule of propertyRules) {
        // validate the rules
        const validator = validators[rule.ruleName];
        const context: DotvvmValidationContext = {
            valueToValidate: value,
            parentViewModel: viewModel,
            parameters: rule.parameters
        };

        if (!validator.isValid(value, context, property)) {
            ValidationError.attach(rule.errorMessage, property);
        }
    }
}

/**
 * Gets validation errors from the passed object and its children.
 * @param targetObservable Object that is supposed to contain the errors or properties with the errors
 * @param includeErrorsFromGrandChildren Is called "IncludeErrorsFromChildren" in ValidationSummary.cs
 * @returns By default returns only errors from the viewModel's immediate children
 */
function getValidationErrors<T>(
    targetObservable: KnockoutObservable<T> | T | null | any,
    includeErrorsFromGrandChildren: boolean,
    includeErrorsFromTarget: boolean,
    includeErrorsFromChildren = true): ValidationError[] {

    if (!targetObservable) {
        return [];
    }

    let errors: ValidationError[] = [];

    if (includeErrorsFromTarget && ErrorsPropertyName in targetObservable) {
        errors = errors.concat(targetObservable[ErrorsPropertyName]);
    }

    if (!includeErrorsFromChildren) {
        return errors;
    }

    const validationTarget = ko.unwrap(targetObservable);
    if (isPrimitive(validationTarget)) {
        return errors;
    }
    if (Array.isArray(validationTarget)) {
        for (const item of validationTarget) {
            // the next children are grandchildren
            errors = errors.concat(getValidationErrors(
                item,
                includeErrorsFromGrandChildren,
                true,
                includeErrorsFromGrandChildren));
        }
        return errors;
    }
    for (const propertyName of keys(validationTarget)) {
        if (propertyName[0] == '$') {
            continue;
        }

        const property = (validationTarget as any)[propertyName];
        if (!ko.isObservable(property)) {
            continue;
        }

        // consider nested properties to be children
        errors = errors.concat(getValidationErrors(
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
export function showValidationErrorsFromServer(serverResponseObject: any, options: PostbackOptions) {
    watchAndTriggerValidationErrorChanged(options, () => {
        detachAllErrors();

        // add validation errors
        for (const prop of serverResponseObject.modelState) {
            
            // find the property
            const propertyPath = prop.propertyPath;
            let rootVM = dotvvm.viewModels.root.viewModel;
            const property =
                propertyPath ?
                evaluateExpression2(rootVM, propertyPath) :
                rootVM;

            ValidationError.attach(prop.errorMessage, property);
        }
    });
}

function applyValidatorActions(
    validator: HTMLElement,
    observable: any,
    validatorOptions: any): void {

    const errors = getErrors(observable);
    const errorMessages = errors.map(v => v.errorMessage);
    for (const option of keys(validatorOptions)) {
        elementActions[option](
            validator,
            errorMessages,
            validatorOptions[option]);
    }
}

function watchAndTriggerValidationErrorChanged(options: PostbackOptions, action: () => void) {
    const originalErrorsCount = allErrors.length;
    action();

    const currentErrorsCount = allErrors.length;
    if (originalErrorsCount == 0 && currentErrorsCount == 0) {
        // no errors before, no errors now
        return;
    }

    validationErrorsChanged.trigger({ ...options, allErrors });
}
