
// DotvvmEvent is used because CustomEvent is not browser compatible and does not support
// calling missed events for handler that subscribed too late.
class DotvvmEvent<T> {
    private _handlers: DotvvmEventHandler<T>[] = [];
    private _history: T[] = [];

    constructor(public readonly name: string, private readonly triggerMissedEventsOnSubscribe: boolean = false) {
    }

    public subscribe(handler: (data: T) => void) {
        this._handlers.push(new DotvvmEventHandler<T>(handler, false));

        if (this.triggerMissedEventsOnSubscribe) {
            for (const h of this._history) {
                handler(h);
            }
        }
    }

    public subscribeOnce(handler: (data: T) => void) {
        this._handlers.push(new DotvvmEventHandler<T>(handler, true));
    }

    public unsubscribe(handler: (data: T) => void) {
        for (var i = 0; i < this._handlers.length; i++) {
            if (this._handlers[i].handler === handler) {
                this._handlers.splice(i, 1);
                return;
            }
        }
    }

    public trigger(data: T): void {
        for (var i = 0; i < this._handlers.length; i++) {
            this._handlers[i].handler(data);
            if (this._handlers[i].isOneTime) {
                this._handlers.splice(i, 1);
                i--;
            }
        }

        if (this.triggerMissedEventsOnSubscribe) {
            this._history.push(data);
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

export function createPostbackArgs(options: PostbackOptions) {
    return {
        postbackClientId: options.postbackId,
        viewModelName: options.viewModelName || "root",
        viewModel: options.viewModel,
        sender: options.sender,
        postbackOptions: options
    }
}
