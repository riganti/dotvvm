import * as counter from './counter'
import { postbackCore } from './postbackCore'

const globalPostbackHandlers: (ClientFriendlyPostbackHandlerConfiguration)[] = [this.suppressOnDisabledElementHandler, this.isPostBackRunningHandler, this.postbackHandlersStartedEventHandler]
const globalLaterPostbackHandlers: (ClientFriendlyPostbackHandlerConfiguration)[] = [this.postbackHandlersCompletedEventHandler, this.beforePostbackEventPostbackHandler]

export async function postBack(viewModelName: string, sender: HTMLElement, path: string[], command: string, controlUniqueId: string, context?: any, handlers?: ClientFriendlyPostbackHandlerConfiguration[], commandArgs?: any[]): Promise<DotvvmAfterPostBackEventArgs> {
    context = context || ko.contextFor(sender);

    const preparedHandlers = findPostbackHandlers(context, globalPostbackHandlers.concat(handlers || []).concat(this.globalLaterPostbackHandlers));
    if (preparedHandlers.filter(h => h.name && h.name.indexOf("concurrency-") == 0).length == 0) {
        // add a default concurrency handler if none is specified
        preparedHandlers.push(this.defaultConcurrencyPostbackHandler);
    }
    const options = new PostbackOptions(counter.backUpPostBackConter(), sender, commandArgs, context.$data, viewModelName)
    const promise = this.applyPostbackHandlersCore(options => {
        return postbackCore(options, path, command, controlUniqueId, context, commandArgs)
    }, options, preparedHandlers);

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

function applyPostbackHandlers(next: (options: PostbackOptions) => Promise<PostbackCommitFunction | undefined>, sender: HTMLElement, handlers?: ClientFriendlyPostbackHandlerConfiguration[], args: any[] = [], context = ko.contextFor(sender), viewModel = context.$root, viewModelName?: string): Promise<DotvvmAfterPostBackEventArgs> {
    const options = new PostbackOptions(this.backUpPostBackConter(), sender, args, viewModel, viewModelName);

    var postbackCommit = next;
    

    const handlers = this.findPostbackHandlers(context, this.globalPostbackHandlers.concat(handlers || []).concat(this.globalLaterPostbackHandlers));
    const sortedHandlers = this.sortHandlers(handlers);
    for (let handler of sortedHandlers) {
        if (typeof postbackCommit !== "function") {
            return Promise.resolve(new DotvvmAfterPostBackEventArgs(options, null, postbackCommit));
        }
        postbackCommit;
    }

    const promise = this.applyPostbackHandlersCore(callback, options, )
        .then(r => r(), r => Promise.reject(r))

    promise.catch(reason => { if (reason) console.log("Rejected: " + reason) });

    return promise;
}

async function applyPostbackHandlersCore(next: (options: PostbackOptions) => Promise<PostbackCommitFunction | undefined>, options: PostbackOptions, handlers?: DotvvmPostbackHandler[]): Promise<PostbackCommitFunction> {
    const processResult = t => typeof t == "function" ? t : (() => )
    if (handlers == null || handlers.length === 0) {
        
    } else {
        
        return sortedHandlers
            .reduceRight(
                (prev, val, index) => () =>
                    val.execute(prev, options),
                () => callback(options).then(processResult, r => Promise.reject(r))
            )();
    }
}

