import { initCore, getViewModel, getViewModelObservable, initBindings, getCulture, getState, getStateManager } from "./dotvvm-base"
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
import { logError, logWarning, logInfo, logInfoVerbose, level, logPostBackScriptError } from "./utils/logging"
import * as metadataHelper from './metadata/metadataHelper'
import * as array from './collections/arrayHelper'
import * as dictionary from './collections/dictionaryHelper'
import * as string from './utils/stringHelper'

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
        wrapObservable: evaluator.wrapObservable,
        evaluateExpression: evaluator.evaluateOnViewModel
    },
    fileUpload: {
        reportProgress: fileUpload.reportProgress,
        showUploadDialog: fileUpload.showUploadDialog
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
    postbackHandlers: postbackHandlers,
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
             events) as (Partial<typeof spaEvents> & typeof events),
    viewModels: {
        root: {
            get viewModel() { return getViewModel() }
        }
    },
    get state() { return getState() },
    patchState(a: any) {
        getStateManager().patchState(a)
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
    metadata: {
        getTypeId: metadataHelper.getTypeId,
        getTypeMetadata: metadataHelper.getTypeMetadata,
        getEnumMetadata: metadataHelper.getEnumMetadata,
        getEnumValue: metadataHelper.getEnumValue
    },
    viewModules: {
        registerOne: viewModuleManager.registerViewModule,
        init: viewModuleManager.initViewModule,
        call: viewModuleManager.callViewModuleCommand,
        registerMany: viewModuleManager.registerViewModules
    },
    resourceLoader: {
        notifyModuleLoaded
    },
    log: {
        logError,
        logWarning,
        logInfo,
        logInfoVerbose,
        logPostBackScriptError,
        level
    },
    translations: {
        array,
        dictionary,
        string
    } as any
}

if (compileConstants.isSpa) {
    (dotvvmExports as any).isSpaReady = isSpaReady;
    (dotvvmExports as any).handleSpaNavigation = handleSpaNavigation;
}

declare global {
    const dotvvm: typeof dotvvmExports & {isSpaReady?: typeof isSpaReady, handleSpaNavigation?: typeof handleSpaNavigation};

    interface Window {
        dotvvm: typeof dotvvmExports
    }
}

window.dotvvm = dotvvmExports;

export default dotvvmExports
