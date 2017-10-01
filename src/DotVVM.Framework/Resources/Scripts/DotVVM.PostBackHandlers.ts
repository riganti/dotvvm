type DotvvmPostbackHandler2 = {
    execute<T>(callback: () => Promise<T>, options: PostbackOptions): Promise<T>
    name?: string
    after?: (string | DotvvmPostbackHandler2)[]
    before?: (string | DotvvmPostbackHandler2)[]
}

type PostbackRejectionReason =
    | { type: "handler", handler: DotvvmPostbackHandler2 | DotvvmPostBackHandler, message?: string }
    | { type: 'network', args: DotvvmErrorEventArgs }
    | { type: 'commit', args: DotvvmErrorEventArgs }
    | { type: 'event' }
    & { options?: PostbackOptions }

class DotvvmPostBackHandler {
    public execute<T>(callback: () => void, sender: HTMLElement): void {
    }
}

class ConfirmPostBackHandler extends DotvvmPostBackHandler {
    constructor(public message: string) {
        super();
    }

    public execute(callback: () => void, sender: HTMLElement) {
        if (confirm(this.message)) {
            callback();
        }
    }
}

interface AdditionalPostbackData {
    [key: string]: any
    validationTargetPath?: string
}

class PostbackOptions {
    public readonly additionalPostbackData: AdditionalPostbackData = {};
    constructor(public readonly postbackId: number, public readonly sender?: HTMLElement, public readonly args : any[] = [], public readonly viewModel?: any, public readonly viewModelName?: string) {}
}

class ConfirmPostBackHandler2 implements DotvvmPostbackHandler2 {
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

type DotvvmPostBackHandlerConfiguration = {
    name: string;
    options: (context: KnockoutBindingContext) => any;
}

type ClientFriendlyPostbackHandlerConfiguration =
    | string // just a name
    | DotvvmPostbackHandler2 // the handler itself
    | DotvvmPostBackHandlerConfiguration // the verbose configuration
    | [string, object] // compressed configuration - [name, handler options]
    | [string, (context: KnockoutBindingContext, data: any) => any] // compressed configuration with binding support - [name, context => handler options]