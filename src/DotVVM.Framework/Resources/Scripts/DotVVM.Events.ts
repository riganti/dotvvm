
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

export const init = new DotvvmEvent<DotvvmEventArgs>("dotvvm.events.init", true);
export const beforePostback = new DotvvmEvent<DotvvmBeforePostBackEventArgs>("dotvvm.events.beforePostback");
export const afterPostback = new DotvvmEvent<DotvvmAfterPostBackEventArgs>("dotvvm.events.afterPostback");
export const error = new DotvvmEvent<DotvvmErrorEventArgs>("dotvvm.events.error");
export const spaNavigating = new DotvvmEvent<DotvvmSpaNavigatingEventArgs>("dotvvm.events.spaNavigating");
export const spaNavigated = new DotvvmEvent<DotvvmSpaNavigatedEventArgs>("dotvvm.events.spaNavigated");
export const redirect = new DotvvmEvent<DotvvmRedirectEventArgs>("dotvvm.events.redirect");
export const postbackHandlersStarted = new DotvvmEvent<{}>("dotvvm.events.postbackHandlersStarted");
export const postbackHandlersCompleted = new DotvvmEvent<{}>("dotvvm.events.postbackHandlersCompleted");
export const postbackResponseReceived = new DotvvmEvent<{}>("dotvvm.events.postbackResponseReceived");
export const postbackCommitInvoked = new DotvvmEvent<{}>("dotvvm.events.postbackCommitInvoked");
export const postbackViewModelUpdated = new DotvvmEvent<{}>("dotvvm.events.postbackViewModelUpdated");
export const postbackRejected = new DotvvmEvent<{}>("dotvvm.events.postbackRejected");
export const staticCommandMethodInvoking = new DotvvmEvent<{ args: any[], command: string }>("dotvvm.events.staticCommandMethodInvoking");
export const staticCommandMethodInvoked = new DotvvmEvent<{ args: any[], command: string, result: any, xhr: XMLHttpRequest }>("dotvvm.events.staticCommandMethodInvoked");
export const staticCommandMethodFailed = new DotvvmEvent<{ args: any[], command: string, xhr: XMLHttpRequest, error?: any }>("dotvvm.events.staticCommandMethodInvoked");

class DotvvmEventHandler<T> {
    constructor(public handler: (f: T) => void, public isOneTime: boolean) {
    }
}
