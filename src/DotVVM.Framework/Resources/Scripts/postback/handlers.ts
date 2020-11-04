import * as internalHandlers from "./internal-handlers";

class ConfirmPostBackHandler implements DotvvmPostbackHandler {
    constructor(public message: string) { }
    public async execute<T>(next: () => Promise<T>, options: PostbackOptions): Promise<T> {
        if (window.confirm(this.message)) {
            return await next();
        } else {
            throw { type: "handler", handler: this, message: "The postback was not confirmed" };
        }
    }
}

class SuppressPostBackHandler implements DotvvmPostbackHandler {
    // tslint:disable-next-line:no-shadowed-variable
    constructor(public suppress: boolean) { }
    public async execute<T>(next: () => Promise<T>, options: PostbackOptions): Promise<T> {
        if (this.suppress) {
            throw { type: "handler", handler: this, message: "The postback was suppressed" };
        } else {
            return await next();
        }
    }
}

function createWindowSetTimeoutHandler(time: number): DotvvmPostbackHandler {
    return {
        name: "timeout",
        before: ["setIsPostbackRunning"],
        async execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
            await new Promise((resolve, reject) => window.setTimeout(resolve, time))
            return await next()
        }
    }
}
const windowSetTimeoutHandler = createWindowSetTimeoutHandler(0);

export const confirm = (options: any) => new ConfirmPostBackHandler(options.message)
export const suppress = (options: any) => new SuppressPostBackHandler(options.suppress)
export const timeout = (options: any) => options.time ? createWindowSetTimeoutHandler(options.time) : windowSetTimeoutHandler

export const postbackHandlers: DotvvmPostbackHandlerCollection = buildPostbackHandlers();

function buildPostbackHandlers() {
    return {
        "confirm": confirm,
        "suppress": suppress,
        "timeout": timeout,
        "concurrency-default": internalHandlers.concurrencyDefault,
        "concurrency-deny": internalHandlers.concurrencyDeny,
        "concurrency-queue": internalHandlers.concurrencyQueue,
        "suppressOnUpdating": internalHandlers.suppressOnUpdating
    };
}

export function getPostbackHandler(name: string) {
    const handler = postbackHandlers[name];
    if (handler) {
        return handler;
    } else {
        throw new Error(`Could not find postback handler of name '${name}'.`);
    }
}

export const defaultConcurrencyPostbackHandler = postbackHandlers["concurrency-default"]({});
