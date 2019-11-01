/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout/knockout.dotvvm.d.ts" />
/// <reference path="typings/globalize/globalize.d.ts" />

import { getElementByDotvvmId } from './utils/dom'
import { DotvvmValidation } from './DotVVM.Validation'
import * as spa from './spa/spa';
import * as deserialization from './serialization/deserialize'
import * as serialization from './serialization/serialize'
import * as uri from './utils/uri'
import * as http from './postback/http'
import * as magicNavigator from './utils/magic-navigator'

import bindingHandlers from './binding-handlers/all-handlers'

interface IDotvvmPostbackScriptFunction {
    (pageArea: string, sender: HTMLElement, pathFragments: string[], controlId: string, useWindowSetTimeout: boolean, validationTarget: string, context: any, handlers: DotvvmPostBackHandlerConfiguration[]): void
}

interface IDotvvmViewModelInfo {
    viewModel?;
    renderedResources?: string[];
    url?: string;
    virtualDirectory?: string;
}

interface IDotvvmViewModels {
    [name: string]: IDotvvmViewModelInfo
}
type DotvvmStaticCommandResponse = {
    result: any;
} | {
    action: "redirect";
    url: string;
};


interface IDotvvmPostbackHandlerCollection {
    [name: string]: ((options: any) => DotvvmPostbackHandler);
    confirm: (options: { message?: string }) => ConfirmPostBackHandler;
    suppress: (options: { suppress?: boolean }) => SuppressPostBackHandler;
}

export class DotVVM {
    private postBackCounter = 0;
    private lastStartedPostack = 0;
    private isViewModelUpdating: boolean = true;

    // warning this property is referenced in ModelState.cs and KnockoutHelper.cs
    public viewModelObservables: {
        [name: string]: KnockoutObservable<IDotvvmViewModelInfo>;
    } = {};
    public viewModels: IDotvvmViewModels = {};
    public culture: string;
    public serialization = new DotvvmSerialization();

    public postbackHandlers: IDotvvmPostbackHandlerCollection = {
        confirm: (options: any) => new ConfirmPostBackHandler(options.message),
        suppress: (options: any) => new SuppressPostBackHandler(options.suppress),
        timeout: (options: any) => options.time ? this.createWindowSetTimeoutHandler(options.time) : this.windowSetTimeoutHandler,
        "concurrency-default": (o: any) => ({
            name: "concurrency-default",
            before: ["setIsPostbackRunning"],
            execute: (callback: () => Promise<PostbackCommitFunction>, options: PostbackOptions) => {
                return this.commonConcurrencyHandler(callback(), options, o.q || "default")
            }
        }),
        "concurrency-deny": (o: any) => ({
            name: "concurrency-deny",
            before: ["setIsPostbackRunning"],
            execute(callback: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
                var queue = o.q || "default";
                if (dotvvm.getPostbackQueue(queue).noRunning > 0)
                    return Promise.reject({ type: "handler", handler: this, message: "An postback is already running" });
                return dotvvm.commonConcurrencyHandler(callback(), options, queue);
            }
        }),
        "concurrency-queue": (o: any) => ({
            name: "concurrency-queue",
            before: ["setIsPostbackRunning"],
            execute(callback: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
                var queue = o.q || "default";
                var handler = () => dotvvm.commonConcurrencyHandler(callback(), options, queue);

                if (dotvvm.getPostbackQueue(queue).noRunning > 0) {
                    return new Promise<PostbackCommitFunction>(resolve => {
                        dotvvm.getPostbackQueue(queue).queue.push(() => resolve(handler()));
                    })
                }
                return handler();
            }
        }),
        "suppressOnUpdating": (options: any) => ({
            name: "suppressOnUpdating",
            before: ["setIsPostbackRunning", "concurrency-default", "concurrency-queue", "concurrency-deny"],
            execute(callback: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
                if (dotvvm.isViewModelUpdating) return Promise.reject({ type: "handler", handler: this, message: "ViewModel is updating, so it's probably false onchange event" })
                else return callback()
            }
        })
    }

    private suppressOnDisabledElementHandler: DotvvmPostbackHandler = {
        name: "suppressOnDisabledElement",
        before: ["setIsPostbackRunning", "concurrency-default", "concurrency-queue", "concurrency-deny"],
        execute: (callback: () => Promise<PostbackCommitFunction>, options: PostbackOptions) => {
            if (options.sender && dotvvm.isPostBackProhibited(options.sender)) {
                return Promise.reject({ type: "handler", handler: this, message: "PostBack is prohibited on disabled element" })
            }
            else return callback()
        }
    }

