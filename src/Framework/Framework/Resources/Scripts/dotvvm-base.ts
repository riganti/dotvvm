import { setIdFragment } from './utils/dom'
import * as deserialization from './serialization/deserialize'
import * as serialization from './serialization/serialize'
import * as resourceLoader from './postback/resourceLoader'

import bindingHandlers from './binding-handlers/all-handlers'
import * as events from './events';
import * as spaEvents from './spa/events';
import { replaceTypeInfo } from './metadata/typeMap'

import { StateManager } from './state-manager'

type DotvvmCoreState = {
    _culture: string
    _viewModelCache?: any
    _viewModelCacheId?: string
    _virtualDirectory: string
    _initialUrl: string,
    _routeName: string,
    _routeParameters: {
        [name: string]: any
    },
    _stateManager: StateManager<RootViewModel>
}

let currentCoreState: DotvvmCoreState | null = null

function getCoreState() {
    if (!currentCoreState)
        throw new Error("DotVVM is not initialized.")
    return currentCoreState
}

export function getViewModel() {
    return getStateManager().stateObservable()
}
export function getViewModelCacheId(): string | undefined {
    return getCoreState()._viewModelCacheId;
}
export function getViewModelCache(): any {
    return getCoreState()._viewModelCache;
}
export function getViewModelObservable(): DeepKnockoutObservable<RootViewModel> {
    return getCoreState()._stateManager.stateObservable
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
export function getRouteName(): string { return getCoreState()._routeName; }
export function getRouteParameters(): { [name: string]: any } { return getCoreState()._routeParameters; }
export function getStateManager(): StateManager<RootViewModel> { return getCoreState()._stateManager }

let initialViewModelWrapper: any;

export function initCore(culture: string): void {
    if (currentCoreState) {
        throw new Error("DotVVM is already loaded");
    }

    // load the viewmodel
    const thisViewModel = initialViewModelWrapper = JSON.parse(getViewModelStorageElement().value);

    resourceLoader.registerResources(thisViewModel.renderedResources)

    setIdFragment(thisViewModel.resultIdFragment);

    replaceTypeInfo(thisViewModel.typeMetadata);

    const manager = new StateManager<RootViewModel>(thisViewModel.viewModel, events.newState)

    currentCoreState = {
        _culture: culture,
        _initialUrl: thisViewModel.url,
        _virtualDirectory: thisViewModel.virtualDirectory!,
        _stateManager: manager,
        _routeName: thisViewModel.routeName,
        _routeParameters: thisViewModel.routeParameters
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
                _stateManager: currentCoreState!._stateManager,
                _routeName: a.serverResponseObject.routeName,
                _routeParameters: a.serverResponseObject.routeParameters
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
    const viewModel = getState()
    const persistedViewModel = { ...initialViewModelWrapper, viewModel };

    getViewModelStorageElement().value = JSON.stringify(persistedViewModel);
}
