import { initCore, getViewModel, getViewModelObservable } from "./dotvvm-base"
import addPolyfills from './DotVVM.Polyfills'
import * as events from './events'
import * as spa from "./spa/spa"
import * as validation from './validation/validation'
import { postBack } from './postback/postback'
import { serialize } from './serialization/serialize'
import { deserialize } from './serialization/deserialize'

if (compileConstants.nomodules) {
    addPolyfills()
}

if (window["dotvvm"]) {
    throw new Error('DotVVM is already loaded!')
}
function init(culture: string) {

    initCore(culture)

    validation.init();

    if (compileConstants.isSpa) {
        spa.init()
    }
}

const dotvvm_g = {
    // evaluator,
    // fileUpload,
    // getXHR,
    // globalize,
    // postBackHandlers,
    // handleSpaNavigation,
    // buildUrlSuffix,
    // isSpaReady,
    // buildRouteUrl,
    validation: validation.globalValidationObject,
    postBack,
    init,
    events,
    viewModels: {
        root: {
            get viewModel() { return getViewModel() }
        }
    },
    viewModelObservables: {
        get root() { return getViewModelObservable(); }
    },
    serialization: {
        serialize,
        deserialize
    }
}

declare global {
    const dotvvm: typeof dotvvm_g;

    interface Window {
        dotvvm: typeof dotvvm_g
    }
}

window.dotvvm = dotvvm;
