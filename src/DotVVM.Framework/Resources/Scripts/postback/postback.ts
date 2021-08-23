import * as counter from './counter'
import { postbackCore, throwIfAborted } from './postbackCore'
import { getViewModel } from '../dotvvm-base'
import { defaultConcurrencyPostbackHandler, getPostbackHandler } from './handlers';
import * as internalHandlers from './internal-handlers';
import * as events from '../events';
import * as gate from './gate';
import { DotvvmPostbackError } from '../shared-classes';
import { logError } from '../utils/logging';
import { handleRedirect } from './redirect';

const globalPostbackHandlers: (ClientFriendlyPostbackHandlerConfiguration)[] = [
    internalHandlers.suppressOnDisabledElementHandler,
    internalHandlers.isPostBackRunningHandler
];
const globalLaterPostbackHandlers: (ClientFriendlyPostbackHandlerConfiguration)[] = [];

export async function postBack(
    sender: HTMLElement,
    path: string[],
    command: string,
    controlUniqueId: string,
    context?: any,
    handlers?: ClientFriendlyPostbackHandlerConfiguration[],
    commandArgs?: any[],
    abortSignal?: AbortSignal
): Promise<DotvvmAfterPostBackEventArgs> {
    context = context || ko.contextFor(sender);

    const preparedHandlers = findPostbackHandlers(context, globalPostbackHandlers.concat(handlers || []).concat(globalLaterPostbackHandlers));
    if (preparedHandlers.filter(h => h.name && h.name.indexOf("concurrency-") == 0).length == 0) {
        // add a default concurrency handler if none is specified
        preparedHandlers.push(defaultConcurrencyPostbackHandler);
    }

    const options: PostbackOptions = {
        postbackId: counter.backUpPostBackCounter(),
        sender,
        args: ko.toJS(commandArgs) || [],  // TODO: consult with @exyi to fix it properly. Whether commandArgs should or not be serialized via dotvvm serializer.
        viewModel: context.$data,
        knockoutContext: context,
        commandType: "postback",
        abortSignal
    }

    const coreCallback = (o: PostbackOptions) => postbackCore(o, path, command, controlUniqueId, context, options.args);

    try {
        const wrappedPostbackCommit = await applyPostbackHandlersCore(coreCallback, options, preparedHandlers);
        const result = await wrappedPostbackCommit();
        events.afterPostback.trigger(result);

        return result;

    } catch (err) {
        if (abortSignal && abortSignal.aborted) {
            err = new DotvvmPostbackError({ type: "abort", options })
        }

        if (err instanceof DotvvmPostbackError) {
            const reason = err.reason
            const wasInterrupted = isInterruptingErrorReason(reason);
            const serverResponseObject = extractServerResponseObject(reason);
            
            if (wasInterrupted) {
                // trigger postbackRejected event
                const postbackRejectedEventArgs: DotvvmPostbackRejectedEventArgs = {
                    ...options,
                    error: err
                };
                events.postbackRejected.trigger(postbackRejectedEventArgs)
            }

            // trigger afterPostback event
            const eventArgs: DotvvmAfterPostBackEventArgs = {
                ...options,
                serverResponseObject,
                wasInterrupted,
                commandResult: null,
                response: (reason as any)?.response,
                error: err
            }
            events.afterPostback.trigger(eventArgs);

            if (shouldTriggerErrorEvent(reason)) {
                // trigger error event
                const errorEventArgs: DotvvmErrorEventArgs = {
                    ...options,
                    serverResponseObject,
                    response: (reason as any)?.response,
                    error: err,
                    handled: false
                }
                events.error.trigger(errorEventArgs);
                if (!errorEventArgs.handled) {
                    logError("postback", "Postback failed", errorEventArgs);                    
                } else {
                    return {
                        ...options,
                        serverResponseObject,
                        response: (reason as any)?.response,
                        error: err
                    };
                }
            }
        } else {
            logError("postback", "Unexpected exception during postback.", err);
        }
        throw err;
    }
}

function findPostbackHandlers(knockoutContext: KnockoutBindingContext, config: ClientFriendlyPostbackHandlerConfiguration[]) {
    const createHandler = (name: string, options: any) => options.enabled === false ? null : getPostbackHandler(name)(options);
    return config.map(h => {
        if (typeof h == 'string') {
            return createHandler(h, {});
        } else if (isPostbackHandler(h)) {
            return h;
        } else if (h instanceof Array) {
            const [name, opt] = h;
            return createHandler(name, typeof opt == "function" ? opt(knockoutContext, knockoutContext.$data) : opt);
        } else {
            return createHandler(h.name, h.options && h.options(knockoutContext));
        }
    }).filter(h => h != null) as DotvvmPostbackHandler[];
}

type MaybePromise<T> = Promise<T> | T

