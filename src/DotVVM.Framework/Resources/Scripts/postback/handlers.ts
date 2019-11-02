class ConfirmPostBackHandler implements DotvvmPostbackHandler {
    constructor(public message: string) { }
    async execute<T>(next: () => Promise<T>, options: PostbackOptions): Promise<T> {
        if (confirm(this.message)) {
            return await next();
        } else {
            throw { type: "handler", handler: this, message: "The postback was not confirmed" };
        }
    }
}
export const confirm = (options: any) => new ConfirmPostBackHandler(options.message)

class SuppressPostBackHandler implements DotvvmPostbackHandler {
    constructor(public suppress: boolean) { }
    async execute<T>(next: () => Promise<T>, options: PostbackOptions): Promise<T> {
        if (this.suppress) {
            throw { type: "handler", handler: this, message: "The postback was suppressed" };
        } else {
            return await next();
        }
    }
}
export const suppress = (options: any) => new SuppressPostBackHandler(options.suppress)

function createWindowSetTimeoutHandler(time: number): DotvvmPostbackHandler {
    return {
        name: "timeout",
        before: ["eventInvoke-postbackHandlersStarted", "setIsPostbackRunning"],
        async execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
            await new Promise((resolve, reject) => window.setTimeout(resolve, time))
            return await next()
        }
    }
}
const windowSetTimeoutHandler = createWindowSetTimeoutHandler(0);

export const timeout = (options: any) =>
    options.time ? createWindowSetTimeoutHandler(options.time) : windowSetTimeoutHandler
