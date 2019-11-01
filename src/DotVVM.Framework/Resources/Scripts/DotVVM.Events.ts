
// DotvvmEvent is used because CustomEvent is not browser compatible and does not support
// calling missed events for handler that subscribed too late.
class DotvvmEvent<T> {
    private handlers: DotvvmEventHandler<T>[] = [];
    private history: T[] = [];

    constructor(public readonly name: string, private readonly triggerMissedEventsOnSubscribe: boolean = false) {
    }

    public subscribe(handler: (data: T) => void) {
        this.handlers.push(new DotvvmEventHandler<T>(handler, false));

        if (this.triggerMissedEventsOnSubscribe) {
            for (const h of this.history) {
                handler(h);
            }
        }
    }

    public subscribeOnce(handler: (data: T) => void) {
        this.handlers.push(new DotvvmEventHandler<T>(handler, true));
    }

    public unsubscribe(handler: (data: T) => void) {
        for (var i = 0; i < this.handlers.length; i++) {
            if (this.handlers[i].handler === handler) {
                this.handlers.splice(i, 1);
                return;
            }
        }
    }

    public trigger(data: T): void {
        for (var i = 0; i < this.handlers.length; i++) {
            this.handlers[i].handler(data);
            if (this.handlers[i].isOneTime) {
                this.handlers.splice(i, 1);
                i--;
            }
        }

        if (this.triggerMissedEventsOnSubscribe) {
            this.history.push(data);
        }
    }
}

export const events = {
    init: new DotvvmEvent<DotvvmEventArgs>("dotvvm.events.init", true),
    beforePostback: new DotvvmEvent<DotvvmBeforePostBackEventArgs>("dotvvm.events.beforePostback"),
    afterPostback: new DotvvmEvent<DotvvmAfterPostBackEventArgs>("dotvvm.events.afterPostback"),
    error: new DotvvmEvent<DotvvmErrorEventArgs>("dotvvm.events.error"),
    spaNavigating: new DotvvmEvent<DotvvmSpaNavigatingEventArgs>("dotvvm.events.spaNavigating"),
    spaNavigated: new DotvvmEvent<DotvvmSpaNavigatedEventArgs>("dotvvm.events.spaNavigated"),
    redirect: new DotvvmEvent<DotvvmRedirectEventArgs>("dotvvm.events.redirect"),

    postbackHandlersStarted: new DotvvmEvent<{}>("dotvvm.events.postbackHandlersStarted"),
    postbackHandlersCompleted: new DotvvmEvent<{}>("dotvvm.events.postbackHandlersCompleted"),
    postbackResponseReceived: new DotvvmEvent<{}>("dotvvm.events.postbackResponseReceived"),
    postbackCommitInvoked: new DotvvmEvent<{}>("dotvvm.events.postbackCommitInvoked"),
    postbackViewModelUpdated: new DotvvmEvent<{}>("dotvvm.events.postbackViewModelUpdated"),
    postbackRejected: new DotvvmEvent<{}>("dotvvm.events.postbackRejected"),

    staticCommandMethodInvoking: new DotvvmEvent<{ args: any[], command: string }>("dotvvm.events.staticCommandMethodInvoking"),
    staticCommandMethodInvoked: new DotvvmEvent<{ args: any[], command: string, result: any, xhr: XMLHttpRequest }>("dotvvm.events.staticCommandMethodInvoked"),
    staticCommandMethodFailed: new DotvvmEvent<{ args: any[], command: string, xhr: XMLHttpRequest, error?: any }>("dotvvm.events.staticCommandMethodInvoked"),
}

class DotvvmEventHandler<T> {
    constructor(public handler: (f: T) => void, public isOneTime: boolean) {
    }
}

interface PostbackEventArgs extends DotvvmEventArgs {
    postbackClientId: number
    viewModelName?: string
    sender?: Element
    xhr?: XMLHttpRequest | null
    serverResponseObject?: any
}

interface DotvvmEventArgs {
    viewModel: any
}
class DotvvmErrorEventArgs implements PostbackEventArgs {
    public handled = false;
    constructor(public sender: Element | undefined, public viewModel: any, public postbackClientId: number, public serverResponseObject: any = undefined, public isSpaNavigationError: boolean = false) {
    }
}
class DotvvmBeforePostBackEventArgs implements PostbackEventArgs {
    public cancel: boolean = false;
    public clientValidationFailed: boolean = false;
    constructor(public sender: HTMLElement, public viewModel: any, public viewModelName: string, public postbackClientId: number) {
    }
}
class DotvvmAfterPostBackEventArgs implements PostbackEventArgs {
    public isHandled: boolean = false;
    public wasInterrupted: boolean = false;
    public get postbackClientId() { return this.postbackOptions.postbackId }
    public get viewModelName() { return this.postbackOptions.viewModelName! }
    public get viewModel() { return this.postbackOptions.viewModel! }
    public get sender() { return this.postbackOptions.sender }
    constructor(public postbackOptions: PostbackOptions, public serverResponseObject: any, public commandResult: any = null, public xhr?: XMLHttpRequest) {
    }
}
class DotvvmAfterPostBackWithRedirectEventArgs extends DotvvmAfterPostBackEventArgs {
    public get redirectPromise(): Promise<DotvvmNavigationEventArgs> | undefined { return this._redirectPromise; }
    constructor(postbackOptions: PostbackOptions, serverResponseObject: any, commandResult: any = null, xhr?: XMLHttpRequest, private _redirectPromise?: Promise<DotvvmNavigationEventArgs>) {
        super(postbackOptions, serverResponseObject, commandResult, xhr);
    }
}
class DotvvmSpaNavigatingEventArgs implements DotvvmEventArgs {
    public cancel: boolean = false;
    constructor(public viewModel: any, public viewModelName: string, public newUrl: string) {
    }
}
class DotvvmNavigationEventArgs implements DotvvmEventArgs {
    public isHandled: boolean = false;
    constructor(public viewModel: any, public viewModelName: string, public serverResponseObject: any, public xhr?: XMLHttpRequest) {
    }
}
class DotvvmSpaNavigatedEventArgs extends DotvvmNavigationEventArgs {
}
class DotvvmRedirectEventArgs implements DotvvmEventArgs {
    public isHandled: boolean = false;
    constructor(public viewModel: any, public viewModelName: string, public url: string, public replace: boolean) {
    }
}
