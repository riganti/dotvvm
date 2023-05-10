import { DotvvmValidationContext, isEmpty, getValidationMetadata } from "./common";

export type DotvvmValidator = {
    isValid: (value: any, context: DotvvmValidationContext, property: KnockoutObservable<any>) => boolean
}

export const required: DotvvmValidator = {
    isValid(value: any): boolean {
        return !isEmpty(value);
    }
}
export const regex: DotvvmValidator = {
    isValid(value: any, context): boolean {
        const [expr] = context.parameters;
        return isEmpty(value) || new RegExp(expr).test(value);
    }
}

export const enforceClientFormat: DotvvmValidator = {
    isValid(value: any, context, property): boolean {
        const [allowNull, allowEmptyString, allowEmptyStringOrWhitespaces] = context.parameters;
        let valid = true;
        if (!allowNull && value == null) {
            valid = false;
        }
        if (!allowEmptyString && value === "") {
            valid = false;
        }
        if (!allowEmptyStringOrWhitespaces && isEmpty(value)) {
            valid = false;
        }

        const metadata = getValidationMetadata(property);
        if (metadata) {
            for (const metaElement of metadata) {
                if (!metaElement.elementValidationState) {
                    valid = false;
                }
            }
        }
        return valid;
    }
}

export const range: DotvvmValidator = {
    isValid(val: any, context): boolean {
        const [from, to] = context.parameters;
        return isEmpty(val) || (val >= from && val <= to);
    }
}

export const notNull: DotvvmValidator = {
    isValid(value: any) {
        return value != null;
    }
}

export const emailAddress: DotvvmValidator = {
    isValid(value: any): boolean {
        if (typeof value != "string") {
            return true;
        }

        let found = false;
        for (let i = 0; i < value.length; i++) {
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

type DotvvmValidatorSet = { [name: string]: DotvvmValidator };
export const validators: DotvvmValidatorSet = {
    required: required,
    regularExpression: regex,
    range: range,
    emailAddress: emailAddress,
    notnull: notNull,
    enforceClientFormat: enforceClientFormat
}