    private beforePostbackEventPostbackHandler: DotvvmPostbackHandler = {
        execute: (callback: () => Promise<PostbackCommitFunction>, options: PostbackOptions) => {

            // trigger beforePostback event
            var beforePostbackArgs = new DotvvmBeforePostBackEventArgs(options.sender!, options.viewModel, options.viewModelName!, options.postbackId);
            this.events.beforePostback.trigger(beforePostbackArgs);
            if (beforePostbackArgs.cancel) {
                return Promise.reject({ type: "event", options: options });
            }
            return callback();
        }
    }

    private isPostBackRunningHandler: DotvvmPostbackHandler = (() => {
        let postbackCount = 0;
        return {
            name: "setIsPostbackRunning",
            before: ["eventInvoke-postbackHandlersStarted"],
            execute: (callback: () => Promise<PostbackCommitFunction>, options: PostbackOptions) => {
                this.isPostbackRunning(true)
                postbackCount++
                let promise = callback()
                promise.then(() => this.isPostbackRunning(!!--postbackCount), () => this.isPostbackRunning(!!--postbackCount))
                return promise
            }
        };
    })();

    private createWindowSetTimeoutHandler(time: number): DotvvmPostbackHandler {
        return {
            name: "timeout",
            before: ["eventInvoke-postbackHandlersStarted", "setIsPostbackRunning"],
            execute: <T>(callback: () => Promise<T>, options: PostbackOptions) => {
                return new Promise((resolve, reject) => window.setTimeout(resolve, time))
                    .then(() => callback())
            }
        }
    }
    private windowSetTimeoutHandler: DotvvmPostbackHandler = this.createWindowSetTimeoutHandler(0);

    private commonConcurrencyHandler = <T>(promise: Promise<PostbackCommitFunction>, options: PostbackOptions, queueName: string): Promise<PostbackCommitFunction> => {
        const queue = this.getPostbackQueue(queueName)
        queue.noRunning++
        dotvvm.updateProgressChangeCounter(dotvvm.updateProgressChangeCounter() + 1);

        const dispatchNext = (args) => {
            const drop = () => {
                queue.noRunning--;
                dotvvm.updateProgressChangeCounter(dotvvm.updateProgressChangeCounter() - 1);
                if (queue.queue.length > 0) {
                    const callback = queue.queue.shift()!
                    window.setTimeout(callback, 0)
                }
            }
            if (args instanceof DotvvmAfterPostBackWithRedirectEventArgs && args.redirectPromise) {
                args.redirectPromise.then(drop, drop);
            } else {
                drop();
            }


        }

        return promise.then(result => {
            var p = this.lastStartedPostack == options.postbackId ?
                result :
                () => Promise.reject(null);
            return () => {
                const pr = p()
                pr.then(dispatchNext, dispatchNext)
                return pr
            };
        }, error => {
            dispatchNext(error)
            return Promise.reject(error)
        });
    }

    private defaultConcurrencyPostbackHandler: DotvvmPostbackHandler = this.postbackHandlers["concurrency-default"]({})

    private postbackQueues: { [name: string]: { queue: (() => void)[], noRunning: number } } = {}
    public getPostbackQueue(name = "default") {
        if (!this.postbackQueues[name]) this.postbackQueues[name] = { queue: [], noRunning: 0 }
        return this.postbackQueues[name];
    }

    private postbackHandlersStartedEventHandler: DotvvmPostbackHandler = {
        name: "eventInvoke-postbackHandlersStarted",
        execute: <T>(callback: () => Promise<T>, options: PostbackOptions) => {
            dotvvm.events.postbackHandlersStarted.trigger(options);
            return callback()
        }
    }

    private postbackHandlersCompletedEventHandler: DotvvmPostbackHandler = {
        name: "eventInvoke-postbackHandlersCompleted",
        after: ["eventInvoke-postbackHandlersStarted"],
        execute: <T>(callback: () => Promise<T>, options: PostbackOptions) => {
            dotvvm.events.postbackHandlersCompleted.trigger(options);
            return callback()
        }
    }

    public globalPostbackHandlers: (ClientFriendlyPostbackHandlerConfiguration)[] = [this.suppressOnDisabledElementHandler, this.isPostBackRunningHandler, this.postbackHandlersStartedEventHandler]
    public globalLaterPostbackHandlers: (ClientFriendlyPostbackHandlerConfiguration)[] = [this.postbackHandlersCompletedEventHandler, this.beforePostbackEventPostbackHandler]

    public events = new DotvvmEvents();
    public globalize = new DotvvmGlobalize();
    public validation: DotvvmValidation;
    public extensions: IDotvvmExtensions = {};
    public fileUpload = DotvvmFileUpload;

    public isPostbackRunning = ko.observable(false);
    public updateProgressChangeCounter = ko.observable(0);

