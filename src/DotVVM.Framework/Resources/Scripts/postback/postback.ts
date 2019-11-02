import * as counter from './counter'
import { postbackCore } from './postbackCore'
import { getViewModel } from '../dotvvm-base'

const globalPostbackHandlers: (ClientFriendlyPostbackHandlerConfiguration)[] = [this.suppressOnDisabledElementHandler, this.isPostBackRunningHandler, this.postbackHandlersStartedEventHandler]
const globalLaterPostbackHandlers: (ClientFriendlyPostbackHandlerConfiguration)[] = [this.postbackHandlersCompletedEventHandler, this.beforePostbackEventPostbackHandler]

export async function postBack(viewModelName: string, sender: HTMLElement, path: string[], command: string, controlUniqueId: string, context?: any, handlers?: ClientFriendlyPostbackHandlerConfiguration[], commandArgs?: any[]): Promise<DotvvmAfterPostBackEventArgs> {
    context = context || ko.contextFor(sender);

    const preparedHandlers = findPostbackHandlers(context, globalPostbackHandlers.concat(handlers || []).concat(this.globalLaterPostbackHandlers));
    if (preparedHandlers.filter(h => h.name && h.name.indexOf("concurrency-") == 0).length == 0) {
        // add a default concurrency handler if none is specified
        preparedHandlers.push(this.defaultConcurrencyPostbackHandler);
    }
    const options: PostbackOptions = { 
        postbackId: counter.backUpPostBackCounter(), 
        sender: sender, 
        args: commandArgs!, 
        viewModel: context.$data,
        additionalPostbackData: {}
    };

    const postbackCommit = () => postbackCore(options, path, command, controlUniqueId, context, commandArgs);

    await applyPostbackHandlersCore(postbackCommit, options, preparedHandlers);

    const result = promise.then(
        r => r().then(r => r, error => Promise.reject({ type: "commit", args: error })),
        r => Promise.reject(r)
    );
    result.then(
        r => r && this.events.afterPostback.trigger(r),
        (error: PostbackRejectionReason) => {
            const wasInterrupted = error.type == "handler" || error.type == "event"
            const afterPostBackArgsCanceled: DotvvmAfterPostBackEventArgs = {
                serverResponseObject: error.type == "commit" && error.args ? error.args.serverResponseObject : null,
                wasInterrupted
            }
            if (wasInterrupted) {
                // trigger afterPostback event
                events.postbackRejected.trigger({})
            } else if (error.type == "network") {
                events.error.trigger(error.args)
            }
            events.afterPostback.trigger(afterPostBackArgsCanceled)
        });
    return result;
}

function findPostbackHandlers(knockoutContext, config: ClientFriendlyPostbackHandlerConfiguration[]) {
    const createHandler = (name, options) => options.enabled === false ? null : this.getPostbackHandler(name)(options);
    return <DotvvmPostbackHandler[]>config.map(h =>
        typeof h == 'string' ? createHandler(h, {}) :
            this.isPostbackHandler(h) ? h :
                h instanceof Array ? (() => {
                    const [name, opt] = h;
                    return createHandler(name, typeof opt == "function" ? opt(knockoutContext, knockoutContext.$data) : opt);
                })() :
                    createHandler(h.name, h.options && h.options(knockoutContext)))
        .filter(h => h != null)
}

function applyPostbackHandlers(
    next: () => Promise<PostbackCommitFunction>, 
    options: PostbackOptions,
    sender: HTMLElement, 
    handlers?: DotvvmPostbackHandler[], 
    args: any[] = [], 
    context = ko.contextFor(sender), 
    viewModel = context.$root, 
    viewModelName?: string
    ): Promise<DotvvmAfterPostBackEventArgs> {

    const sortedHandlers: DotvvmPostbackHandler[] = this.sortHandlers(handlers);
    for (let handler of sortedHandlers) {
        next = handler.execute(next, options);
    }

    const promise = this.applyPostbackHandlersCore(callback, options, )
        .then(r => r(), r => Promise.reject(r))

    promise.catch(reason => { if (reason) console.log("Rejected: " + reason) });

    return promise;
}

function applyPostbackHandlersForStaticCommand(
    next: () => Promise<PostbackCommitFunction>, 
    sender: HTMLElement, 
    handlerConfigurations: ClientFriendlyPostbackHandlerConfiguration[]
    ): Promise<DotvvmAfterPostBackEventArgs> {

    const options: PostbackOptions = {
        postbackId: counter.backUpPostBackCounter(), 
        sender: sender, 
        args: [], 
        viewModel: getViewModel(),
        additionalPostbackData: {}
    };
    const context = ko.contextFor(sender);

    const handlers = findPostbackHandlers(context, globalPostbackHandlers.concat(handlerConfigurations || []).concat(globalLaterPostbackHandlers));

    return applyPostbackHandlersCore(next, options, handlers);
}

async function applyPostbackHandlersCore(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions, handlers?: DotvvmPostbackHandler[]): Promise<PostbackCommitFunction> {
    const processResult = t => typeof t == "function" ? t : (() => )
    if (handlers == null || handlers.length === 0) {
        var result = await next;
        return await processResult(result);
    } else {
        const sortedHandlers = this.sortHandlers(handlers);
        return sortedHandlers
            .reduceRight(
            (prev, val, index) => () =>
                val.execute(prev, options),
            () => callback(options).then(processResult, r => Promise.reject(r))
            )();
    }
}

function wrapAsPromise(value: any, options: PostbackOptions): Promise<DotvvmAfterPostBackEventArgs> {
    if (typeof value === "function") {
        return <Promise<DotvvmAfterPostBackEventArgs>>value;
    }
    else {
        return Promise.resolve<DotvvmAfterPostBackEventArgs>({ postbackOptions: options, serverResponseObject: value, sender: null });
    }
}

