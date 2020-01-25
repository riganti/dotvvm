import * as evaluator from "../utils/evaluator"
import { DotvvmValidator } from "./validators"
import { validators } from './validators'
import { allErrors, detachAllErrors, ValidationError, getErrors } from "./error"
import { DotvvmEvent } from '../events'
import * as dotvvmEvents from '../events'
import * as spaEvents from '../spa/events'
import { DotvvmPostbackError } from "../shared-classes"
import { postbackHandlers } from "../postback/handlers"
import { DotvvmValidationContext, ErrorsPropertyName } from "./common"
import { hasOwnProperty, isPrimitive } from "../utils/objects"
import { validateType } from "../serialization/typeValidation"
import { elementActions } from "./actions"
import { getValidationRules } from "../dotvvm-base"

type ValidationSummaryBinding = {
    target: KnockoutObservable<any>,
    includeErrorsFromChildren: boolean,
    includeErrorsFromTarget: boolean
}

const validationErrorsChanged = new DotvvmEvent<DotvvmEventArgs>("dotvvm.validation.events.validationErrorsChanged");

export const events = {
    validationErrorsChanged
};

export const globalValidationObject = {
    rules: validators,
    errors: allErrors,
    events
}

const createValidationHandler = (path: string) => ({
    execute: (callback: () => Promise<PostbackCommitFunction>, options: PostbackOptions) => {
        if (path) {
            options.additionalPostbackData.validationTargetPath = path;
            // resolve target
            const context = ko.contextFor(options.sender);
            const validationTarget = evaluator.evaluateOnViewModel(context, path);

            detachAllErrors();
            validateViewModel(validationTarget);

            validationErrorsChanged.trigger({ });
            if (allErrors.length > 0) {
                console.log("Validation failed: postback aborted; errors: ", allErrors);
                return Promise.reject(new DotvvmPostbackError({ type: "handler", handlerName: "validation", message: "Validation failed" }))
            }
        }
        return callback()
    }
})

export function init() {
    postbackHandlers["validate"] = (opt) => createValidationHandler(opt.path);
    postbackHandlers["validate-root"] = () => createValidationHandler("dotvvm.viewModelObservables['root']");
    postbackHandlers["validate-this"] = () => createValidationHandler("$data");

    dotvvmEvents.afterPostback.subscribe(args => {
        if (!args.wasInterrupted && args.serverResponseObject) {
            if (args.serverResponseObject.action === "successfulCommand") {
                // merge validation rules from postback with those we already have (required when a new type appears in the view model)
                mergeValidationRules(args);
                args.handled = true;
            } else if (args.serverResponseObject.action === "validationErrors") {
                // apply validation errors from server
                detachAllErrors();
                showValidationErrorsFromServer(args);
                validationErrorsChanged.trigger(args);
                args.handled = true;
            }
        }

    });
    if (compileConstants.isSpa) {
        spaEvents.spaNavigating.subscribe(_ => {
            detachAllErrors();
            validationErrorsChanged.trigger({ });
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
            });
        }
    }
}

function validateViewModel(viewModel: any): void {
    if (ko.isObservable(viewModel)) {
        viewModel = ko.unwrap(viewModel);
    }
    if (!viewModel) {
        return;
    }

    // find validation rules for the property type
    const rootRules = getValidationRules();
    const type = ko.unwrap(viewModel.$type);
    const rules = rootRules[type] || {};

    // validate all properties
    for (const propertyName of Object.keys(viewModel)) {
        if (propertyName[0] == '$') {
            continue;
        }

        const observable = viewModel[propertyName];
        if (!ko.isObservable(observable)) {
            continue;
        }

        const propertyValue = observable();

        // run validators
        if (hasOwnProperty(rules, propertyName)) {
            validateProperty(viewModel, observable, propertyValue, rules[propertyName]);
        }

        // check the value is even valid for the given type
        const options = viewModel[propertyName + "$options"];
        if (options
            && options.type
            && getErrors(observable).length == 0
            && !validateType(propertyValue, options.type)) {
            ValidationError.attach(`The value of property ${propertyName} (${propertyValue}) is invalid value for type ${options.type}.`, observable);
        }

        if (!propertyValue) {
            continue;
        }

        // recurse
        if (Array.isArray(propertyValue)) {
            // handle collections
            for (const item of propertyValue) {
                validateViewModel(item);
            }
        }
        else if (propertyValue instanceof Object) {
            // handle nested objects
            validateViewModel(propertyValue);
        }
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

/** Adds validation rules from the serverResponseObject into our global validation rule collection */
function mergeValidationRules(args: DotvvmAfterPostBackEventArgs) {
    const newRules = args.serverResponseObject.validationRules;
    if (newRules) {
        const existingRules = getValidationRules();
        for (const type of Object.keys(newRules)) {
            existingRules[type] = newRules[type];
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

    if (includeErrorsFromTarget && ko.isObservable(targetObservable) && ErrorsPropertyName in targetObservable) {
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
    for (const propertyName of Object.keys(validationTarget)) {
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
function showValidationErrorsFromServer(args: DotvvmAfterPostBackEventArgs) {
    // resolve validation target
    const dataContext = ko.contextFor(args.sender);
    const validationTarget = <KnockoutObservable<any>> evaluator.evaluateOnViewModel(
        dataContext,
        args.postbackOptions.additionalPostbackData.validationTargetPath!);
    if (!validationTarget) {
        return;
    }

    // add validation errors
    for (const prop of args.serverResponseObject.modelState) {
        // find the property
        const propertyPath = prop.propertyPath;
        const property =
            propertyPath ?
            evaluator.evaluateOnViewModel(ko.unwrap(validationTarget), propertyPath) :
            validationTarget;

        ValidationError.attach(prop.errorMessage, property);
    }
}

function applyValidatorActions(
    validator: HTMLElement,
    observable: any,
    validatorOptions: any): void {

    const errors = getErrors(observable);
    const errorMessages = errors.map(v => v.errorMessage);
    for (const option of Object.keys(validatorOptions)) {
        elementActions[option](
            validator,
            errorMessages,
            validatorOptions[option]);
    }
}