export async function applyPostbackHandlers(
    next: (options: PostbackOptions) => MaybePromise<PostbackCommitFunction | any>,
    sender: HTMLElement,
    handlerConfigurations?: ClientFriendlyPostbackHandlerConfiguration[],
    args: any[] = [],
    context = ko.contextFor(sender),
    abortSignal?: AbortSignal
): Promise<DotvvmAfterPostBackEventArgs> {
    const saneNext = (o: PostbackOptions) => {
        return wrapCommitFunction(next(o), o);
    }

    const options: PostbackOptions = {
        postbackId: counter.backUpPostBackCounter(),
        commandType: "staticCommand",
        sender,
        args,
        viewModel: context.$data,
        knockoutContext: context,
        abortSignal
    }

    const handlers = findPostbackHandlers(context, globalPostbackHandlers.concat(handlerConfigurations || []).concat(globalLaterPostbackHandlers));

    try {
        const commit = await applyPostbackHandlersCore(saneNext, options, handlers);
        const result = await commit(...args);
        return result;
    } catch (err) {
        if (abortSignal && abortSignal.aborted) {
            err = new DotvvmPostbackError({ type: "abort", options })
        }
        
        if (err instanceof DotvvmPostbackError) {
            var reason = err.reason;
            if (reason.type == "redirect") {
                return await handleRedirect(options, reason.responseObject, reason.response!)
            }
            else if (shouldTriggerErrorEvent(reason)) {
                // trigger error event
                const serverResponseObject = extractServerResponseObject(reason);
                const errorEventArgs: DotvvmErrorEventArgs = {
                    ...options,
                    serverResponseObject,
                    response: (reason as any)?.response,
                    error: err,
                    handled: false
                }
                events.error.trigger(errorEventArgs);

                if (!errorEventArgs.handled) {
                    logError("static-command", "StaticCommand failed", errorEventArgs);
                } else {
                    return {
                        ...options,
                        serverResponseObject,
                        response: (reason as any)?.response,
                        error: err
                    };
                }
            }
        } else {
            logError("static-command", "Unexpected exception during static command.", err);
        }
        throw err
    }
}

function applyPostbackHandlersCore(next: (options: PostbackOptions) => Promise<PostbackCommitFunction>, options: PostbackOptions, handlers: DotvvmPostbackHandler[]): Promise<PostbackCommitFunction> {
    events.postbackHandlersStarted.trigger(options);

    let fired = false
    const nextWithCheck = (o: PostbackOptions) => {
        if (fired) {
            throw new Error("The same postback can't run twice.");
        }
        fired = true;
        events.postbackHandlersCompleted.trigger(options);
        return next(o);
    }

    const sortedHandlers = sortHandlers(handlers)

    function recursiveCore(index: number): Promise<PostbackCommitFunction> {
        if (gate.isPostbackDisabled(options.postbackId)) {
            throw new DotvvmPostbackError({ type: "gate" })
        }
        throwIfAborted(options)
        if (index == sortedHandlers.length) {
            return nextWithCheck(options);
        } else {
            return sortedHandlers[index].execute(
                () => recursiveCore(index + 1),
                options
            );
        }
    }
    return recursiveCore(0);
}

function wrapCommitFunction(value: MaybePromise<PostbackCommitFunction | any>, options: PostbackOptions): Promise<PostbackCommitFunction> {

    return Promise.resolve(value).then(v => {
        if (typeof v == "function") {
            return <PostbackCommitFunction>value;
        } else {
            return () => Promise.resolve<DotvvmAfterPostBackEventArgs>({
                ...options,
                commandResult: v,
                wasInterrupted: false
            });
        }
    });
}

export function isPostbackHandler(obj: any): obj is DotvvmPostbackHandler {
    return obj && typeof obj.execute == "function";
}

export function sortHandlers(handlers: DotvvmPostbackHandler[]): DotvvmPostbackHandler[] {
    const getHandler = (() => {
        const handlerMap: { [name: string]: DotvvmPostbackHandler } = {};
        for (const h of handlers) {
            if (h.name != null) {
                handlerMap[h.name] = h;
            }
        }
        return (s: string | DotvvmPostbackHandler) => typeof s == "string" ? handlerMap[s] : s;
    })();
    const dependencies = handlers.map((handler, i) => ((<any>handler)["@sort_index"] = i, ({ handler, deps: (handler.after || []).map(getHandler) })));
    for (const h of handlers) {
        if (h.before) {
            for (const before of h.before.map(getHandler)) {
                if (before) {
                    const index = (<any>before)["@sort_index"] as number;
                    dependencies[index].deps.push(h);
                }
            }
        }
    }

    const result: DotvvmPostbackHandler[] = [];
    const doneBitmap = new Uint8Array(dependencies.length);
    const addToResult = (index: number) => {
        switch (doneBitmap[index]) {
            case 0: break;
            case 1: throw new Error("Cyclic PostbackHandler dependency found.");
            case 2: return; // it's already in the list
            default: throw new Error("");
        }
        if (doneBitmap[index] == 1) {
            return;
        }
        doneBitmap[index] = 1;

        const { handler, deps } = dependencies[index];
        for (const d of deps) {
            addToResult((<any>d)["@sort_index"]);
        }

        doneBitmap[index] = 2;
        result.push(handler);
    }
    for (let i = 0; i < dependencies.length; i++) {
        addToResult(i);
    }
    return result;
}

function isInterruptingErrorReason(reason: DotvvmPostbackErrorReason) {
    return ["event", "handler", "abort"].includes(reason.type);
}
function shouldTriggerErrorEvent(reason: DotvvmPostbackErrorReason) {
    return ["network", "serverError" ].includes(reason.type);
}
function extractServerResponseObject(reason: DotvvmPostbackErrorReason | undefined) {
    if (!reason) return null;
    if (reason.type == "commit" && reason.args) {
        return reason.args.serverResponseObject;
    } 
    else if (reason.type == "network") {
        return reason.err;
    } else if (reason.type == "serverError" || reason.type == "redirect") {
        return reason.responseObject;
    }
    return null;
}
