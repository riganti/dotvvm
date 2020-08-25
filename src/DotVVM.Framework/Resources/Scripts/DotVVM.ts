/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout/knockout.dotvvm.d.ts" />
/// <reference path="typings/globalize/globalize.d.ts" />

interface Document {
    getElementByDotvvmId(id: string): HTMLElement;
}

document.getElementByDotvvmId = function (id) {
    return <HTMLElement>document.querySelector(`[data-dotvvm-id='${id}']`);
}

interface IRenderedResourceList {
    [name: string]: string;
}

interface IDotvvmPostbackScriptFunction {
    (pageArea: string, sender: HTMLElement, pathFragments: string[], controlId: string, useWindowSetTimeout: boolean, validationTarget: string, context: any, handlers: DotvvmPostBackHandlerConfiguration[]): void
}

interface IDotvvmExtensions {
}

interface IDotvvmViewModelInfo {
    viewModel?;
    viewModelCacheId?: string;
    viewModelCache?;
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

class DotVVM {
    private postBackCounter = 0;
    private lastStartedPostack = 0;
    // when we perform redirect, we also disable all new postbacks to prevent strange behavior
    private arePostbacksDisabled = false;
    private fakeRedirectAnchor: HTMLAnchorElement;
    private resourceSigns: { [name: string]: boolean } = {}
    private isViewModelUpdating: boolean = true;
    private spaHistory = new DotvvmSpaHistory();

    // warning this property is referenced in ModelState.cs and KnockoutHelper.cs
    public viewModelObservables: {
        [name: string]: KnockoutObservable<IDotvvmViewModelInfo>;
    } = {};
    public isSpaReady = ko.observable(false);
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
        execute: <T>(callback: () => Promise<T>, options: PostbackOptions) => {
            if (options.sender && dotvvm.isPostBackProhibited(options.sender)) {
                return Promise.reject({ type: "handler", handler: this, message: "PostBack is prohibited on disabled element" })
            }
            else return callback()
        }
    }

