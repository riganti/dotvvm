import { setIdFragment } from './utils/dom'
import * as deserialization from './serialization/deserialize'
import * as serialization from './serialization/serialize'
import * as resourceLoader from './postback/resourceLoader'

import bindingHandlers from './binding-handlers/all-handlers'
import * as events from './events';
import * as spaEvents from './spa/events';

import { StateManager, DeepKnockoutWrapped } from './state-manager'

type DotvvmCoreState = {
    _culture: string
    _viewModelCache?: any
    _viewModelCacheId?: string
    _virtualDirectory: string
    _initialUrl: string,
    _validationRules: ValidationRuleTable,
    _stateManager: StateManager<RootViewModel>
}

let currentState: DotvvmCoreState | null = null

export function getViewModel() {
    return getStateManager().stateObservable()
}
export function getViewModelCacheId(): string | undefined {
    return currentState!._viewModelCacheId;
}
export function getViewModelCache(): any {
    return currentState!._viewModelCache;
}
export function getViewModelObservable(): DeepKnockoutWrapped<RootViewModel> {
    return currentState!._stateManager.stateObservable
}
export function getInitialUrl(): string {
    return currentState!._initialUrl
}
export function getVirtualDirectory(): string {
    return currentState!._virtualDirectory
}
export function replaceViewModel(vm: RootViewModel): void {
    getStateManager().setState(vm);
}
export function getState(): Readonly<RootViewModel> {
    return getStateManager().state
}
export function updateViewModelCache(viewModelCacheId: string, viewModelCache: any) {
    currentState!._viewModelCacheId = viewModelCacheId;
    currentState!._viewModelCache = viewModelCache;
}
export function clearViewModelCache() {
    delete currentState!._viewModelCacheId;
    delete currentState!._viewModelCache;
}
export function getValidationRules() {
    return currentState!._validationRules
}
export function getCulture(): string { return currentState!._culture; }

export function getStateManager(): StateManager<RootViewModel> { return currentState!._stateManager }

let initialViewModelWrapper: any;

export function initCore(culture: string): void {
    if (currentState) {
        throw new Error("DotVVM is already loaded");
    }

    // load the viewmodel
    const thisViewModel = initialViewModelWrapper = JSON.parse(getViewModelStorageElement().value);

    resourceLoader.registerResources(thisViewModel.renderedResources)

    setIdFragment(thisViewModel.resultIdFragment);

    const manager = new StateManager<RootViewModel>(thisViewModel.viewModel, events.newState)

    currentState = {
        _culture: culture,
        _initialUrl: thisViewModel.url,
        _virtualDirectory: thisViewModel.virtualDirectory!,
        _stateManager: manager,
        _validationRules: thisViewModel.validationRules || {}
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
            currentState = {
                _culture: a.serverResponseObject.culture,
                _initialUrl: a.serverResponseObject.url,
                _virtualDirectory: a.serverResponseObject.virtualDirectory!,
                _validationRules: a.serverResponseObject.validationRules || {},
                _stateManager: currentState!._stateManager
            }
        });
    }
}

export function initBindings() {
    ko.applyBindings(getViewModelObservable(), document.documentElement);
}

const getViewModelStorageElement = () =>
    <HTMLInputElement> document.getElementById("__dot_viewmodel_root")

function persistViewModel() {
    const viewModel = getState()
    const persistedViewModel = {...initialViewModelWrapper, viewModel };

    getViewModelStorageElement().value = JSON.stringify(persistedViewModel);
}
