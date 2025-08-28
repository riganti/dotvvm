import { setIdFragment } from './utils/dom'
import * as deserialization from './serialization/deserialize'
import * as serialization from './serialization/serialize'
import * as resourceLoader from './postback/resourceLoader'

import bindingHandlers from './binding-handlers/all-handlers'
import * as events from './events';
import * as spaEvents from './spa/events';
import { getKnownTypes, replaceTypeInfo, getCurrentTypeMap } from './metadata/typeMap'

import { StateManager } from './state-manager'
import { getTypeMetadata } from './metadata/metadataHelper'

export const options = {
    compressPOST: true
}

type DotvvmCoreState = {
    _culture: string
    _viewModelCache?: any
    _viewModelCacheId?: string
    _virtualDirectory: string
    _initialUrl: string,
    _stateManager: StateManager<RootViewModel>
}

let currentCoreState: DotvvmCoreState | null = null

function getCoreState() {
    if (!currentCoreState)
        throw new Error("DotVVM is not initialized.")
    return currentCoreState
}

export function getViewModel() {
    return getViewModelObservable()()
}
export function getViewModelCacheId(): string | undefined {
    return getCoreState()._viewModelCacheId;
}
export function getViewModelCache(): any {
    return getCoreState()._viewModelCache;
}
export function getViewModelObservable(): DeepKnockoutObservable<RootViewModel> {
    return getStateManager().stateObservable
}
export function getInitialUrl(): string {
    return getCoreState()._initialUrl
}
export function getVirtualDirectory(): string {
    return getCoreState()._virtualDirectory
}
export function replaceViewModel(vm: RootViewModel): void {
    getStateManager().setState(vm);
}
export function getState(): Readonly<RootViewModel> {
    return getStateManager().state
}
export function updateViewModelCache(viewModelCacheId: string, viewModelCache: any) {
    getCoreState()._viewModelCacheId = viewModelCacheId;
    getCoreState()._viewModelCache = viewModelCache;
}
export function clearViewModelCache() {
    delete getCoreState()._viewModelCacheId;
    delete getCoreState()._viewModelCache;
}
export function getCulture(): string { return getCoreState()._culture; }

export function getStateManager(): StateManager<RootViewModel> { return getCoreState()._stateManager }

let initialViewModelWrapper: any;

function isBackForwardNavigation() {
    return (performance.getEntriesByType?.("navigation").at(-1) as PerformanceNavigationTiming)?.type == "back_forward";
}

export function initCore(culture: string): void {
    if (currentCoreState) {
        throw new Error("DotVVM is already loaded");
    }

    // load the viewmodel
    const thisViewModel = initialViewModelWrapper =
        (isBackForwardNavigation() ? history.state?.viewModel : null) ??
        JSON.parse(getViewModelStorageElement().value);

    resourceLoader.registerResources(thisViewModel.renderedResources)

    setIdFragment(thisViewModel.resultIdFragment);

    replaceTypeInfo(thisViewModel.typeMetadata);

    const manager = new StateManager<RootViewModel>(thisViewModel.viewModel, events.newState)

    currentCoreState = {
        _culture: culture,
        _initialUrl: thisViewModel.url,
        _virtualDirectory: thisViewModel.virtualDirectory!,
        _stateManager: manager
    }

    // store cached viewmodel
    if (thisViewModel.viewModelCacheId) {
        updateViewModelCache(thisViewModel.viewModelCacheId, thisViewModel.viewModel);
    }

    events.init.trigger({ viewModel: manager.state });

    // persist the viewmodel in the hidden field so the Back button will work correctly
    window.addEventListener("beforeunload", e => {
        persistViewModel();
    });

    if (compileConstants.isSpa) {
        spaEvents.spaNavigated.subscribe(a => {
            currentCoreState = {
                _culture: currentCoreState!._culture,
                _initialUrl: a.serverResponseObject.url,
                _virtualDirectory: a.serverResponseObject.virtualDirectory!,
                _stateManager: currentCoreState!._stateManager
            }
        });
    }
}

export function initBindings() {
    ko.applyBindings(getViewModelObservable(), document.documentElement);
}

const getViewModelStorageElement = () =>
    <HTMLInputElement>document.getElementById("__dot_viewmodel_root")

function persistViewModel() {
    history.replaceState({
        ...history.state,
        viewModel: {
            ...initialViewModelWrapper,
            typeMetadata: getCurrentTypeMap(),
            viewModel: getState(),
            viewModelCacheId: getViewModelCacheId(),
            url: history.state.url
        }
    }, "")
    // avoid storing the viewmodel hidden field, as Firefox would also reuse it on page reloads
    getViewModelStorageElement()?.remove()
}
