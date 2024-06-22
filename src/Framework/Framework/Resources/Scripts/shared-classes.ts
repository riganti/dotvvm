export class DotvvmPostbackError {
    constructor(public reason: DotvvmPostbackErrorReason) {
    }
    toString() { return "PostbackRejectionError(" + JSON.stringify(this.reason, null, "   ") + ")"}
}

export class CoerceError extends Error implements CoerceErrorType {
    isError: true = true
    wasCoerced: false = false
    get value(): never {
        throw this
    }
    constructor(message: string, public path: string = "") {
        super(message)
        this.name = "CoerceError"
    }
    public static generic(value: any, type: TypeDefinition) {
        return new CoerceError(`Cannot coerce '${value}' to type '${type}'.`);
    }
    public prependPathFragment(fragment: string) {
        this.path = fragment + (this.path ? "/" : "") + this.path;
    }
}
