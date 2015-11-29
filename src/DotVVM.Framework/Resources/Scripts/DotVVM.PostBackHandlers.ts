class DotvvmPostBackHandler {
    public execute(callback: () => void, sender: HTMLElement) {
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

class DotvvmPostBackHandlers {
    public confirm = (options: any) => new ConfirmPostBackHandler(<string>options.message);
}

interface IDotvvmPostBackHandlerConfiguration {
    name: string;
    options: () => any;
}