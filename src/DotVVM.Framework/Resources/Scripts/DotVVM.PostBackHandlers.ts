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

class DisableButtonPostbackHandler extends DotvvmPostBackHandler {
    constructor(public reenable: boolean) {
        super();
    }

    public execute(callback: () => IDotvvmPromise<DotvvmAfterPostBackEventArgs>, sender: HTMLElement) {
        if (sender instanceof HTMLButtonElement || sender instanceof HTMLInputElement) {
            sender.disabled = true;
        }
        callback().done(() => {
            if (this.reenable) if (sender instanceof HTMLButtonElement || sender instanceof HTMLInputElement) {
                sender.disabled = false;
            }
        });
    }
}

class DotvvmPostBackHandlers {
    public confirm = (options: any) => new ConfirmPostBackHandler(<string>options.message);
    public disableButton = (options: any) => new DisableButtonPostbackHandler(<boolean>options.reenable);
}

interface IDotvvmPostBackHandlerConfiguration {
    name: string;
    options: () => any;
}