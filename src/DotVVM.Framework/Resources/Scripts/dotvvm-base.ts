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