    private beforePostbackEventPostbackHandler: DotvvmPostbackHandler = {
        execute: <T>(callback: () => Promise<T>, options: PostbackOptions) => {

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
            execute: <T>(callback: () => Promise<T>, options: PostbackOptions) => {
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
                // run the next postback after everything about this one is finished (after, error events, ...)
                Promise.resolve().then(() => {
                    queue.noRunning--;
                    dotvvm.updateProgressChangeCounter(dotvvm.updateProgressChangeCounter() - 1);
                    if (queue.queue.length > 0) {
                        const callback = queue.queue.shift()!
                        callback()
                    }
                })
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
    public evaluator = new DotvvmEvaluator();
    public domUtils = new DotvvmDomUtils();
    public fileUpload = new DotvvmFileUpload();
    public validation: DotvvmValidation;
    public extensions: IDotvvmExtensions = {};

    public useHistoryApiSpaNavigation: boolean;
    public isPostbackRunning = ko.observable(false);
    public isSpaNavigationRunning = ko.observable(false);
    public updateProgressChangeCounter = ko.observable(0);

    public init(viewModelName: string, culture: string): void {
        this.addKnockoutBindingHandlers();

        // load the viewmodel
        var thisViewModel = this.viewModels[viewModelName] = JSON.parse((<HTMLInputElement>document.getElementById("__dot_viewmodel_" + viewModelName)).value);
        if (thisViewModel.resources) {
            for (var r in thisViewModel.resources) {
                this.resourceSigns[r] = true;
            }
        }
        if (thisViewModel.renderedResources) {
            thisViewModel.renderedResources.forEach(r => this.resourceSigns[r] = true);
        }
        var idFragment = thisViewModel.resultIdFragment;

        // store server-side cached viewmodel
        if (thisViewModel.viewModelCacheId) {
            thisViewModel.viewModelCache = this.viewModels[viewModelName].viewModel;
        }

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
        const spaPlaceHolder = this.getSpaPlaceHolder();
        if (spaPlaceHolder != null) {
            var hashChangeHandler = (initialLoad: boolean) => this.handleHashChange(viewModelName, spaPlaceHolder, initialLoad);

            this.useHistoryApiSpaNavigation = <boolean>JSON.parse(<string>spaPlaceHolder.getAttribute("data-dotvvm-spacontentplaceholder-usehistoryapi"));
            if (this.useHistoryApiSpaNavigation) {
                hashChangeHandler = (initialLoad: boolean) => this.handleHashChangeWithHistory(viewModelName, spaPlaceHolder, initialLoad);
            }

            var spaChangedHandler = () => hashChangeHandler(false)
            this.domUtils.attachEvent(window, "hashchange", spaChangedHandler);
            hashChangeHandler(true);
        }

        window.addEventListener('popstate', (event) => this.handlePopState(viewModelName, event, spaPlaceHolder != null));

        this.isViewModelUpdating = false;

        if (idFragment) {
            if (spaPlaceHolder) {
                var element = document.getElementById(idFragment);
                if (element && "function" == typeof element.scrollIntoView) element.scrollIntoView(true);
            }
            else location.hash = idFragment;
        }

        // persist the viewmodel in the hidden field so the Back button will work correctly
        this.domUtils.attachEvent(window, "beforeunload", e => {
            this.persistViewModel(viewModelName);
        });
    }

    private handlePopState(viewModelName: string, event: PopStateEvent, inSpaPage: boolean) {
        if (this.spaHistory.isSpaPage(event.state)) {
            var historyRecord = this.spaHistory.getHistoryRecord(event.state);
            if (inSpaPage)
                this.navigateCore(viewModelName, historyRecord.url);
            else
                this.performRedirect(historyRecord.url, true);

            event.preventDefault();
        }
    }

    private handleHashChangeWithHistory(viewModelName: string, spaPlaceHolder: HTMLElement, isInitialPageLoad: boolean) {
        if (document.location.hash.indexOf("#!/") === 0) {
            // the user requested navigation to another SPA page
            this.navigateCore(viewModelName, document.location.hash.substring(2),
                (url) => { this.spaHistory.replacePage(url); });

        } else {
            var defaultUrl = spaPlaceHolder.getAttribute("data-dotvvm-spacontentplaceholder-defaultroute");
            var containsContent = spaPlaceHolder.hasAttribute("data-dotvvm-spacontentplaceholder-content");

            if (!containsContent && defaultUrl) {
                this.navigateCore(viewModelName, "/" + defaultUrl, (url) => this.spaHistory.replacePage(url));
            } else {
                this.isSpaReady(true);
                spaPlaceHolder.style.display = "";

                var currentRelativeUrl = location.pathname + location.search + location.hash
                this.spaHistory.replacePage(currentRelativeUrl);
            }
        }
    }

    private handleHashChange(viewModelName: string, spaPlaceHolder: HTMLElement, isInitialPageLoad: boolean) {
        if (document.location.hash.indexOf("#!/") === 0) {
            // the user requested navigation to another SPA page
            this.navigateCore(viewModelName, document.location.hash.substring(2));

        } else {
            var url = spaPlaceHolder.getAttribute("data-dotvvm-spacontentplaceholder-defaultroute");

            if (url) {
                // perform redirect to default page
                url = "#!/" + url;
                url = this.fixSpaUrlPrefix(url);
                this.performRedirect(url, isInitialPageLoad);

            } else if (!isInitialPageLoad) {
                // get startup URL and redirect there
                url = document.location.toString();
                var slashIndex = url.indexOf('/', 'https://'.length);
                if (slashIndex > 0) {
                    url = url.substring(slashIndex);
                } else {
                    url = "/";
                }
                this.navigateCore(viewModelName, url);

            } else {
                // the page was loaded for the first time
                this.isSpaReady(true);
                spaPlaceHolder.style.display = "";
            }
        }
    }

    private persistViewModel(viewModelName: string) {
        var viewModel = this.viewModels[viewModelName];
        var persistedViewModel = {};
        for (var p in viewModel) {
            if (viewModel.hasOwnProperty(p)) {
                persistedViewModel[p] = viewModel[p];
            }
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

    private async fetchCsrfToken(viewModelName: string): Promise<string> {
        const vm = this.viewModels[viewModelName].viewModel
        if (vm.$csrfToken == null) {
            const response = await fetch((this.viewModels[viewModelName].virtualDirectory || "") + "/___dotvvm-create-csrf-token___")
            if (response.status != 200)
                throw new Error(`Can't fetch CSRF token: ${response.statusText}`)
            vm.$csrfToken = await response.text()
        }
        return vm.$csrfToken
    }

    public staticCommandPostback(viewModelName: string, sender: HTMLElement, command: string, args: any[], callback = _ => { }, errorCallback = (errorInfo: { xhr?: XMLHttpRequest, error?: any }) => { }) {
        (async () => {
            var csrfToken;
            try {
                csrfToken = await this.fetchCsrfToken(viewModelName);
            }
            catch (err) {
                this.events.error.trigger(new DotvvmErrorEventArgs(sender, this.viewModels[viewModelName].viewModel, viewModelName, null, null));
                console.warn(`CSRF token fetch failed.`);
                errorCallback({ error: err });
                return;
            }

            var data = this.serialization.serialize({
                args,
                command,
                "$csrfToken": csrfToken
            });
            dotvvm.events.staticCommandMethodInvoking.trigger(data);

            this.postJSON(<string>this.viewModels[viewModelName].url, "POST", ko.toJSON(data), response => {
                try {
                    this.isViewModelUpdating = true;
                    const responseObj = <DotvvmStaticCommandResponse>JSON.parse(response.responseText);
                    if ("action" in responseObj) {
                        if (responseObj.action == "redirect") {
                            // redirect
                            this.handleRedirect(responseObj, viewModelName);
                            errorCallback({ xhr: response, error: "redirect" });
                            return;
                        } else {
                            throw new Error(`Invalid action ${responseObj.action}`);
                        }
                    }
                    const result = responseObj.result;
                    dotvvm.events.staticCommandMethodInvoked.trigger({ ...data, result, xhr: response });
                    callback(result);
                } catch (error) {
                    dotvvm.events.staticCommandMethodFailed.trigger({ ...data, xhr: response, error: error })
                    errorCallback({ xhr: response, error });
                } finally {
                    this.isViewModelUpdating = false;
                }
            }, (xhr) => {
                if (/^application\/json(;|$)/.test(xhr.getResponseHeader("Content-Type")!)) {
                    const errObject = JSON.parse(xhr.responseText)

                    if (errObject.action === "invalidCsrfToken") {
                        // ok, renew the token and try again. Do that before any event is triggered
                        this.viewModels[viewModelName].viewModel.$csrfToken = null
                        console.log("Resending postback due to invalid CSRF token.") // this may loop indefinitely (in some extreme case), we don't currently have any loop detection mechanism, so at least we can log it.
                        this.staticCommandPostback(viewModelName, sender, command, args, callback, errorCallback)
                        return;
                    }
                }
                this.events.error.trigger(new DotvvmErrorEventArgs(sender, this.viewModels[viewModelName].viewModel, viewModelName, xhr, null));
                console.warn(`StaticCommand postback failed: ${xhr.status} - ${xhr.statusText}`, xhr);
                errorCallback({ xhr });
                dotvvm.events.staticCommandMethodFailed.trigger({ ...data, xhr })
            },
                xhr => {
                    xhr.setRequestHeader("X-PostbackType", "StaticCommand");
                });
        })()
    }

    private processPassedId(id: any, context: any): string {
        if (typeof id == "string" || id == null) return id;
        if (typeof id == "object" && id.expr) return this.evaluator.evaluateOnViewModel(context, id.expr);
        throw new Error("invalid argument");
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
        if (dotvvm.isSpaNavigationRunning()) return Promise.reject({ type: "handler" });

        const options = new PostbackOptions(this.backUpPostBackConter(), sender, args, viewModel, viewModelName)
        const promise = this.applyPostbackHandlersCore(callback, options, this.findPostbackHandlers(context, this.globalPostbackHandlers.concat(handlers || []).concat(this.globalLaterPostbackHandlers)))
            .then(r => r(), r => Promise.reject(r))

        promise.catch(reason => { if (reason) console.log("Rejected: " + reason) });

        return promise;
    }

    public postbackCore(options: PostbackOptions, path: string[], command: string, controlUniqueId: string, context: any, commandArgs?: any[]) {
        return new Promise<() => Promise<DotvvmAfterPostBackEventArgs>>(async (resolve, reject) => {
            const viewModelName = options.viewModelName!;
            const viewModel = this.viewModels[viewModelName].viewModel;

            try {
                await this.fetchCsrfToken(viewModelName);
            }
            catch (err) {
                reject({ type: 'network', options: options, args: new DotvvmErrorEventArgs(options.sender, viewModel, viewModelName, null, options.postbackId) });
                return;
            }

            if (this.arePostbacksDisabled) {
                reject({ type: 'handler' })
            }

            this.lastStartedPostack = options.postbackId
            // perform the postback
            this.updateDynamicPathFragments(context, path);
            const data: any = {
                currentPath: path,
                command: command,
                controlUniqueId: this.processPassedId(controlUniqueId, context),
                additionalData: options.additionalPostbackData,
                renderedResources: this.viewModels[viewModelName].renderedResources,
                commandArgs: commandArgs
            };

            const completeViewModel = this.serialization.serialize(viewModel, { pathMatcher(val) { return context && val == context.$data } });

            // if the viewmodel is cached on the server, send only the diff
            if (this.viewModels[viewModelName].viewModelCache) {
                data.viewModelDiff = this.diff(this.viewModels[viewModelName].viewModelCache, completeViewModel);
                data.viewModelCacheId = this.viewModels[viewModelName].viewModelCacheId;
            } else {
                data.viewModel = completeViewModel;    
            }

            const errorAction = xhr => {
                if (/^application\/json(;|$)/.test(xhr.getResponseHeader("Content-Type")!)) {
                    const errObject = JSON.parse(xhr.responseText)

                    if (errObject.action === "invalidCsrfToken") {
                        // ok, renew the token and try again. Do that before any event is triggered
                        this.viewModels[viewModelName].viewModel.$csrfToken = null
                        console.log("Resending postback due to invalid CSRF token.") // this may loop indefinitely (in some extreme case), we don't currently have any loop detection mechanism, so at least we can log it.
                        this.postbackCore(options, path, command, controlUniqueId, context, commandArgs).then(resolve, reject)
                        return;
                    }
                }
                reject({ type: 'network', options: options, args: new DotvvmErrorEventArgs(options.sender, viewModel, viewModelName, xhr, options.postbackId) });
            };

            this.postJSON(<string>this.viewModels[viewModelName].url, "POST", ko.toJSON(data), result => {
                var resultObject: any = {};

                const successAction = actualResult => {
                    dotvvm.events.postbackResponseReceived.trigger({})
                    resolve(() => new Promise((resolve, reject) => {
                        dotvvm.events.postbackCommitInvoked.trigger({})
                        
                        if (!resultObject.viewModel && resultObject.viewModelDiff) {
                            // TODO: patch (~deserialize) it to ko.observable viewModel
                            resultObject.viewModel = this.patch(completeViewModel, resultObject.viewModelDiff);
                        }

                        this.loadResourceList(resultObject.resources, () => {
                            var isSuccess = false;
                            if (resultObject.action === "successfulCommand") {
                                try {
                                    this.isViewModelUpdating = true;

                                    // store server-side cached viewmodel
                                    if (resultObject.viewModelCacheId) {
                                        this.viewModels[viewModelName].viewModelCacheId = resultObject.viewModelCacheId;
                                        this.viewModels[viewModelName].viewModelCache = resultObject.viewModel;
                                    } else {
                                        delete this.viewModels[viewModelName].viewModelCacheId;
                                        delete this.viewModels[viewModelName].viewModelCache;
                                    }

                                    // remove updated controls
                                    var updatedControls = this.cleanUpdatedControls(resultObject);

                                    // update the viewmodel
                                    if (resultObject.viewModel) {
                                        ko.delaySync.pause();
                                        this.serialization.deserialize(resultObject.viewModel, this.viewModels[viewModelName].viewModel);
                                        ko.delaySync.resume();
                                    }
                                    isSuccess = true;

                                    // remove updated controls which were previously hidden
                                    this.cleanUpdatedControls(resultObject, updatedControls);

                                    // add updated controls
                                    this.restoreUpdatedControls(resultObject, updatedControls, true);
                                }
                                finally {
                                    this.isViewModelUpdating = false;
                                }
                                dotvvm.events.postbackViewModelUpdated.trigger({})
                            } else if (resultObject.action === "redirect") {
                                // redirect
                                var promise = this.handleRedirect(resultObject, viewModelName)
                                var redirectAfterPostBackArgs = new DotvvmAfterPostBackWithRedirectEventArgs(options, resultObject, resultObject.commandResult, result, promise);
                                resolve(redirectAfterPostBackArgs);
                                return;
                            }

                            var idFragment = resultObject.resultIdFragment;
                            if (idFragment) {
                                if (this.getSpaPlaceHolder() || location.hash == "#" + idFragment) {
                                    var element = document.getElementById(idFragment);
                                    if (element && "function" == typeof element.scrollIntoView) element.scrollIntoView(true);
                                }
                                else location.hash = idFragment;
                            }

                            // trigger afterPostback event
                            if (!isSuccess) {
                                reject(new DotvvmErrorEventArgs(options.sender, viewModel, viewModelName, actualResult, options.postbackId, resultObject))
                            } else {
                                var afterPostBackArgs = new DotvvmAfterPostBackEventArgs(options, resultObject, resultObject.commandResult, result)
                                resolve(afterPostBackArgs)
                            }
                        });
                    }));
                }

                const parseResultObject = actualResult => {
                    const locationHeader = actualResult.getResponseHeader("Location");
                    resultObject = locationHeader != null && locationHeader.length > 0 ?
                        { action: "redirect", url: locationHeader } :
                        JSON.parse(actualResult.responseText);
                }
                parseResultObject(result);

                if (resultObject.action === "viewModelNotCached") {
                    // repeat request with full viewModel
                    delete this.viewModels[viewModelName].viewModelCache;
                    delete this.viewModels[viewModelName].viewModelCacheId;
                    delete data.viewModelDiff;
                    delete data.viewModelCacheId;
                    data.viewModel = completeViewModel;

                    return this.postJSON(<string>this.viewModels[viewModelName].url, "POST", ko.toJSON(data), result2 => {
                        parseResultObject(result2);
                        successAction(result2);
                    }, errorAction);
                }
                else {
                    // process the response
                    successAction(result);
                }                
            }, errorAction);
        });
    }

    public handleSpaNavigation(element: HTMLElement): Promise<DotvvmNavigationEventArgs> {
        var target = element.getAttribute('target');
        if (target == "_blank") {
            return Promise.resolve(new DotvvmNavigationEventArgs(this.viewModels.root.viewModel, "root", null));
        }
        return this.handleSpaNavigationCore(element.getAttribute('href'));
    }

    public handleSpaNavigationCore(url: string | null): Promise<DotvvmNavigationEventArgs> {
        return new Promise<DotvvmNavigationEventArgs>((resolve, reject) => {
            if (url && url.indexOf("/") === 0) {
                var viewModelName = "root"
                url = this.removeVirtualDirectoryFromUrl(url, viewModelName);
                this.navigateCore(viewModelName, url, (navigatedUrl) => {
                    if (!history.state || history.state.url != navigatedUrl) {
                        this.spaHistory.pushPage(navigatedUrl);
                    }
                }).then(resolve, reject);
            } else {
                reject();
            }
        });
    }

    public postBack(viewModelName: string, sender: HTMLElement, path: string[], command: string, controlUniqueId: string, context?: any, handlers?: ClientFriendlyPostbackHandlerConfiguration[], commandArgs?: any[]): Promise<DotvvmAfterPostBackEventArgs> {
        if (dotvvm.isSpaNavigationRunning()) return Promise.reject({ type: "handler" });

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

    private loadResourceList(resources: IRenderedResourceList, callback: () => void) {
        var html = "";
        for (var name in resources) {
            if (!/^__noname_\d+$/.test(name)) {
                if (this.resourceSigns[name]) continue;
                this.resourceSigns[name] = true;
            }
            html += resources[name] + " ";
        }
        if (html.trim() === "") {
            setTimeout(callback, 4);
            return;
        }
        else {
            var tmp = document.createElement("div");
            tmp.innerHTML = html;
            var elements: HTMLElement[] = [];
            for (var i = 0; i < tmp.children.length; i++) {
                elements.push(<HTMLElement>tmp.children.item(i));
            }
            this.loadResourceElements(elements, 0, callback);
        }
    }

    private loadResourceElements(elements: HTMLElement[], offset: number, callback: () => void) {
        if (offset >= elements.length) {
            callback();
            return;
        }
        var element = elements[offset];
        var waitForScriptLoaded = false;
        if (element.tagName.toLowerCase() == "script") {
            var originalScript = <HTMLScriptElement>element;
            var script = <HTMLScriptElement>document.createElement("script");
            if (originalScript.src) {
                script.src = originalScript.src;
                waitForScriptLoaded = true;
            }
            if (originalScript.type) {
                script.type = originalScript.type;
            }
            if (originalScript.text) {
                script.text = originalScript.text;
            }
            if (element.id) {
                script.id = element.id;
            }
            element = script;
        }
        else if (element.tagName.toLowerCase() == "link") {
            // create link
            var originalLink = <HTMLLinkElement>element;
            var link = <HTMLLinkElement>document.createElement("link");
            if (originalLink.href) {
                link.href = originalLink.href;
            }
            if (originalLink.rel) {
                link.rel = originalLink.rel;
            }
            if (originalLink.type) {
                link.type = originalLink.type;
            }
            element = link;
        }

        // load next script when this is finished
        if (waitForScriptLoaded) {
            element.addEventListener("load", () => this.loadResourceElements(elements, offset + 1, callback));
            element.addEventListener("error", () => this.loadResourceElements(elements, offset + 1, callback));
        }
        document.head.appendChild(element);
        if (!waitForScriptLoaded) {
            this.loadResourceElements(elements, offset + 1, callback);
        }
    }

    private getSpaPlaceHolder() {
        var elements = document.getElementsByName("__dot_SpaContentPlaceHolder");
        if (elements.length == 1) {
            return <HTMLElement>elements[0];
        }
        return null;
    }

    private navigateCore(viewModelName: string, url: string, handlePageNavigating?: (url: string) => void): Promise<DotvvmNavigationEventArgs> {
        return new Promise((resolve, reject: (reason?: DotvvmErrorEventArgs) => void) => {

            var viewModel = this.viewModels[viewModelName].viewModel;

            // prevent double postbacks
            var currentPostBackCounter = this.backUpPostBackConter();
            this.isSpaNavigationRunning(true);

            // trigger spaNavigating event
            var spaNavigatingArgs = new DotvvmSpaNavigatingEventArgs(viewModel, viewModelName, url);
            this.events.spaNavigating.trigger(spaNavigatingArgs);
            if (spaNavigatingArgs.cancel) {
                return;
            }

            var virtualDirectory = this.viewModels[viewModelName].virtualDirectory || "";

            // add virtual directory prefix
            var spaUrl = "/___dotvvm-spa___" + this.addLeadingSlash(url);
            var fullUrl = this.addLeadingSlash(this.concatUrl(virtualDirectory, spaUrl));

            // find SPA placeholder
            var spaPlaceHolder = this.getSpaPlaceHolder();
            if (!spaPlaceHolder) {
                document.location.href = fullUrl;
                return;
            }

            if (handlePageNavigating) {
                handlePageNavigating(
                    this.addLeadingSlash(this.concatUrl(virtualDirectory, this.addLeadingSlash(url))));
            }

            // send the request
            var spaPlaceHolderUniqueId = spaPlaceHolder.attributes["data-dotvvm-spacontentplaceholder"].value;
            this.getJSON(fullUrl, "GET", spaPlaceHolderUniqueId, result => {
                // if another postback has already been passed, don't do anything
                if (!this.isPostBackStillActive(currentPostBackCounter)) return;

                var resultObject = JSON.parse(result.responseText);
                this.loadResourceList(resultObject.resources, () => {
                    var isSuccess = false;
                    if (resultObject.action === "successfulCommand" || !resultObject.action) {
                        try {
                            this.isViewModelUpdating = true;

                            // remove updated controls
                            var updatedControls = this.cleanUpdatedControls(resultObject);

                            // update the viewmodel
                            this.viewModels[viewModelName] = {};
                            for (var p in resultObject) {
                                if (resultObject.hasOwnProperty(p)) {
                                    this.viewModels[viewModelName][p] = resultObject[p];
                                }
                            }

                            // store server-side cached viewmodel
                            if (resultObject.viewModelCacheId) {
                                this.viewModels[viewModelName].viewModelCache = resultObject.viewModel;
                            } else {
                                delete this.viewModels[viewModelName].viewModelCache;
                            }

                            ko.delaySync.pause();
                            this.serialization.deserialize(resultObject.viewModel, this.viewModels[viewModelName].viewModel);
                            ko.delaySync.resume();
                            isSuccess = true;

                            // add updated controls
                            this.viewModelObservables[viewModelName](this.viewModels[viewModelName].viewModel);
                            this.restoreUpdatedControls(resultObject, updatedControls, true);

                            this.isSpaReady(true);
                        }
                        finally {
                            this.isViewModelUpdating = false;
                        }
                    } else if (resultObject.action === "redirect") {
                        this.isSpaNavigationRunning(false);
                        this.handleRedirect(resultObject, viewModelName, true).then(resolve, reject);
                        return;
                    }
                    
                    // trigger spaNavigated event
                    var spaNavigatedArgs = new DotvvmSpaNavigatedEventArgs(this.viewModels[viewModelName].viewModel, viewModelName, resultObject, result);
                    this.events.spaNavigated.trigger(spaNavigatedArgs);

                    this.isSpaNavigationRunning(false);

                    if (!isSuccess && !spaNavigatedArgs.isHandled) {
                        reject();
                        throw "Invalid response from server!";
                    }

                    resolve(spaNavigatedArgs);
                });
            }, xhr => {
                // if another postback has already been passed, don't do anything
                if (!this.isPostBackStillActive(currentPostBackCounter)) return;

                // execute error handlers
                var errArgs = new DotvvmErrorEventArgs(undefined, viewModel, viewModelName, xhr, -1, undefined, true);
                this.events.error.trigger(errArgs);
                if (!errArgs.handled) {
                    alert(xhr.responseText);
                }

                this.isSpaNavigationRunning(false);
                reject(errArgs);
            });
        });
    }

    private handleRedirect(resultObject: any, viewModelName: string, replace: boolean = false): Promise<DotvvmNavigationEventArgs> {
        if (resultObject.replace != null) replace = resultObject.replace;
        var url;
        // redirect
        if (this.getSpaPlaceHolder() && !this.useHistoryApiSpaNavigation && resultObject.url.indexOf("//") < 0 && resultObject.allowSpa) {
            // relative URL - keep in SPA mode, but remove the virtual directory
            url = "#!" + this.removeVirtualDirectoryFromUrl(resultObject.url, viewModelName);
            if (url === "#!") {
                url = "#!/";
            }

            // verify that the URL prefix is correct, if not - add it before the fragment
            url = this.fixSpaUrlPrefix(url);

        } else {
            // absolute URL - load the URL
            url = resultObject.url;
        }

        // trigger redirect event
        var redirectArgs = new DotvvmRedirectEventArgs(dotvvm.viewModels[viewModelName], viewModelName, url, replace);
        this.events.redirect.trigger(redirectArgs);

        return this.performRedirect(url, replace, resultObject.allowSpa && this.useHistoryApiSpaNavigation);
    }

    private disablePostbacks() {
        this.lastStartedPostack = -1 // this stops further commits
        for (const q in this.postbackQueues) {
            if (this.postbackQueues.hasOwnProperty(q)) {
                let postbackQueue = this.postbackQueues[q];
                postbackQueue.queue.length = 0;
                postbackQueue.noRunning = 0;
            }
        }
        // disable all other postbacks
        // but not in SPA mode, since we'll need them for the next page
        // and user might want to try another postback in case this navigation hangs
        if (!this.getSpaPlaceHolder()) {
            this.arePostbacksDisabled = true
        }
    }

    private performRedirect(url: string, replace: boolean, useHistoryApiSpaRedirect?: boolean): Promise<DotvvmNavigationEventArgs> {
        return new Promise((resolve, reject) => {
            this.disablePostbacks()
            if (replace) {
                location.replace(url);
                resolve();
            }
            else if (useHistoryApiSpaRedirect) {
                this.handleSpaNavigationCore(url).then(resolve, reject);
            }
            else {
                var fakeAnchor = this.fakeRedirectAnchor;
                if (!fakeAnchor) {
                    fakeAnchor = <HTMLAnchorElement>document.createElement("a");
                    fakeAnchor.style.display = "none";
                    fakeAnchor.setAttribute("data-dotvvm-fake-id", "dotvvm_fake_redirect_anchor_87D7145D_8EA8_47BA_9941_82B75EE88CDB");
                    document.body.appendChild(fakeAnchor);
                    this.fakeRedirectAnchor = fakeAnchor;
                }
                fakeAnchor.href = url;
                fakeAnchor.click();
                resolve();
            }
        });
    }

    private fixSpaUrlPrefix(url: string): string {
        var attr = this.getSpaPlaceHolder()!.attributes["data-dotvvm-spacontentplaceholder-urlprefix"];
        if (!attr) {
            return url;
        }

        var correctPrefix = attr.value;
        var currentPrefix = document.location.pathname;
        if (correctPrefix !== currentPrefix) {
            if (correctPrefix === "") {
                correctPrefix = "/";
            }
            url = correctPrefix + url;
        }
        return url;
    }

    private removeVirtualDirectoryFromUrl(url: string, viewModelName: string) {
        var virtualDirectory = "/" + this.viewModels[viewModelName].virtualDirectory;
        if (url.indexOf(virtualDirectory) == 0) {
            return this.addLeadingSlash(url.substring(virtualDirectory.length));
        } else {
            return url;
        }
    }

    private addLeadingSlash(url: string) {
        if (url.length > 0 && url.substring(0, 1) != "/") {
            return "/" + url;
        }
        return url;
    }

    private concatUrl(url1: string, url2: string) {
        if (url1.length > 0 && url1.substring(url1.length - 1) == "/") {
            url1 = url1.substring(0, url1.length - 1);
        }
        return url1 + this.addLeadingSlash(url2);
    }

    public patch(source: any, patch: any): any {
        if (source instanceof Array && patch instanceof Array) {
            return patch.map((val, i) =>
                this.patch(source[i], val));
        }
        else if (source instanceof Array || patch instanceof Array)
            return patch;
        else if (typeof source == "object" && typeof patch == "object" && source && patch) {
            for (var p in patch) {
                if (patch[p] == null) source[p] = null;
                else if (source[p] == null) source[p] = patch[p];
                else source[p] = this.patch(source[p], patch[p]);
            }
        }
        else return patch;

        return source;
    }

    public diff(source: any, modified: any): any {
        if (source instanceof Array && modified instanceof Array) {
            var diffArray = modified.map((el, index) => this.diff(source[index], el));
            if (source.length === modified.length 
                && diffArray.every((el, index) => el === this.diffEqual || source[index] === modified[index])) {
                return this.diffEqual;
            } else {
                return diffArray;
            }
        }
        else if (source instanceof Array || modified instanceof Array) {
            return modified;
        }
        else if (typeof source == "object" && typeof modified == "object" && source && modified) {
            var result = this.diffEqual;
            for (var p in modified) {
                var propertyDiff = this.diff(source[p], modified[p]);
                if (propertyDiff !== this.diffEqual && source[p] !== modified[p]) {
                    if (result === this.diffEqual) {
                        result = {};
                    }
                    result[p] = propertyDiff;
                } else if (p[0] === "$") {
                    if (result == this.diffEqual) {
                        result = {};
                    }
                    result[p] = modified[p];
                }
            }
            return result;
        }
        else if (source === modified) {
            if (typeof source == "object") {
                return this.diffEqual;
            } else {
                return source; 
            }
        } else {
            return modified;
        }
    }
    public readonly diffEqual = {};

    private updateDynamicPathFragments(context: any, path: string[]): void {
        for (var i = path.length - 1; i >= 0; i--) {
            if (path[i].indexOf("[$index]") >= 0) {
                path[i] = path[i].replace("[$index]", `[${context.$index()}]`);
            }

            if (path[i].indexOf("[$indexPath]") >= 0) {
                path[i] = path[i].replace("[$indexPath]", `[${context.$indexPath.map(i => i()).join("]/[")}]`);
            }

            context = context.$parentContext;
        }
    }

    private postJSON(url: string, method: string, postData: any, success: (request: XMLHttpRequest) => void, error: (response: XMLHttpRequest) => void, preprocessRequest = (xhr: XMLHttpRequest) => { }) {
        var xhr = this.getXHR();
        xhr.open(method, url, true);
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.setRequestHeader("X-DotVVM-PostBack", "true");
        xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
        preprocessRequest(xhr);
        xhr.onreadystatechange = () => {
            if (xhr.readyState !== XMLHttpRequest.DONE) return;
            if (xhr.status && xhr.status < 400) {
                success(xhr);
            } else {
                error(xhr);
            }
        };
        xhr.send(postData);
    }

    private getJSON(url: string, method: string, spaPlaceHolderUniqueId: string, success: (request: XMLHttpRequest) => void, error: (response: XMLHttpRequest) => void) {
        var xhr = this.getXHR();
        xhr.open(method, url, true);
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.setRequestHeader("X-DotVVM-SpaContentPlaceHolder", spaPlaceHolderUniqueId);
        xhr.onreadystatechange = () => {
            if (xhr.readyState !== XMLHttpRequest.DONE) return;
            if (xhr.status && xhr.status < 400) {
                success(xhr);
            } else {
                error(xhr);
            }
        };
        xhr.send();
    }

    public getXHR(): XMLHttpRequest {
        return XMLHttpRequest ? new XMLHttpRequest() : <XMLHttpRequest>new (window["ActiveXObject"])("Microsoft.XMLHTTP");
    }

    private cleanUpdatedControls(resultObject: any, updatedControls: any = {}) {
        for (var id in resultObject.updatedControls) {
            if (resultObject.updatedControls.hasOwnProperty(id)) {
                var control = document.getElementByDotvvmId(id);
                if (control) {
                    var dataContext = ko.contextFor(control);
                    var nextSibling = control.nextSibling;
                    var parent = control.parentNode;
                    ko.removeNode(control);
                    updatedControls[id] = { control: control, nextSibling: nextSibling, parent: parent, dataContext: dataContext };
                }
            }
        }
        return updatedControls;
    }

    private restoreUpdatedControls(resultObject: any, updatedControls: any, applyBindingsOnEachControl: boolean) {
        for (var id in resultObject.updatedControls) {
            if (resultObject.updatedControls.hasOwnProperty(id)) {
                var updatedControl = updatedControls[id];
                if (updatedControl) {
                    var wrapper = document.createElement(updatedControls[id].parent.tagName || "div");
                    wrapper.innerHTML = resultObject.updatedControls[id];
                    if (wrapper.childElementCount > 1) throw new Error("Postback.Update control can not render more than one element");
                    var element = wrapper.firstElementChild;
                    if (element.id == null) throw new Error("Postback.Update control always has to render id attribute.");
                    if (element.id !== updatedControls[id].control.id) console.log(`Postback.Update control changed id from '${updatedControls[id].control.id}' to '${element.id}'`);
                    wrapper.removeChild(element);
                    if (updatedControl.nextSibling) {
                        updatedControl.parent.insertBefore(element, updatedControl.nextSibling);
                    } else {
                        updatedControl.parent.appendChild(element);
                    }
                }
            }
        }

        if (applyBindingsOnEachControl) {
            window.setTimeout(() => {
                try {
                    this.isViewModelUpdating = true;
                    for (var id in resultObject.updatedControls) {
                        var updatedControl = document.getElementByDotvvmId(id);
                        if (updatedControl) {
                            ko.applyBindings(updatedControls[id].dataContext, updatedControl);
                        }
                    }
                }
                finally {
                    this.isViewModelUpdating = false;
                }
            }, 0);
        }
    }

    public unwrapArrayExtension(array: any): any {
        return ko.unwrap(ko.unwrap(array));
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
        var resultSuffix, hashSuffix;
        if (urlSuffix.indexOf("#") !== -1) {
            resultSuffix = urlSuffix.substring(0, urlSuffix.indexOf("#"));
            hashSuffix = urlSuffix.substring(urlSuffix.indexOf("#"));
        } else {
            resultSuffix = urlSuffix;
            hashSuffix = "";
        }
        for (var property in query) {
            if (query.hasOwnProperty(property)) {
                if (!property) continue;
                var queryParamValue = ko.unwrap(query[property]);
                if (queryParamValue == null) continue;
                resultSuffix = resultSuffix.concat(resultSuffix.indexOf("?") !== -1
                    ? `&${property}=${queryParamValue}`
                    : `?${property}=${queryParamValue}`);
            }
        }
        return resultSuffix.concat(hashSuffix);
    }

    private isPostBackProhibited(element: HTMLElement) {
        if (element && element.tagName && ["a", "input", "button"].indexOf(element.tagName.toLowerCase()) > -1 && element.getAttribute("disabled")) {
            return true;
        }
        return false;
    }

    private addKnockoutBindingHandlers() {
        function createWrapperComputed(accessor: () => any, propertyDebugInfo: string | null = null) {
            var computed = ko.pureComputed({
                read() {
                    var property = accessor();
                    var propertyValue = ko.unwrap(property); // has to call that always as it is a dependency
                    return propertyValue;
                },
                write(value) {
                    var val = accessor();
                    if (ko.isObservable(val)) {
                        val(value);
                    }
                    else {
                        console.warn(`Attempted to write to readonly property` + (propertyDebugInfo == null ? `` : ` ` + propertyDebugInfo) + `.`);
                    }
                }
            });
            computed["wrappedProperty"] = accessor;
            return computed;
        }

        ko.virtualElements.allowedBindings["dotvvm_withControlProperties"] = true;
        ko.bindingHandlers["dotvvm_withControlProperties"] = {
            init: (element, valueAccessor, allBindings, viewModel, bindingContext) => {
                if (!bindingContext) throw new Error();

                var value = valueAccessor();
                for (var prop in value) {

                    value[prop] = createWrapperComputed(
                        function () {
                            var property = valueAccessor()[this.prop];
                            return !ko.isObservable(property) ? dotvvm.serialization.deserialize(property) : property
                        }.bind({ prop: prop }),
                        `'${prop}' at '${valueAccessor.toString()}'`);
                }
                var innerBindingContext = bindingContext.extend({ $control: value });
                element.innerBindingContext = innerBindingContext;
                ko.applyBindingsToDescendants(innerBindingContext, element);
                return { controlsDescendantBindings: true }; // do not apply binding again
            },
            update(element, valueAccessor, allBindings, viewModel, bindingContext) {
            }
        };

        ko.virtualElements.allowedBindings["dotvvm_introduceAlias"] = true;
        ko.bindingHandlers["dotvvm_introduceAlias"] = {
            init(element, valueAccessor, allBindings, viewModel, bindingContext) {
                if (!bindingContext) throw new Error();
                var value = valueAccessor();
                var extendBy = {};
                for (var prop in value) {
                    var propPath = prop.split('.');
                    var obj = extendBy;
                    for (var i = 0; i < propPath.length - 1; i) {
                        obj = extendBy[propPath[i]] || (extendBy[propPath[i]] = {});
                    }
                    obj[propPath[propPath.length - 1]] = createWrapperComputed(function () { return valueAccessor()[this.prop] }.bind({ prop: prop }), `'${prop}' at '${valueAccessor.toString()}'`);
                }
                var innerBindingContext = bindingContext.extend(extendBy);
                element.innerBindingContext = innerBindingContext;
                ko.applyBindingsToDescendants(innerBindingContext, element);
                return { controlsDescendantBindings: true }; // do not apply binding again
            }
        }

        const makeUpdatableChildrenContextHandler = (
            makeContextCallback: (bindingContext: KnockoutBindingContext, value: any, _allBindings: KnockoutAllBindingsAccessor) => any,
            shouldDisplay: (value: any) => boolean
        ) => (element: Node, valueAccessor, _allBindings: KnockoutAllBindingsAccessor, _viewModel, bindingContext: KnockoutBindingContext) => {
            if (!bindingContext) throw new Error()

            var savedNodes: Node[] | undefined;
            var isInitial = true;
            ko.computed(function () {
                var rawValue = valueAccessor();
                ko.unwrap(rawValue); // we have to touch the observable in the binding so that the `getDependenciesCount` call knows about this dependency. If would be unwrapped only later (in the makeContextCallback) we would not have the savedNodes.

                // Save a copy of the inner nodes on the initial update, but only if we have dependencies.
                if (isInitial && ko.computedContext.getDependenciesCount()) {
                    savedNodes = ko.utils.cloneNodes(ko.virtualElements.childNodes(element), true /* shouldCleanNodes */);
                }

                if (shouldDisplay(rawValue)) {
                    if (!isInitial) {
                        ko.virtualElements.setDomNodeChildren(element, ko.utils.cloneNodes(savedNodes!));
                    }
                    ko.applyBindingsToDescendants(makeContextCallback(bindingContext, rawValue, _allBindings), element);
                } else {
                    ko.virtualElements.emptyNode(element);
                }

                isInitial = false;

            }, null, { disposeWhenNodeIsRemoved: element });
            return { controlsDescendantBindings: true } // do not apply binding again
        }

        const foreachCollectionSymbol = "$foreachCollectionSymbol"
        ko.virtualElements.allowedBindings["dotvvm-SSR-foreach"] = true
        ko.bindingHandlers["dotvvm-SSR-foreach"] = {
            init: makeUpdatableChildrenContextHandler(
                (bindingContext, rawValue) => bindingContext.extend({ [foreachCollectionSymbol]: rawValue.data }),
                v => v.data != null)
        }
        ko.virtualElements.allowedBindings["dotvvm-SSR-item"] = true
        ko.bindingHandlers["dotvvm-SSR-item"] = {
            init(element, valueAccessor, _allBindings, _viewModel, bindingContext) {
                if (!bindingContext) throw new Error()
                var collection = bindingContext[foreachCollectionSymbol]
                var innerBindingContext = bindingContext.createChildContext(() => {
                    return ko.unwrap((ko.unwrap(collection) || [])[valueAccessor()]);
                }).extend({ $index: ko.pureComputed(valueAccessor) });
                element.innerBindingContext = innerBindingContext
                ko.applyBindingsToDescendants(innerBindingContext, element)
                return { controlsDescendantBindings: true } // do not apply binding again
            },
            update(element) {
                if (element.seenUpdate)
                    console.error(`dotvvm-SSR-item binding did not expect to see a update`);
                element.seenUpdate = 1;
            }
        }
        ko.virtualElements.allowedBindings["withGridViewDataSet"] = true;
        ko.bindingHandlers["withGridViewDataSet"] = {
            init: makeUpdatableChildrenContextHandler(
                (bindingContext, value, allBindings) => bindingContext.extend({
                    $gridViewDataSet: value,
                    [foreachCollectionSymbol]: dotvvm.evaluator.getDataSourceItems(value),
                    $gridViewDataSetHelper: {
                        columnMapping: allBindings.get("gridViewDataSetColumnMapping"),
                        isInEditMode: function ($context) {
                            let columnName = $context.$gridViewDataSet().RowEditOptions().PrimaryKeyPropertyName();
                            columnName = this.columnMapping[columnName] || columnName;
                            return ko.unwrap($context.$gridViewDataSet().RowEditOptions().EditRowId) === ko.unwrap($context.$data[columnName]);
                        }
                    }
                }),
                _ => true
            )
        };

        ko.bindingHandlers['dotvvmEnable'] = {
            'update': (element, valueAccessor) => {
                var value = ko.utils.unwrapObservable(valueAccessor());
                if (value && element.disabled) {
                    element.disabled = false;
                    element.removeAttribute("disabled");
                } else if ((!value) && (!element.disabled)) {
                    element.disabled = true;
                    element.setAttribute("disabled", "disabled");
                }
            }
        };
        ko.bindingHandlers['dotvvm-checkbox-updateAfterPostback'] = {
            init(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
                dotvvm.events.afterPostback.subscribe((e) => {
                    var bindings = allBindingsAccessor();
                    if (bindings["dotvvm-checked-pointer"]) {
                        var checked = bindings[bindings["dotvvm-checked-pointer"]];
                        if (ko.isObservable(checked)) {
                            if (checked.valueHasMutated) {
                                checked.valueHasMutated();
                            } else {
                                checked.notifySubscribers();
                            }
                        }
                    }
                });
            }
        };
        ko.bindingHandlers['dotvvm-checked-pointer'] = {
            init(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
            }
        };

        ko.bindingHandlers["dotvvm-checkedItems"] = {
            after: ko.bindingHandlers.checked.after,
            init: ko.bindingHandlers.checked.init,
            options: ko.bindingHandlers.checked.options,
            update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                var value = valueAccessor();
                if (!Array.isArray(ko.unwrap(value))) {
                    throw Error("The value of a `checkedItems` binding must be an array (i.e. not null nor undefined).");
                }
                // Note: As of now, the `checked` binding doesn't have an `update`. If that changes, invoke it here.
            }
        }

        ko.bindingHandlers["dotvvm-UpdateProgress-Visible"] = {
            init(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
                element.style.display = "none";
                var delay = element.getAttribute("data-delay");

                let includedQueues = (element.getAttribute("data-included-queues") || "").split(",").filter(i => i.length > 0);
                let excludedQueues = (element.getAttribute("data-excluded-queues") || "").split(",").filter(i => i.length > 0);

                var timeout;
                var running = false;

                var show = () => {
                    running = true;
                    if (delay == null) {
                        element.style.display = "";
                    } else {
                        timeout = setTimeout(e => {
                            element.style.display = "";
                        }, delay);
                    }
                }

                var hide = () => {
                    running = false;
                    clearTimeout(timeout);
                    element.style.display = "none";
                }

                dotvvm.updateProgressChangeCounter.subscribe(e => {
                    let shouldRun = false;

                    if (includedQueues.length === 0) {
                        for (let queue in dotvvm.postbackQueues) {
                            if (excludedQueues.indexOf(queue) < 0 && dotvvm.postbackQueues[queue].noRunning > 0) {
                                shouldRun = true;
                                break;
                            }
                        }
                    } else {
                        shouldRun = includedQueues.some(q => dotvvm.postbackQueues[q] && dotvvm.postbackQueues[q].noRunning > 0);
                    }

                    if (shouldRun) {
                        if (!running) {
                            show();
                        }
                    } else {
                        hide();
                    }
                });

            }
        };
        ko.bindingHandlers['dotvvm-table-columnvisible'] = {
            init(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
                let lastDisplay = "";
                let currentVisible = true;
                function changeVisibility(table: HTMLTableElement, columnIndex: number, visible: boolean) {
                    if (currentVisible == visible) return;
                    currentVisible = visible;
                    for (let i = 0; i < table.rows.length; i++) {
                        let row = <HTMLTableRowElement>table.rows.item(i);
                        let style = (<HTMLElement>row.cells[columnIndex]).style;
                        if (visible) {
                            style.display = lastDisplay;
                        }
                        else {
                            lastDisplay = style.display || "";
                            style.display = "none";
                        }
                    }
                }
                if (!(element instanceof HTMLTableCellElement)) return;
                // find parent table
                let table: any = element;
                while (!(table instanceof HTMLTableElement)) table = table.parentElement;
                const row = table.rows.item(0);
                if (!row) throw Error("Table with dotvvm-table-columnvisible binding handler must not be empty.")
                let colIndex = [].slice.call(row.cells).indexOf(element);


                element['dotvvmChangeVisibility'] = changeVisibility.bind(null, table, colIndex);
            },
            update(element, valueAccessor) {
                element.dotvvmChangeVisibility(ko.unwrap(valueAccessor()));
            }
        };
        ko.bindingHandlers['dotvvm-textbox-text'] = {
            init(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
                var obs = valueAccessor(),
                    valueUpdate = allBindingsAccessor.get("valueUpdate");

                //generate metadata func
                var elmMetadata = new DotvvmValidationElementMetadata();
                elmMetadata.element = element;
                elmMetadata.dataType = (element.attributes["data-dotvvm-value-type"] || { value: "" }).value;
                elmMetadata.format = (element.attributes["data-dotvvm-format"] || { value: "" }).value;

                //add metadata for validation
                var metadata = <DotvvmValidationObservableMetadata><any>null;
                if (ko.isObservable(obs)) {

                    if (!obs.dotvvmMetadata) {
                        obs.dotvvmMetadata = new DotvvmValidationObservableMetadata();
                        obs.dotvvmMetadata.elementsMetadata = [elmMetadata];
                    } else {
                        if (!obs.dotvvmMetadata.elementsMetadata) {
                            obs.dotvvmMetadata.elementsMetadata = [];
                        }
                        obs.dotvvmMetadata.elementsMetadata.push(elmMetadata);
                    }
                    metadata = obs.dotvvmMetadata.elementsMetadata;
                } else {
                    metadata = new DotvvmValidationObservableMetadata();
                }
                setTimeout((metaArray: DotvvmValidationElementMetadata[], element: HTMLElement) => {
                    // remove element from collection when its removed from dom
                    ko.utils.domNodeDisposal.addDisposeCallback(element, () => {
                        for (var meta of metaArray) {
                            if (meta.element === element) {
                                metaArray.splice(metaArray.indexOf(meta), 1);
                                break;
                            }
                        }
                    });
                }, 0, metadata, element);


                dotvvm.domUtils.attachEvent(element, "change", () => {
                    if (!ko.isObservable(obs)) return;
                    // parse the value
                    var result, isEmpty, newValue;
                    if (elmMetadata.dataType === "datetime") {
                        // parse date
                        var currentValue = obs();
                        if (currentValue != null) {
                            currentValue = dotvvm.globalize.parseDotvvmDate(currentValue);
                        }
                        result = dotvvm.globalize.parseDate(element.value, elmMetadata.format, currentValue);
                        isEmpty = result == null;
                        newValue = isEmpty ? null : dotvvm.serialization.serializeDate(result, false);
                    } else {
                        // parse number
                        result = dotvvm.globalize.parseNumber(element.value);
                        isEmpty = result === null || isNaN(result);
                        newValue = isEmpty ? null : result;
                    }

                    // update element validation metadata
                    if (newValue == null && element.value !== null && element.value !== "") {
                        element.attributes["data-invalid-value"] = element.value;
                        element.attributes["data-dotvvm-value-type-valid"] = false;
                        elmMetadata.elementValidationState = false;
                    } else {
                        element.attributes["data-invalid-value"] = null;
                        element.attributes["data-dotvvm-value-type-valid"] = true;
                        elmMetadata.elementValidationState = true;
                    }

                    if (obs() === newValue) {
                        if (obs.valueHasMutated) {
                            obs.valueHasMutated();
                        } else {
                            obs.notifySubscribers();
                        }
                    } else {
                        obs(newValue);
                    }
                });
            },
            update(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
                var obs = valueAccessor(),
                    format = (element.attributes["data-dotvvm-format"] || { value: "" }).value,
                    value = ko.unwrap(obs);

                if (format) {
                    var formatted = dotvvm.globalize.formatString(format, value),
                        invalidValue = element.attributes["data-invalid-value"];

                    if (invalidValue == null) {
                        element.value = formatted || "";

                        if (obs.dotvvmMetadata && obs.dotvvmMetadata.elementsMetadata) {
                            var elemsMetadata: DotvvmValidationElementMetadata[] = obs.dotvvmMetadata.elementsMetadata;

                            for (const elemMetadata of elemsMetadata) {
                                if (elemMetadata.element === element) {
                                    element.attributes["data-dotvvm-value-type-valid"] = true;
                                    elemMetadata.elementValidationState = true;
                                }
                            }
                        }
                    }
                    else {
                        element.attributes["data-invalid-value"] = null;
                        element.value = invalidValue;
                    }
                } else {
                    element.value = value;
                }
            }
        };
        ko.bindingHandlers["dotvvm-textbox-select-all-on-focus"] = {
            init(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
                element.$selectAllOnFocusHandler = () => {
                    element.select();
                };
            },
            update(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
                const value = ko.unwrap(valueAccessor());

                if (value === true) {
                    element.addEventListener("focus", element.$selectAllOnFocusHandler);
                }
                else {
                    element.removeEventListener("focus", element.$selectAllOnFocusHandler);
                }
            }
        };

        ko.bindingHandlers["dotvvm-CheckState"] = {
            init(element, valueAccessor, allBindings, viewModel, bindingContext) {
                ko.getBindingHandler("checked").init!(element, valueAccessor, allBindings, viewModel, bindingContext);
            },
            update(element, valueAccessor, allBindings) {
                let value = ko.unwrap(valueAccessor());
                element.indeterminate = value == null;
            }
        };

    }
}
