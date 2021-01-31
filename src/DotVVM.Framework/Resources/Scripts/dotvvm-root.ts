import { initCore, getViewModel, getViewModelObservable, initBindings, getCulture } from "./dotvvm-base"
import addPolyfills from './DotVVM.Polyfills'
import * as events from './events'
import * as spa from "./spa/spa"
import * as validation from './validation/validation'
import { postBack } from './postback/postback'
import { serialize } from './serialization/serialize'
import { serializeDate, parseDate } from './serialization/date'
import { deserialize } from './serialization/deserialize'
import registerBindingHandlers from './binding-handlers/register'
import * as evaluator from './utils/evaluator'
import * as globalize from './DotVVM.Globalize'
import { staticCommandPostback } from './postback/staticCommand'
import { applyPostbackHandlers } from './postback/postback'
import { isSpaReady } from "./spa/spa"
import { buildRouteUrl, buildUrlSuffix } from './controls/routeLink'
import * as fileUpload from './controls/fileUpload'
import { handleSpaNavigation } from './spa/spa'
import { postbackHandlers } from './postback/handlers'
import * as spaEvents from './spa/events'
import { isPostbackRunning } from "./postback/internal-handlers"
import * as api from './api/api'
import * as eventHub from './api/eventHub'
import * as viewModuleManager from './viewModules/viewModuleManager'
import { notifyModuleLoaded } from './postback/resourceLoader'

if (compileConstants.nomodules) {
    addPolyfills()
}

if (window["dotvvm"]) {
    throw new Error('DotVVM is already loaded!')
}
function init(culture: string) {

    initCore(culture)
    registerBindingHandlers()
    validation.init()

    if (compileConstants.isSpa) {
        spa.init()
    }

    initBindings()

    events.initCompleted.trigger({})
}

const dotvvmExports = {
    getCulture: getCulture,
    evaluator: {
        getDataSourceItems: evaluator.getDataSourceItems,
        wrapObservable: evaluator.wrapObservable
    },
    fileUpload: {
        reportProgress: fileUpload.reportProgress,
        showUploadDialog: fileUpload.showUploadDialog,
        createUploadId: fileUpload.createUploadId
    },
    api: {
        invoke: api.invoke,
        refreshOn: api.refreshOn
    },
    eventHub: {
        get: eventHub.get,
        notify: eventHub.notify
    },
    globalize,
    postBackHandlers: postbackHandlers,
    buildUrlSuffix,
    buildRouteUrl,
    staticCommandPostback,
    applyPostbackHandlers,
    validation: validation.globalValidationObject,
    postBack,
    init,
    isPostbackRunning,
    events: (compileConstants.isSpa ?
             { ...events, ...spaEvents } :
             events),
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
        serializeDate,
        parseDate,
        deserialize
    },
    viewModules: {
        registerOne: viewModuleManager.registerViewModule,
        init: viewModuleManager.initViewModule,
        call: viewModuleManager.callViewModuleCommand,
        registerNamedCommand: viewModuleManager.registerNamedCommand,
        registerMany: viewModuleManager.registerViewModules
    },
    resourceLoader: {
        notifyModuleLoaded
    }
}

if (compileConstants.isSpa) {
    (dotvvmExports as any).isSpaReady = isSpaReady;
    (dotvvmExports as any).handleSpaNavigation = handleSpaNavigation;
}

declare global {
    const dotvvm: typeof dotvvmExports;

    interface Window {
        dotvvm: typeof dotvvmExports
    }
}

window.dotvvm = dotvvmExports;

export default dotvvmExports
