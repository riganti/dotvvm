export class DotvvmPostbackError {
    constructor(public reason: DotvvmPostbackErrorReason) {
    }
    toString() { return "PostbackRejectionError(" + JSON.stringify(this.reason, null, "   ") + ")"}
}

export class CoerceError implements CoerceErrorType {
    isError: true = true
    wasCoerced: false = false
    get value(): never {
        throw this
    }
    constructor(public message: string, public path: string = "") {
    }
    public static generic(value: any, type: TypeDefinition) {
        return new CoerceError(`Cannot coerce '${value}' to type '${type}'.`);
    }
    public prependPathFragment(fragment: string) {
        this.path = fragment + (this.path ? "/" : "") + this.path;
    }
}
