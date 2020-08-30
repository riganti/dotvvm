import * as events from "../events";
import * as gate from "./gate";
import { DotvvmPostbackError } from "../shared-classes";
import { isElementDisabled } from "../utils/dom";
import { getPostbackQueue, enterActivePostback, leaveActivePostback, runNextInQueue } from "./queue";
import { getLastStartedPostbackId } from "./postbackCore";
import { getIsViewModelUpdating } from "./updater";

let postbackCount = 0;

export const isPostbackRunning = ko.observable(false)

export const suppressOnDisabledElementHandler: DotvvmPostbackHandler = {
    name: "suppressOnDisabledElement",
    before: ["setIsPostbackRunning", "concurrency-default", "concurrency-queue", "concurrency-deny"],
    execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
        if (isElementDisabled(options.sender)) {
            return Promise.reject(new DotvvmPostbackError({
                type: "handler",
                handlerName: "suppressOnDisabledElement",
                message: "PostBack is prohibited on disabled element"
            }));
        } else {
            return next();
        }
    }
};

export const isPostBackRunningHandler: DotvvmPostbackHandler = {
    name: "setIsPostbackRunning",
    async execute(next: () => Promise<PostbackCommitFunction>) {
        isPostbackRunning(true)
        postbackCount++
        try {
            return await next();
        } finally {
            isPostbackRunning(!!--postbackCount);
        }
    }
};

export const concurrencyDefault = (o: any) => ({
    name: "concurrency-default",
    before: ["setIsPostbackRunning"],
    execute: (next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) => {
        return commonConcurrencyHandler(next(), options, o.q || "default")
    }
});

export const concurrencyDeny = (o: any) => ({
    name: "concurrency-deny",
    before: ["setIsPostbackRunning"],
    execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
        const queue = o.q || "default";
        if (getPostbackQueue(queue).noRunning > 0) {
            return Promise.reject(new DotvvmPostbackError({
                type: "handler",
                handlerName: "concurrency-deny",
                message: "An postback is already running"
            }));
        }
        return commonConcurrencyHandler(next(), options, queue);
    }
});

export const concurrencyQueue = (o: any) => ({
    name: "concurrency-queue",
    before: ["setIsPostbackRunning"],
    execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
        const queue = o.q || "default";
        const handler = () => commonConcurrencyHandler(next(), options, queue);

        if (getPostbackQueue(queue).noRunning > 0) {
            return new Promise<PostbackCommitFunction>(resolve => {
                getPostbackQueue(queue).queue.push(() => resolve(handler()));
            });
        }
        return handler();
    }
});

export const suppressOnUpdating = (o: any) => ({
    name: "suppressOnUpdating",
    before: ["setIsPostbackRunning", "concurrency-default", "concurrency-queue", "concurrency-deny"],
    execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
        if (getIsViewModelUpdating()) {
            return Promise.reject(new DotvvmPostbackError({
                type: "handler",
                handlerName: "suppressOnUpdating",
                message: "ViewModel is updating, so it's probably false onchange event"
            }));
        } else {
            return next();
        }
    }
})

export function isPostbackStillActive(id: number) {
    return getLastStartedPostbackId() == id && !gate.isPostbackDisabled(id)
}

function commonConcurrencyHandler<T>(promise: Promise<PostbackCommitFunction>, options: PostbackOptions, queueName: string): Promise<PostbackCommitFunction> {
    enterActivePostback(queueName);

    const dispatchNext = async () => {
        // run the next postback after everything about this one is finished (after, error events, ...)
        await Promise.resolve()

        leaveActivePostback(queueName)
        runNextInQueue(queueName)
    }

    return promise.then(innerCommit => {
        return async () => {
            try {
                if (isPostbackStillActive(options.postbackId)) {
                    return await innerCommit();
                } else {
                    throw new DotvvmPostbackError({ type: "commit" })
                }
            } finally {
                dispatchNext()
            }
        };
    }, error => {
        dispatchNext()
        return Promise.reject(error)
    });
}
