class DotvvmEvents {
    public init = new DotvvmEvent<DotvvmEventArgs>("dotvvm.events.init", true);
    public beforePostback = new DotvvmEvent<DotvvmBeforePostBackEventArgs>("dotvvm.events.beforePostback");
    public afterPostback = new DotvvmEvent<DotvvmAfterPostBackEventArgs>("dotvvm.events.afterPostback");
    public error =  new DotvvmEvent<DotvvmErrorEventArgs>("dotvvm.events.error");
    public spaNavigating = new DotvvmEvent<DotvvmSpaNavigatingEventArgs>("dotvvm.events.spaNavigating");
    public spaNavigated = new DotvvmEvent<DotvvmSpaNavigatedEventArgs>("dotvvm.events.spaNavigated");
    public redirect = new DotvvmEvent<DotvvmRedirectEventArgs>("dotvvm.events.redirect");
    public postbackHandlersStarted = new DotvvmEvent<{}>("dotvvm.events.postbackHandlersStarted")
    public postbackHandlersCompleted = new DotvvmEvent<{}>("dotvvm.events.postbackHandlersCompleted")
    public postbackResponseReceived = new DotvvmEvent<{}>("dotvvm.events.postbackResponseReceived")
    public postbackCommitInvoked = new DotvvmEvent<{}>("dotvvm.events.postbackCommitInvoked")
    public postbackViewModelUpdated = new DotvvmEvent<{}>("dotvvm.events.postbackViewModelUpdated")
    public postbackRejected = new DotvvmEvent<{}>("dotvvm.events.postbackRejected")

    public staticCommandMethodInvoking = new DotvvmEvent<{args: any[], command: string}>("dotvvm.events.staticCommandMethodInvoking")
    public staticCommandMethodInvoked = new DotvvmEvent<{args: any[], command: string, result: any}>("dotvvm.events.staticCommandMethodInvoked")
    public staticCommandMethodFailed = new DotvvmEvent<{args: any[], command: string, xhr: XMLHttpRequest, error?: any}>("dotvvm.events.staticCommandMethodInvoked")
}

// DotvvmEvent is used because CustomEvent is not browser compatible and does not support
// calling missed events for handler that subscribed too late.
class DotvvmEvent<T> {
    private handlers : ((f:T) => void)[] = [];
    private history : T[] = [];

    constructor(public readonly name: string, private readonly triggerMissedEventsOnSubscribe: boolean = false) {
    }

    public subscribe(handler: (data: T) => void) {
        this.handlers.push(handler);

        if (this.triggerMissedEventsOnSubscribe) {
            for (var i = 0; i < this.history.length; i++) {
                handler(history[i]);
            }
        }
    }

    public unsubscribe(handler: (data: T) => void) {
        var index = this.handlers.indexOf(handler);
        if (index >= 0) {
            this.handlers = this.handlers.splice(index, 1);
        }
    }

    public trigger(data: T): void {
        for (var i = 0; i < this.handlers.length; i++) {
            this.handlers[i](data);
        }

        if (this.triggerMissedEventsOnSubscribe) {
            this.history.push(data);
        }
    }
}

interface PostbackEventArgs extends DotvvmEventArgs {
    postbackClientId: number
    viewModelName: string
    sender?: Element
    xhr?: XMLHttpRequest
    serverResponseObject?: any
}

interface DotvvmEventArgs {
    viewModel: any
}
class DotvvmErrorEventArgs implements PostbackEventArgs {
    public handled = false;
    constructor(public sender: Element | undefined, public viewModel: any, public viewModelName: any, public xhr: XMLHttpRequest, public postbackClientId, public serverResponseObject: any = undefined, public isSpaNavigationError: boolean = false) {
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
class DotvvmSpaNavigatingEventArgs implements DotvvmEventArgs {
    public cancel: boolean = false;
    constructor(public viewModel: any, public viewModelName: string, public newUrl: string) {
    }
}
class DotvvmSpaNavigatedEventArgs implements DotvvmEventArgs {
    public isHandled: boolean = false;
    constructor(public viewModel: any, public viewModelName: string, public serverResponseObject: any, public xhr?: XMLHttpRequest) {
    }
}
class DotvvmRedirectEventArgs implements DotvvmEventArgs {
    public isHandled: boolean = false;
    constructor(public viewModel: any, public viewModelName: string, public url: string, public replace: boolean) {
    }
}
