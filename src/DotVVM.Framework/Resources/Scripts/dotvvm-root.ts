import { initCore, getViewModel, getViewModelObservable } from "./dotvvm-base"
import addPolyfills from './DotVVM.Polyfills'
import * as events from './DotVVM.Events'
import * as spa from "./spa/spa"

if (compileConstants.nomodules) {
    addPolyfills()
}

if (window["dotvvm"]) {
    throw 'DotVVM is already loaded!';
}
function init(culture: string) {

    initCore(culture)

    if (compileConstants.isSpa) {
        spa.init("root")
    }
}

const dotvvm = {
    // evaluator,
    // fileUpload,
    // getXHR,
    // globalize,
    // postBackHandlers,
    // handleSpaNavigation,
    // buildUrlSuffix,
    // isSpaReady,
    // buildRouteUrl,
    init,
    events,
    viewModels: {
        get root() { return getViewModel(); }
    },
    viewModelObservables: {
        get root() { return getViewModelObservable(); }
    }
}

window.dotvvm = dotvvm;
