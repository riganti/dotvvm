type DotvvmPostbackHandler2 = {
    execute<T>(callback: () => Promise<T>, sender: HTMLElement): Promise<T>
}

type PostbackRejectionReason =
    | { type: "handler", handler: DotvvmPostbackHandler2 | DotvvmPostBackHandler, message?: string }

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

class ConfirmPostBackHandler2 implements DotvvmPostbackHandler2 {
    constructor(public message: string) { }
    execute<T>(callback: () => Promise<T>, sender: HTMLElement): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            if (confirm(this.message)) {
                callback().then(resolve, reject)
            } else {
                reject({type: "handler", handler: this, message: "The postback was not confirmed"})
            }
        });
    }
}

interface IDotvvmPostBackHandlerConfiguration {
    name: string;
    options: () => any;
}