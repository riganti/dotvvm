import { ErrorsPropertyName } from "./common";
import { DotvvmEvent } from "../events";

export const allErrors: ValidationError[] = []

export function detachAllErrors() {
    while (allErrors.length > 0) {
        allErrors[allErrors.length - 1].detach();
    }
}

const unwrapComputedProperty =
    <T>(o: KnockoutObservable<T>) =>
        "wrappedProperty" in o ? o["wrappedProperty"] as KnockoutObservable<T> :
        o;

export function getErrors<T>(o: KnockoutObservable<T> | null): ValidationError[] {
    if (!ko.isObservable(o)) {
        return []
    }
    o = unwrapComputedProperty(o);
    return o[ErrorsPropertyName] || [];
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

        observable = unwrapComputedProperty(observable);
        if (!observable.hasOwnProperty(ErrorsPropertyName)) {
            observable[ErrorsPropertyName] = [];
        }
        const error = new ValidationError(errorMessage, observable);
        observable[ErrorsPropertyName].push(error);
        allErrors.push(error);
        return error;
    }

    public detach(): void {
        const errors = this.validatedObservable[ErrorsPropertyName];
        const observableIndex = errors.indexOf(this);
        errors.splice(observableIndex, 1);

        const arrayIndex = allErrors.indexOf(this);
        allErrors.splice(arrayIndex, 1);
    }
}
