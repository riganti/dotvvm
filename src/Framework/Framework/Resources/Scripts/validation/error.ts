import { errorsSymbol } from "./common";
import { DotvvmEvent } from "../events";
import { unwrapComputedProperty } from "../utils/evaluator";

export const allErrors: ValidationError[] = []

export const validationErrorsChanged = new DotvvmEvent<Partial<PostbackOptions> & {
    readonly allErrors: ValidationError[]
}>("dotvvm.validation.events.validationErrorsChanged");

const errorSubscriptions = new WeakMap<KnockoutObservable<any>, {
    value: any,
    subscription: KnockoutSubscription
}>();
let errorsChangedTask: number | undefined;

function currentValue(observable: KnockoutObservable<any>): any {
    const value = observable.peek();
    // Scalar client validators inspect the KO value, which can lag a state
    // patch. Collections need their immutable snapshot instead of a reused KO
    // wrapper, including fresh errors attached immediately after a direct edit.
    return value && typeof value === "object" && !(value instanceof Date) && "state" in observable
        ? (observable as any).state : value;
}

/** Clears stale errors without revalidating or changing the viewmodel value. */
export function detachErrors(observable: any, recursive = false): void {
    if (allErrors.length === 0 || observable == null) return;

    let removed = false;
    const visited = new Set<any>();
    function detach(value: any) {
        if (value == null || visited.has(value)) return;
        visited.add(value);
        const owner = unwrapComputedProperty(value);
        const tracked = errorSubscriptions.get(owner);
        if (recursive || !tracked || !Object.is(tracked.value, currentValue(owner))) {
            for (const error of [...getErrors(value)]) {
                error.detach();
                removed = true;
            }
        }
        if (recursive) {
            const contents = ko.unwrap(value);
            if (contents && typeof contents === "object") {
                for (const key of Object.keys(contents)) {
                    detach(contents[key]);
                }
            }
        }
    }
    detach(observable);

    if (removed && errorsChangedTask === undefined) {
        // StateManager can discard old children before their replacements reach
        // the DOM. Notify once after the complete observable update has settled.
        errorsChangedTask = ko.tasks.schedule(() => {
            errorsChangedTask = undefined;
            validationErrorsChanged.trigger({ allErrors });
        });
    }
}

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

    private constructor(public errorMessage: string, public propertyPath: string, public validatedObservable: KnockoutObservable<any>) {
        if (!errorMessage) {
            throw new Error(`String "${errorMessage}" is not a valid ValidationError message.`);
        }
    }

    public static attach(errorMessage: string, propertyPath: string, observable: KnockoutObservable<any>): ValidationError {
        let unwrapped = ValidationError.getOrCreateErrorsCollection(observable);
        const error = new ValidationError(errorMessage, propertyPath, unwrapped);
        unwrapped[errorsSymbol].push(error);
        allErrors.push(error);
        return error;
    }

    static attachIfNoErrors(errorMessage: string, propertyPath: string, observable: KnockoutObservable<any>) {
        let unwrapped = ValidationError.getOrCreateErrorsCollection(observable);
        if (unwrapped[errorsSymbol].length === 0) {
            const error = new ValidationError(errorMessage, propertyPath, unwrapped);
            unwrapped[errorsSymbol].push(error);
            allErrors.push(error);
        }
    }

    private static getOrCreateErrorsCollection(observable: KnockoutObservable<any>): KnockoutObservable<any> & { [errorsSymbol]: ValidationError[] } {
        if (!observable) {
            throw new Error(`ValidationError cannot be attached to "${observable}".`);
        }

        let unwrapped = unwrapComputedProperty(observable) as any;
        if (!unwrapped.hasOwnProperty(errorsSymbol)) {
            unwrapped[errorsSymbol] = [];
        }
        if (ko.isObservable(unwrapped)) {
            // Remove old-value errors before attaching new-value errors, even
            // when the state update has not reached the observable tree yet.
            detachErrors(unwrapped);
            if (!errorSubscriptions.has(unwrapped)) {
                const value = currentValue(unwrapped);
                const subscription = unwrapped.subscribe(() => detachErrors(unwrapped));
                errorSubscriptions.set(unwrapped, { value, subscription });
            }
        }
        return unwrapped;
    }

    public detach(): void {
        const errors = (this.validatedObservable as any)[errorsSymbol];
        const observableIndex = errors.indexOf(this);
        errors.splice(observableIndex, 1);

        const arrayIndex = allErrors.indexOf(this);
        allErrors.splice(arrayIndex, 1);
        if (errors.length === 0) {
            errorSubscriptions.get(this.validatedObservable)?.subscription.dispose();
            errorSubscriptions.delete(this.validatedObservable);
        }
    }
}
