import { setIdFragment } from './utils/dom'
import * as deserialization from './serialization/deserialize'
import * as serialization from './serialization/serialize'
import * as resourceLoader from './postback/resourceLoader'

import bindingHandlers from './binding-handlers/all-handlers'
import * as events from './events';
import * as spaEvents from './spa/events';

type DotvvmCoreState = {
    _culture: string
    _rootViewModel: KnockoutObservable<RootViewModel>
    _viewModelCache?: any
    _viewModelCacheId?: string
    _virtualDirectory: string
    _initialUrl: string,
    _validationRules: ValidationRuleTable
}

let currentState: DotvvmCoreState | null = null

export function getViewModel(): RootViewModel {
    return currentState!._rootViewModel()
}
export function getViewModelCacheId(): string | undefined {
    return currentState!._viewModelCacheId;
}
export function getViewModelCache(): any {
    return currentState!._viewModelCache;
}
export function getViewModelObservable(): KnockoutObservable<RootViewModel> {
    return currentState!._rootViewModel
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

let initialViewModelWrapper: any;

export function initCore(culture: string): void {
    if (currentState) {
        throw new Error("DotVVM is already loaded");
    }

    // load the viewmodel
    const thisViewModel = initialViewModelWrapper = JSON.parse(getViewModelStorageElement().value);

    resourceLoader.registerResources(thisViewModel.renderedResources)

    setIdFragment(thisViewModel.resultIdFragment);

    const viewModel: RootViewModel =
        deserialization.deserialize(thisViewModel.viewModel, {}, true)

    const vmObservable = ko.observable(viewModel)

    currentState = {
        _culture: culture,
        _initialUrl: thisViewModel.url,
        _virtualDirectory: thisViewModel.virtualDirectory!,
        _rootViewModel: vmObservable,
        _validationRules: thisViewModel.validationRules || {}
    }

    // store cached viewmodel
    if (thisViewModel.viewModelCacheId) {
        updateViewModelCache(thisViewModel.viewModelCacheId, thisViewModel.viewModel);
    }

    events.init.trigger({ viewModel });

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
                _rootViewModel: currentState!._rootViewModel,
                _validationRules: a.serverResponseObject.validationRules || {}
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
    const viewModel = serialization.serialize(getViewModel(), { serializeAll: true })
    const persistedViewModel = {...initialViewModelWrapper, viewModel };

    getViewModelStorageElement().value = JSON.stringify(persistedViewModel);
}
