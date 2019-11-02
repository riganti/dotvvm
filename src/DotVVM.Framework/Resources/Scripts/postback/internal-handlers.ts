import { events, createPostbackArgs } from "../DotVVM.Events";
import { DotvvmPostbackError } from "../shared-classes";
import { isElementDisabled } from "../utils/dom";

export const suppressOnDisabledElementHandler: DotvvmPostbackHandler = {
    name: "suppressOnDisabledElement",
    before: ["setIsPostbackRunning", "concurrency-default", "concurrency-queue", "concurrency-deny"],
    execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
        if (isElementDisabled(options.sender)) {
            return Promise.reject(new DotvvmPostbackError({
                type: "handler",
                handler: suppressOnDisabledElementHandler,
                message: "PostBack is prohibited on disabled element"
            }))
        }
        else return next()
    }
}

export const beforePostbackEventPostbackHandler: DotvvmPostbackHandler = {
    execute: (next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) => {

        // trigger beforePostback event
        const beforePostbackArgs: DotvvmBeforePostBackEventArgs = {
            ...createPostbackArgs(options),
            cancel: false
        };
        events.beforePostback.trigger(beforePostbackArgs);
        if (beforePostbackArgs.cancel) {
            return Promise.reject(new DotvvmPostbackError({ type: "event", options }));
        }
        return next();
    }
}

export const isPostbackRunning = ko.observable(false)

let postbackCount = 0;

export const isPostBackRunningHandler: DotvvmPostbackHandler = {
    name: "setIsPostbackRunning",
    before: ["eventInvoke-postbackHandlersStarted"],
    async execute(next: () => Promise<PostbackCommitFunction>) {
        isPostbackRunning(true)
        postbackCount++
        try {
            return await next()
        }
        finally {
            isPostbackRunning(!!--postbackCount)
        }
    }
}
