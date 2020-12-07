export type DotvvmValidationContext = {
    readonly valueToValidate: any,
    readonly parentViewModel: any,
    readonly parameters: any[]
}

export type DotvvmValidationObservableMetadata = DotvvmValidationElementMetadata[];
export type DotvvmValidationElementMetadata = {
    element: HTMLElement;
    dataType: string;
    format: string;
    domNodeDisposal: boolean;
    elementValidationState: boolean;
}

export const ErrorsPropertyName = "validationErrors";

/** Checks if the value is null, undefined or a whitespace only string */
export function isEmpty(value: any): boolean {
    return value == null || (typeof value == "string" && value.trim() === "")
}

export function getValidationMetadata(property: KnockoutObservable<any>): DotvvmValidationObservableMetadata {
    return (<any> property).dotvvmMetadata;
}
