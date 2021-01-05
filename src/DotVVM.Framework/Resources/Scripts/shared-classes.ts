export class DotvvmPostbackError {
    constructor(public reason: DotvvmPostbackErrorReason) {
    }
    toString() { return "PostbackRejectionError(" + JSON.stringify(this.reason, null, "   ") + ")"}
}

export class CoerceError {
    constructor(public message: string, public path: string = "") {
    }
    public prependPathFragment(fragment: string) {
        this.path = fragment + (this.path ? "/" : "") + this.path;
    }
}