    public init(viewModelName: string, culture: string): void {
        this.addKnockoutBindingHandlers();

        // load the viewmodel
        var thisViewModel = this.viewModels[viewModelName] = JSON.parse((<HTMLInputElement>document.getElementById("__dot_viewmodel_" + viewModelName)).value);
        if (thisViewModel.resources) {
            for (const r of Object.keys(thisViewModel.resources)) {
                this.resourceSigns[r] = true;
            }
        }
        if (thisViewModel.renderedResources) {
            thisViewModel.renderedResources.forEach(r => this.resourceSigns[r] = true);
        }
        var idFragment = thisViewModel.resultIdFragment;
        var viewModel = thisViewModel.viewModel = this.serialization.deserialize(this.viewModels[viewModelName].viewModel, {}, true);

        // initialize services
        this.culture = culture;
        this.validation = new DotvvmValidation(this);

        // wrap it in the observable
        this.viewModelObservables[viewModelName] = ko.observable(viewModel);
        ko.applyBindings(this.viewModelObservables[viewModelName], document.documentElement);

        // trigger the init event
        this.events.init.trigger({ viewModel });

        // handle SPA requests
        if (compileConstants.isSpa) {
            spa.init(viewModelName);
        }

        this.isViewModelUpdating = false;

        // persist the viewmodel in the hidden field so the Back button will work correctly
        window.addEventListener("beforeunload", e => {
            this.persistViewModel(viewModelName);
        });
    }

    private persistViewModel(viewModelName: string) {
        var viewModel = this.viewModels[viewModelName];
        var persistedViewModel = {};
        for (const p of Object.keys(viewModel)) {
            persistedViewModel[p] = viewModel[p];
        }
        persistedViewModel["viewModel"] = this.serialization.serialize(persistedViewModel["viewModel"], { serializeAll: true });
        (<HTMLInputElement>document.getElementById("__dot_viewmodel_" + viewModelName)).value = JSON.stringify(persistedViewModel);
    }

    private backUpPostBackConter(): number {
        return ++this.postBackCounter;
    }

    private isPostBackStillActive(currentPostBackCounter: number): boolean {
        return this.postBackCounter === currentPostBackCounter;
    }

    protected getPostbackHandler(name: string) {
        const handler = this.postbackHandlers[name]
        if (handler) {
            return handler
        } else {
            throw new Error(`Could not find postback handler of name '${name}'`)
        }
    }

    private isPostbackHandler(obj: any): obj is DotvvmPostbackHandler {
        return obj && typeof obj.execute == "function"
    }

