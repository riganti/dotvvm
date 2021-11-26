import { errorsSymbol } from "./common";
import { DotvvmEvent } from "../events";
import { unwrapComputedProperty } from "../utils/evaluator";

export const allErrors: ValidationError[] = []

export function detachAllErrors() {
    while (allErrors.length > 0) {
        allErrors[allErrors.length - 1].detach();
    }
}

export function getErrors<T>(o: KnockoutObservable<T> | null): ValidationError[] {
    const unwrapped = unwrapComputedProperty(o);
    if (!ko.isObservable(o)) {
        return []
    }
    return unwrapped[errorsSymbol] || [];
}

export class ValidationError {

    private constructor(public errorMessage: string, public validatedObservable: KnockoutObservable<any>) {
    }

    public static attach(errorMessage: string, observable: KnockoutObservable<any>): ValidationError {
        if (!errorMessage) {
            throw new Error(`String "${errorMessage}" is not a valid ValidationError message.`);
        }
        if (!observable) {
            throw new Error(`ValidationError cannot be attached to "${observable}".`);
        }

        let unwrapped = unwrapComputedProperty(observable) as any;
        if (!unwrapped.hasOwnProperty(errorsSymbol)) {
            unwrapped[errorsSymbol] = [];
        }
        const error = new ValidationError(errorMessage, unwrapped);
        unwrapped[errorsSymbol].push(error);
        allErrors.push(error);
        return error;
    }

    public detach(): void {
        const errors = (this.validatedObservable as any)[errorsSymbol];
        const observableIndex = errors.indexOf(this);
        errors.splice(observableIndex, 1);

        const arrayIndex = allErrors.indexOf(this);
        allErrors.splice(arrayIndex, 1);
    }
}
