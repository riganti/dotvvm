import * as evaluator from "../utils/evaluator"
import { validators } from './validators'
import { allErrors, detachAllErrors, ValidationError, getErrors } from "./error"
import { DotvvmEvent } from '../events'
import * as spaEvents from '../spa/events'
import { postbackHandlers } from "../postback/handlers"
import { DotvvmValidationContext, errorsSymbol } from "./common"
import { isPrimitive, keys } from "../utils/objects"
import { elementActions } from "./actions"
import { DotvvmPostbackError } from "../shared-classes"
import { getObjectTypeInfo } from "../metadata/typeMap"
import { tryCoerce } from "../metadata/coercer"
import { primitiveTypes } from "../metadata/primitiveTypes"
import { currentStateSymbol, lastSetErrorSymbol } from "../state-manager"
import { logError } from "../utils/logging"

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
            const validationTarget = evaluator.evaluateOnViewModel(context, path);

            runClientSideValidation(validationTarget, options);

            if (allErrors.length > 0) {
                logError("validation", "Validation failed: postback aborted; errors: ", allErrors);
                return Promise.reject(new DotvvmPostbackError({ type: "handler", handlerName: "validation", message: "Validation failed" }))
            }
        }
        return callback();
    }
})

const runClientSideValidation = (validationTarget: any, options: PostbackOptions) => {

    watchAndTriggerValidationErrorChanged(options,
        () => {
            detachAllErrors();
            const root = dotvvm.state;
            const target = ko.unwrap(validationTarget)[currentStateSymbol];
            const path = evaluator.findPathToChildObject(root, target, "")!;
            validateViewModel(validationTarget, path);
        });
}

export function init() {
    postbackHandlers["validate"] = (opt) => createValidationHandler(opt.path);
    postbackHandlers["validate-root"] = () => createValidationHandler("dotvvm.viewModelObservables['root']");
    postbackHandlers["validate-this"] = () => createValidationHandler("$data");

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

            validationErrorsChanged.subscribe(_ => {
                element.innerHTML = "";
                const errors = getValidationErrors(
                    binding.target,
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

function validateViewModel(viewModel: any, path: string): void {
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
        const propertyPath = path + "/" + propertyName;
        const propInfo = typeInfo?.properties[propertyName];
    
        if (propInfo?.validationRules) {
            validateProperty(viewModel, observable, propertyValue, propertyPath, propInfo.validationRules);
        }

        validateRecursive(observable, propertyValue, propInfo?.type || { type: "dynamic" }, propertyPath);
    }
}

function validateRecursive(observable: KnockoutObservable<any>, propertyValue: any, type: TypeDefinition, propertyPath: string) {
    const lastSetError: CoerceResult = (observable as any)[lastSetErrorSymbol];
    if (lastSetError && lastSetError.isError) {
        ValidationError.attach(lastSetError.message, propertyPath, observable);
    }
    
    if (Array.isArray(type)) {
        if (!propertyValue) return;
        let i = 0;
        for (const item of propertyValue) {
            validateRecursive(item, ko.unwrap(item), type[0], `${propertyPath}/[${i}]`);
            i++;
        }
        
    } else if (typeof type === "string") {
        if (!(type in primitiveTypes)) {
            validateViewModel(propertyValue, propertyPath);
        }

    } else if (typeof type === "object") {
        if (type.type === "dynamic") {

            if (Array.isArray(propertyValue)) {
                let i = 0;
                for (const item of propertyValue) {
                    validateRecursive(item, ko.unwrap(item), { type: "dynamic" }, `${propertyPath}/[${i}]`);
                    i++;
                }
            } else if (propertyValue && typeof propertyValue === "object") {
                if (propertyValue["$type"]) {
                    validateViewModel(propertyValue, propertyPath);
                } else {
                    for (const k of keys(propertyValue)) {
                        validateRecursive(ko.unwrap(propertyValue[k]), propertyValue[k], { type: "dynamic" }, propertyPath + "/" + k);
                    }
                }
            }

        }
    }
}

/** validates the specified property in the viewModel */
function validateProperty(viewModel: any, property: KnockoutObservable<any>, value: any, propertyPath: string, propertyRules: PropertyValidationRuleInfo[]) {
    for (const rule of propertyRules) {
        // validate the rules
        const validator = validators[rule.ruleName];
        const context: DotvvmValidationContext = {
            valueToValidate: value,
            parentViewModel: viewModel,
            parameters: rule.parameters
        };

        if (!validator.isValid(value, context, property)) {
            ValidationError.attach(rule.errorMessage, propertyPath, property);
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
    targetObservable: KnockoutObservable<T> | T | null,
    includeErrorsFromGrandChildren: boolean,
    includeErrorsFromTarget: boolean,
    includeErrorsFromChildren = true): ValidationError[] {

    if (!targetObservable) {
        return [];
    }

    let errors: ValidationError[] = [];

    if (includeErrorsFromTarget && ko.isObservable(targetObservable) && (targetObservable as any)[errorsSymbol] != null) {
        errors = errors.concat((targetObservable as any)[errorsSymbol]);
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
            
            let observableRootVM = dotvvm.viewModelObservables.root;

            // find the property
            const propertyPath = prop.propertyPath;
            const property = evaluator.traverseContext(observableRootVM, propertyPath);

            ValidationError.attach(prop.errorMessage, propertyPath, property);
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