    public findPostbackHandlers(knockoutContext, config: ClientFriendlyPostbackHandlerConfiguration[]) {
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

    private sortHandlers(handlers: DotvvmPostbackHandler[]): DotvvmPostbackHandler[] {
        const getHandler = (() => {
            const handlerMap: { [name: string]: DotvvmPostbackHandler } = {};
            for (const h of handlers) if (h.name != null) {
                handlerMap[h.name] = h;
            }
            return (s: string | DotvvmPostbackHandler) => typeof s == "string" ? handlerMap[s] : s;
        })();
        const dependencies = handlers.map((handler, i) => (handler["@sort_index"] = i, ({ handler, deps: (handler.after || []).map(getHandler) })));
        for (const h of handlers) {
            if (h.before) for (const before of h.before.map(getHandler)) if (before) {
                const index = before["@sort_index"] as number;
                dependencies[index].deps.push(h);
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
            if (doneBitmap[index] == 1) return;
            doneBitmap[index] = 1;

            const { handler, deps } = dependencies[index];
            for (const d of deps) {
                addToResult(d["@sort_index"]);
            }

            doneBitmap[index] = 2;
            result.push(handler);
        }
        for (let i = 0; i < dependencies.length; i++) {
            addToResult(i);
        }
        return result;
    }

    private applyPostbackHandlersCore(callback: (options: PostbackOptions) => Promise<PostbackCommitFunction | undefined>, options: PostbackOptions, handlers?: DotvvmPostbackHandler[]): Promise<PostbackCommitFunction> {
        const processResult = t => typeof t == "function" ? t : (() => Promise.resolve(new DotvvmAfterPostBackEventArgs(options, null, t)))
        if (handlers == null || handlers.length === 0) {
            return callback(options).then(processResult, r => Promise.reject(r));
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

    public applyPostbackHandlers(callback: (options: PostbackOptions) => Promise<PostbackCommitFunction | undefined>, sender: HTMLElement, handlers?: ClientFriendlyPostbackHandlerConfiguration[], args: any[] = [], context = ko.contextFor(sender), viewModel = context.$root, viewModelName?: string): Promise<DotvvmAfterPostBackEventArgs> {
        const options = new PostbackOptions(this.backUpPostBackConter(), sender, args, viewModel, viewModelName)
        const promise = this.applyPostbackHandlersCore(callback, options, this.findPostbackHandlers(context, this.globalPostbackHandlers.concat(handlers || []).concat(this.globalLaterPostbackHandlers)))
            .then(r => r(), r => Promise.reject(r))

        promise.catch(reason => { if (reason) console.log("Rejected: " + reason) });

        return promise;
    }

    public postBack(viewModelName: string, sender: HTMLElement, path: string[], command: string, controlUniqueId: string, context?: any, handlers?: ClientFriendlyPostbackHandlerConfiguration[], commandArgs?: any[]): Promise<DotvvmAfterPostBackEventArgs> {
        context = context || ko.contextFor(sender);

        const preparedHandlers = this.findPostbackHandlers(context, this.globalPostbackHandlers.concat(handlers || []).concat(this.globalLaterPostbackHandlers));
        if (preparedHandlers.filter(h => h.name && h.name.indexOf("concurrency-") == 0).length == 0) {
            // add a default concurrency handler if none is specified
            preparedHandlers.push(this.defaultConcurrencyPostbackHandler);
        }
        const options = new PostbackOptions(this.backUpPostBackConter(), sender, commandArgs, context.$data, viewModelName)
        const promise = this.applyPostbackHandlersCore(options => {
            return this.postbackCore(options, path, command, controlUniqueId, context, commandArgs)
        }, options, preparedHandlers);

        const result = promise.then(
            r => r().then(r => r, error => Promise.reject({ type: "commit", args: error })),
            r => Promise.reject(r)
        );
        result.then(
            r => r && this.events.afterPostback.trigger(r),
            (error: PostbackRejectionReason) => {
                var afterPostBackArgsCanceled = new DotvvmAfterPostBackEventArgs(options, error.type == "commit" && error.args ? error.args.serverResponseObject : null, options.postbackId);
                if (error.type == "handler" || error.type == "event") {
                    // trigger afterPostback event
                    afterPostBackArgsCanceled.wasInterrupted = true
                    this.events.postbackRejected.trigger({})
                } else if (error.type == "network") {
                    this.events.error.trigger(error.args)
                }
                this.events.afterPostback.trigger(afterPostBackArgsCanceled)
            });
        return result;
    }

    private handleRedirect(resultObject: any, viewModelName: string, replace: boolean = false): Promise<DotvvmNavigationEventArgs | void> {
        if (resultObject.replace != null) replace = resultObject.replace;
        var url = resultObject.url;

        // trigger redirect event
        var redirectArgs = new DotvvmRedirectEventArgs(dotvvm.viewModels[viewModelName], viewModelName, url, replace);
        this.events.redirect.trigger(redirectArgs);

        return this.performRedirect(url, replace, resultObject.allowSpa);
    }

    private async performRedirect(url: string, replace: boolean, allowSpa: boolean): Promise<DotvvmNavigationEventArgs | void> {
        if (replace) {
            location.replace(url);
        }

        else if (compileConstants.isSpa && allowSpa) {
            await this.handleSpaNavigationCore(url)
        }
        else {
            magicNavigator.navigate(url);
        }
    }


    public buildRouteUrl(routePath: string, params: any): string {
        // prepend url with backslash to correctly handle optional parameters at start
        routePath = '/' + routePath;

        var url = routePath.replace(/(\/[^\/]*?)\{([^\}]+?)\??(:(.+?))?\}/g, (s, prefix, paramName, _, type) => {
            if (!paramName) return "";
            const x = ko.unwrap(params[paramName.toLowerCase()])
            return x == null ? "" : prefix + x;
        });

        if (url.indexOf('/') === 0) {
            return url.substring(1);
        }
        return url;
    }

    public buildUrlSuffix(urlSuffix: string, query: any): string {
        const hashIndex = urlSuffix.indexOf("#")
        let [resultSuffix, hashSuffix] =
            hashIndex != -1 ?
                [ urlSuffix.substring(0, hashIndex), urlSuffix.substring(hashIndex) ] :
                [ urlSuffix, "" ];
        for (const property of Object.keys(query)) {
            if (!property) continue;
            var queryParamValue = ko.unwrap(query[property]);
            if (queryParamValue == null) continue;

            resultSuffix +=
                (resultSuffix.indexOf("?") != -1 ? "&" : "?")
                + `${property}=${queryParamValue}`
        }
        return resultSuffix + hashSuffix;
    }

    private isPostBackProhibited(element: HTMLElement) {
        if (element && element.tagName && ["a", "input", "button"].indexOf(element.tagName.toLowerCase()) > -1 && element.getAttribute("disabled")) {
            return true;
        }
        return false;
    }

    private addKnockoutBindingHandlers() {
        for (const h of Object.keys(bindingHandlers)) {
            ko.bindingHandlers[h] = bindingHandlers[h];
        }
    }
}
