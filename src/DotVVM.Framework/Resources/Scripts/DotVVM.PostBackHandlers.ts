type DotvvmPostbackHandler = {
    execute(callback: () => Promise<PostbackCommitFunction>, options: PostbackOptions): Promise<PostbackCommitFunction>
    name?: string
    after?: (string | DotvvmPostbackHandler)[]
    before?: (string | DotvvmPostbackHandler)[]
}

type PostbackCommitFunction = () => Promise<DotvvmAfterPostBackEventArgs>

type PostbackRejectionReason =
    | { type: "handler", handler: DotvvmPostbackHandler, message?: string }
    | { type: 'network', args: DotvvmErrorEventArgs }
    | { type: 'commit', args: DotvvmErrorEventArgs }
    | { type: 'csrfToken' }
    | { type: 'json' }
    | { type: 'serverError', status: number, responseObject: any }
    | { type: 'event' }
    & { options?: PostbackOptions }

interface AdditionalPostbackData {
    [key: string]: any
    validationTargetPath?: string
}

class PostbackOptions {
    public readonly additionalPostbackData: AdditionalPostbackData = {};
    constructor(public readonly postbackId: number, public readonly sender?: HTMLElement, public readonly args : any[] = [], public readonly viewModel?: any, public readonly viewModelName?: string) {}
}

class ConfirmPostBackHandler implements DotvvmPostbackHandler {
    constructor(public message: string) { }
    execute<T>(callback: () => Promise<T>, options: PostbackOptions): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            if (confirm(this.message)) {
                callback().then(resolve, reject)
            } else {
                reject({type: "handler", handler: this, message: "The postback was not confirmed"})
            }
        });
    }
}

class SuppressPostBackHandler implements DotvvmPostbackHandler {
    constructor(public suppress) { }
    execute<T>(callback: () => Promise<T>, options: PostbackOptions): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            if (this.suppress) {
                reject({ type: "handler", handler: this, message: "The postback was suppressed" })
            } else {
                callback().then(resolve, reject)
            }
        });
    }
}


type DotvvmPostBackHandlerConfiguration = {
    name: string;
    options: (context: KnockoutBindingContext) => any;
}

type ClientFriendlyPostbackHandlerConfiguration =
    | string // just a name
    | DotvvmPostbackHandler // the handler itself
    | DotvvmPostBackHandlerConfiguration // the verbose configuration
    | [string, object] // compressed configuration - [name, handler options]
    | [string, (context: KnockoutBindingContext, data: any) => any] // compressed configuration with binding support - [name, context => handler options]
