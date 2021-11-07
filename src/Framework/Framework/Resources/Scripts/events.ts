
// DotvvmEvent is used because CustomEvent is not browser compatible and does not support

// calling missed events for handler that subscribed too late.
export class DotvvmEvent<T> {
    private handlers: Array<DotvvmEventHandler<T>> = [];
    private history: T[] = [];

    constructor(public readonly name: string, private readonly triggerMissedEventsOnSubscribe: boolean = false) {
    }

    public subscribe(handler: (data: T) => void) {
        if (compileConstants.debug && typeof handler != 'function') {
            throw new Error(`handler ${handler} is not a function`);
        }
        this.handlers.push(new DotvvmEventHandler<T>(handler, false));

        if (this.triggerMissedEventsOnSubscribe) {
            for (const h of this.history) {
                handler(h);
            }
        }
    }

    public subscribeOnce(handler: (data: T) => void) {
        if (compileConstants.debug && typeof handler != 'function') {
            throw new Error(`handler ${handler} is not a function`);
        }
        this.handlers.push(new DotvvmEventHandler<T>(handler, true));
    }

    public unsubscribe(handler: (data: T) => void) {
        for (let i = 0; i < this.handlers.length; i++) {
            if (this.handlers[i].handler === handler) {
                this.handlers.splice(i, 1);
                return;
            }
        }
    }

    public trigger(data: T): void {
        for (let i = 0; i < this.handlers.length; i++) {
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

export const init = new DotvvmEvent<DotvvmInitEventArgs>("dotvvm.events.init", true);
export const initCompleted = new DotvvmEvent<DotvvmInitCompletedEventArgs>("dotvvm.events.initCompleted", true);
export const beforePostback = new DotvvmEvent<DotvvmBeforePostBackEventArgs>("dotvvm.events.beforePostback");
export const afterPostback = new DotvvmEvent<DotvvmAfterPostBackEventArgs>("dotvvm.events.afterPostback");
export const error = new DotvvmEvent<DotvvmErrorEventArgs>("dotvvm.events.error");
export const redirect = new DotvvmEvent<DotvvmRedirectEventArgs>("dotvvm.events.redirect");
export const postbackHandlersStarted = new DotvvmEvent<DotvvmPostbackHandlersStartedEventArgs>("dotvvm.events.postbackHandlersStarted");
export const postbackHandlersCompleted = new DotvvmEvent<DotvvmPostbackHandlersCompletedEventArgs>("dotvvm.events.postbackHandlersCompleted");
export const postbackResponseReceived = new DotvvmEvent<DotvvmPostbackResponseReceivedEventArgs>("dotvvm.events.postbackResponseReceived");
export const postbackCommitInvoked = new DotvvmEvent<DotvvmPostbackCommitInvokedEventArgs>("dotvvm.events.postbackCommitInvoked");
export const postbackViewModelUpdated = new DotvvmEvent<DotvvmPostbackViewModelUpdatedEventArgs>("dotvvm.events.postbackViewModelUpdated");
export const postbackRejected = new DotvvmEvent<DotvvmPostbackRejectedEventArgs>("dotvvm.events.postbackRejected");
export const staticCommandMethodInvoking = new DotvvmEvent<DotvvmStaticCommandMethodInvokingEventArgs>("dotvvm.events.staticCommandMethodInvoking");
export const staticCommandMethodInvoked = new DotvvmEvent<DotvvmStaticCommandMethodInvokedEventArgs>("dotvvm.events.staticCommandMethodInvoked");
export const staticCommandMethodFailed = new DotvvmEvent<DotvvmStaticCommandMethodFailedEventArgs>("dotvvm.events.staticCommandMethodInvoked");

export const newState = new DotvvmEvent<RootViewModel>("dotvvm.events.newState");

class DotvvmEventHandler<T> {
    constructor(public handler: (f: T) => void, public isOneTime: boolean) {
    }
}
