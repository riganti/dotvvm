/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout/knockout.dotvvm.d.ts" />
/// <reference path="typings/globalize/globalize.d.ts" />

import { setIdFragment } from './utils/dom'
import { DotvvmValidation } from './DotVVM.Validation'
import * as spa from './spa/spa';
import * as deserialization from './serialization/deserialize'
import * as serialization from './serialization/serialize'
import * as uri from './utils/uri'
import * as http from './postback/http'
import * as magicNavigator from './utils/magic-navigator'
import * as resourceLoader from './postback/resourceLoader'

import bindingHandlers from './binding-handlers/all-handlers'
import { events } from './DotVVM.Events';

type DotvvmCoreState = {
    _culture: string
    _rootViewModel: KnockoutObservable<RootViewModel>
    _virtualDirectory: string
    _initialUrl: string
}

let currentState: DotvvmCoreState | null = null

export function getViewModel(): RootViewModel {
    return currentState!._rootViewModel()
}
export function getViewModelObservable(): KnockoutObservable<RootViewModel> {
    return currentState!._rootViewModel
}
export function getRenderedResources(): any {
    return dotvvm.viewModels["root"].renderedResources;
}
export function getInitialUrl(): string {
    return currentState!._initialUrl
}
export function getVirtualDirectory(): string {
    return currentState!._virtualDirectory
}
export function replaceViewModel(vm: RootViewModel): void {
    currentState!._rootViewModel(vm);
}

let initialViewModelWrapper: any;

export function init(viewModelName: string, culture: string): void {
    if (currentState) throw new Error("DotVVM is already loaded");

    this.addKnockoutBindingHandlers();

    // load the viewmodel
    var thisViewModel = initialViewModelWrapper = JSON.parse(getViewModelStorageElement().value);

    resourceLoader.registerResources(thisViewModel.renderedResources)

    setIdFragment(thisViewModel.resultIdFragment);
    var viewModel: RootViewModel =
        deserialization.deserialize(thisViewModel.viewModel, {}, true)

    const vmObservable = ko.observable(viewModel)

    currentState = {
        _culture: culture,
        _initialUrl: thisViewModel.url,
        _virtualDirectory: thisViewModel.virtualDirectory!,
        _rootViewModel: vmObservable
    }
    // TODO: get validationRules from thisViewModel

    ko.applyBindings(vmObservable, document.documentElement);

    events.init.trigger({ viewModel });

    if (compileConstants.isSpa) {
        // TODO: move into spa
        spa.init(viewModelName);
    }

    // persist the viewmodel in the hidden field so the Back button will work correctly
    window.addEventListener("beforeunload", e => {
        persistViewModel(viewModelName);
    });
}

export const postbackHandlers : DotvvmPostbackHandlerCollection = {}

const getViewModelStorageElement = () =>
    <HTMLInputElement>document.getElementById("__dot_viewmodel_root")

function persistViewModel(viewModelName: string) {
    const viewModel = serialization.serialize(getViewModel(), { serializeAll: true })
    const persistedViewModel = {...initialViewModelWrapper, viewModel };

    getViewModelStorageElement().value = JSON.stringify(persistedViewModel);
}

export class DotVVM {
    private lastStartedPostack = 0; // TODO: increment the last postback

    public postbackHandlers: DotvvmPostbackHandlerCollection = {
        "concurrency-default": (o: any) => ({
            name: "concurrency-default",
            before: ["setIsPostbackRunning"],
            execute: (next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) => {
                return this.commonConcurrencyHandler(next(), options, o.q || "default")
            }
        }),
        "concurrency-deny": (o: any) => ({
            name: "concurrency-deny",
            before: ["setIsPostbackRunning"],
            execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
                var queue = o.q || "default";
                if (dotvvm.getPostbackQueue(queue).noRunning > 0)
                    return Promise.reject({ type: "handler", handler: this, message: "An postback is already running" });
                return dotvvm.commonConcurrencyHandler(next(), options, queue);
            }
        }),
        "concurrency-queue": (o: any) => ({
            name: "concurrency-queue",
            before: ["setIsPostbackRunning"],
            execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
                var queue = o.q || "default";
                var handler = () => dotvvm.commonConcurrencyHandler(next(), options, queue);

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
            execute(next: () => Promise<PostbackCommitFunction>, options: PostbackOptions) {
                if (dotvvm.isViewModelUpdating) return Promise.reject({ type: "handler", handler: this, message: "ViewModel is updating, so it's probably false onchange event" })
                else return next()
            }
        })
    }

    private commonConcurrencyHandler = <T>(promise: Promise<PostbackCommitFunction>, options: PostbackOptions, queueName: string): Promise<PostbackCommitFunction> => {
        const queue = this.getPostbackQueue(queueName)
        queue.noRunning++
        dotvvm.updateProgressChangeCounter(dotvvm.updateProgressChangeCounter() + 1);

        const dispatchNext = (args: DotvvmAfterPostBackEventArgs) => {
            const drop = () => {
                queue.noRunning--;
                dotvvm.updateProgressChangeCounter(dotvvm.updateProgressChangeCounter() - 1);
                if (queue.queue.length > 0) {
                    const callback = queue.queue.shift()!
                    window.setTimeout(callback, 0)
                }
            }
            if (args.redirectPromise) {
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

    public updateProgressChangeCounter = ko.observable(0);

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

    
    

    private handleRedirect(resultObject: any, viewModelName: string, replace: boolean = false): Promise<DotvvmNavigationEventArgs | void> {
        if (resultObject.replace != null) replace = resultObject.replace;
        var url = resultObject.url;

        // trigger redirect event
        var redirectArgs : DotvvmRedirectEventArgs = {
            viewModel: dotvvm.viewModels[viewModelName],
            viewModelName,
            url,
            replace,
        }
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

        const url = routePath.replace(/(\/[^\/]*?)\{([^\}]+?)\??(:(.+?))?\}/g, (s, prefix, paramName, _, type) => {
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

    private addKnockoutBindingHandlers() {
        for (const h of Object.keys(bindingHandlers)) {
            ko.bindingHandlers[h] = bindingHandlers[h];
        }
    }
}
