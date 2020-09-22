var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var __assign = (this && this.__assign) || function () {
    __assign = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign.apply(this, arguments);
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
(function () {
    if (typeof Promise === 'undefined' || !self.fetch) {
        var resource = document.createElement('script');
        resource.src = window['dotvvm__polyfillUrl'];
        resource.type = "text/javascript";
        var headElement = document.getElementsByTagName('head')[0];
        headElement.appendChild(resource);
    }
})();
var DotvvmDomUtils = /** @class */ (function () {
    function DotvvmDomUtils() {
    }
    DotvvmDomUtils.prototype.onDocumentReady = function (callback) {
        // many thanks to http://dustindiaz.com/smallest-domready-ever
        /in/.test(document.readyState) ? setTimeout('dotvvm.domUtils.onDocumentReady(' + callback + ')', 9) : callback();
    };
    DotvvmDomUtils.prototype.attachEvent = function (target, name, callback, useCapture) {
        if (useCapture === void 0) { useCapture = false; }
        if (target.addEventListener) {
            target.addEventListener(name, callback, useCapture);
        }
        else {
            target.attachEvent("on" + name, callback);
        }
    };
    return DotvvmDomUtils;
}());
var HistoryRecord = /** @class */ (function () {
    function HistoryRecord(navigationType, url) {
        this.navigationType = navigationType;
        this.url = url;
    }
    return HistoryRecord;
}());
var DotvvmSpaHistory = /** @class */ (function () {
    function DotvvmSpaHistory() {
    }
    DotvvmSpaHistory.prototype.pushPage = function (url) {
        // pushState doesn't work when the url is empty
        url = url || "/";
        history.pushState(new HistoryRecord('SPA', url), '', url);
    };
    DotvvmSpaHistory.prototype.replacePage = function (url) {
        history.replaceState(new HistoryRecord('SPA', url), '', url);
    };
    DotvvmSpaHistory.prototype.isSpaPage = function (state) {
        return state && state.navigationType == 'SPA';
    };
    DotvvmSpaHistory.prototype.getHistoryRecord = function (state) {
        return state;
    };
    return DotvvmSpaHistory;
}());
var DotvvmEvents = /** @class */ (function () {
    function DotvvmEvents() {
        this.init = new DotvvmEvent("dotvvm.events.init", true);
        this.beforePostback = new DotvvmEvent("dotvvm.events.beforePostback");
        this.afterPostback = new DotvvmEvent("dotvvm.events.afterPostback");
        this.error = new DotvvmEvent("dotvvm.events.error");
        this.spaNavigating = new DotvvmEvent("dotvvm.events.spaNavigating");
        this.spaNavigated = new DotvvmEvent("dotvvm.events.spaNavigated");
        this.redirect = new DotvvmEvent("dotvvm.events.redirect");
        this.postbackHandlersStarted = new DotvvmEvent("dotvvm.events.postbackHandlersStarted");
        this.postbackHandlersCompleted = new DotvvmEvent("dotvvm.events.postbackHandlersCompleted");
        this.postbackResponseReceived = new DotvvmEvent("dotvvm.events.postbackResponseReceived");
        this.postbackCommitInvoked = new DotvvmEvent("dotvvm.events.postbackCommitInvoked");
        this.postbackViewModelUpdated = new DotvvmEvent("dotvvm.events.postbackViewModelUpdated");
        this.postbackRejected = new DotvvmEvent("dotvvm.events.postbackRejected");
        this.staticCommandMethodInvoking = new DotvvmEvent("dotvvm.events.staticCommandMethodInvoking");
        this.staticCommandMethodInvoked = new DotvvmEvent("dotvvm.events.staticCommandMethodInvoked");
        this.staticCommandMethodFailed = new DotvvmEvent("dotvvm.events.staticCommandMethodInvoked");
    }
    return DotvvmEvents;
}());
var DotvvmEventHandler = /** @class */ (function () {
    function DotvvmEventHandler(handler, isOneTime) {
        this.handler = handler;
        this.isOneTime = isOneTime;
    }
    return DotvvmEventHandler;
}());
// DotvvmEvent is used because CustomEvent is not browser compatible and does not support
// calling missed events for handler that subscribed too late.
var DotvvmEvent = /** @class */ (function () {
    function DotvvmEvent(name, triggerMissedEventsOnSubscribe) {
        if (triggerMissedEventsOnSubscribe === void 0) { triggerMissedEventsOnSubscribe = false; }
        this.name = name;
        this.triggerMissedEventsOnSubscribe = triggerMissedEventsOnSubscribe;
        this.handlers = [];
        this.history = [];
    }
    DotvvmEvent.prototype.subscribe = function (handler) {
        this.handlers.push(new DotvvmEventHandler(handler, false));
        if (this.triggerMissedEventsOnSubscribe) {
            for (var i = 0; i < this.history.length; i++) {
                handler(history[i]);
            }
        }
    };
    DotvvmEvent.prototype.subscribeOnce = function (handler) {
        this.handlers.push(new DotvvmEventHandler(handler, true));
    };
    DotvvmEvent.prototype.unsubscribe = function (handler) {
        for (var i = 0; i < this.handlers.length; i++) {
            if (this.handlers[i].handler === handler) {
                this.handlers.splice(i, 1);
                return;
            }
        }
    };
    DotvvmEvent.prototype.trigger = function (data) {
        for (var i = 0; i < this.handlers.length; i++) {
            this.handlers[i].handler(data);
            if (this.handlers[i].isOneTime) {
                this.handlers.splice(i, 1);
                i--;
            }
        }
        if (this.triggerMissedEventsOnSubscribe) {
            this.history.push(data);
        }
    };
    return DotvvmEvent;
}());
var DotvvmErrorEventArgs = /** @class */ (function () {
    function DotvvmErrorEventArgs(sender, viewModel, viewModelName, xhr, postbackClientId, serverResponseObject, isSpaNavigationError) {
        if (serverResponseObject === void 0) { serverResponseObject = undefined; }
        if (isSpaNavigationError === void 0) { isSpaNavigationError = false; }
        this.sender = sender;
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.xhr = xhr;
        this.postbackClientId = postbackClientId;
        this.serverResponseObject = serverResponseObject;
        this.isSpaNavigationError = isSpaNavigationError;
        this.handled = false;
    }
    return DotvvmErrorEventArgs;
}());
var DotvvmBeforePostBackEventArgs = /** @class */ (function () {
    function DotvvmBeforePostBackEventArgs(sender, viewModel, viewModelName, postbackClientId) {
        this.sender = sender;
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.postbackClientId = postbackClientId;
        this.cancel = false;
        this.clientValidationFailed = false;
    }
    return DotvvmBeforePostBackEventArgs;
}());
var DotvvmAfterPostBackEventArgs = /** @class */ (function () {
    function DotvvmAfterPostBackEventArgs(postbackOptions, serverResponseObject, commandResult, xhr) {
        if (commandResult === void 0) { commandResult = null; }
        this.postbackOptions = postbackOptions;
        this.serverResponseObject = serverResponseObject;
        this.commandResult = commandResult;
        this.xhr = xhr;
        this.isHandled = false;
        this.wasInterrupted = false;
    }
    Object.defineProperty(DotvvmAfterPostBackEventArgs.prototype, "postbackClientId", {
        get: function () { return this.postbackOptions.postbackId; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(DotvvmAfterPostBackEventArgs.prototype, "viewModelName", {
        get: function () { return this.postbackOptions.viewModelName; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(DotvvmAfterPostBackEventArgs.prototype, "viewModel", {
        get: function () { return this.postbackOptions.viewModel; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(DotvvmAfterPostBackEventArgs.prototype, "sender", {
        get: function () { return this.postbackOptions.sender; },
        enumerable: true,
        configurable: true
    });
    return DotvvmAfterPostBackEventArgs;
}());
var DotvvmAfterPostBackWithRedirectEventArgs = /** @class */ (function (_super) {
    __extends(DotvvmAfterPostBackWithRedirectEventArgs, _super);
    function DotvvmAfterPostBackWithRedirectEventArgs(postbackOptions, serverResponseObject, commandResult, xhr, _redirectPromise) {
        if (commandResult === void 0) { commandResult = null; }
        var _this = _super.call(this, postbackOptions, serverResponseObject, commandResult, xhr) || this;
        _this._redirectPromise = _redirectPromise;
        return _this;
    }
    Object.defineProperty(DotvvmAfterPostBackWithRedirectEventArgs.prototype, "redirectPromise", {
        get: function () { return this._redirectPromise; },
        enumerable: true,
        configurable: true
    });
    return DotvvmAfterPostBackWithRedirectEventArgs;
}(DotvvmAfterPostBackEventArgs));
var DotvvmSpaNavigatingEventArgs = /** @class */ (function () {
    function DotvvmSpaNavigatingEventArgs(viewModel, viewModelName, newUrl) {
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.newUrl = newUrl;
        this.cancel = false;
    }
    return DotvvmSpaNavigatingEventArgs;
}());
var DotvvmNavigationEventArgs = /** @class */ (function () {
    function DotvvmNavigationEventArgs(viewModel, viewModelName, serverResponseObject, xhr) {
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.serverResponseObject = serverResponseObject;
        this.xhr = xhr;
        this.isHandled = false;
    }
    return DotvvmNavigationEventArgs;
}());
var DotvvmSpaNavigatedEventArgs = /** @class */ (function (_super) {
    __extends(DotvvmSpaNavigatedEventArgs, _super);
    function DotvvmSpaNavigatedEventArgs() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    return DotvvmSpaNavigatedEventArgs;
}(DotvvmNavigationEventArgs));
var DotvvmRedirectEventArgs = /** @class */ (function () {
    function DotvvmRedirectEventArgs(viewModel, viewModelName, url, replace) {
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.url = url;
        this.replace = replace;
        this.isHandled = false;
    }
    return DotvvmRedirectEventArgs;
}());
var DotvvmFileUpload = /** @class */ (function () {
    function DotvvmFileUpload() {
    }
    DotvvmFileUpload.prototype.showUploadDialog = function (sender) {
        // trigger the file upload dialog
        var iframe = this.getIframe(sender);
        this.createUploadId(sender, iframe);
        this.openUploadDialog(iframe);
    };
    DotvvmFileUpload.prototype.getIframe = function (sender) {
        return sender.parentElement.previousSibling;
    };
    DotvvmFileUpload.prototype.openUploadDialog = function (iframe) {
        var window = iframe.contentWindow;
        if (window) {
            var fileUpload = window.document.getElementById('upload');
            fileUpload.click();
        }
    };
    DotvvmFileUpload.prototype.createUploadId = function (sender, iframe) {
        iframe = iframe || this.getIframe(sender);
        var uploadId = "DotVVM_upl" + new Date().getTime().toString();
        sender.parentElement.parentElement.setAttribute("data-dotvvm-upload-id", uploadId);
        iframe.setAttribute("data-dotvvm-upload-id", uploadId);
    };
    DotvvmFileUpload.prototype.reportProgress = function (targetControlId, isBusy, progress, result) {
        // find target control viewmodel
        var targetControl = document.querySelector("div[data-dotvvm-upload-id='" + targetControlId.value + "']");
        var viewModel = ko.dataFor(targetControl.firstChild);
        // determine the status
        if (typeof result === "string") {
            // error during upload
            viewModel.Error(result);
        }
        else {
            // files were uploaded successfully
            viewModel.Error("");
            for (var i = 0; i < result.length; i++) {
                viewModel.Files.push(dotvvm.serialization.wrapObservable(dotvvm.serialization.deserialize(result[i])));
            }
            // call the handler
            if ((targetControl.attributes["data-dotvvm-upload-completed"] || { value: null }).value) {
                new Function(targetControl.attributes["data-dotvvm-upload-completed"].value).call(targetControl);
            }
        }
        viewModel.Progress(progress);
        viewModel.IsBusy(isBusy);
    };
    return DotvvmFileUpload;
}());
var DotvvmFileUploadCollection = /** @class */ (function () {
    function DotvvmFileUploadCollection() {
        this.Files = ko.observableArray();
        this.Progress = ko.observable(0);
        this.Error = ko.observable();
        this.IsBusy = ko.observable();
    }
    return DotvvmFileUploadCollection;
}());
var DotvvmFileUploadData = /** @class */ (function () {
    function DotvvmFileUploadData() {
        this.FileId = ko.observable();
        this.FileName = ko.observable();
        this.FileSize = ko.observable();
        this.IsFileTypeAllowed = ko.observable();
        this.IsMaxSizeExceeded = ko.observable();
        this.IsAllowed = ko.observable();
    }
    return DotvvmFileUploadData;
}());
var DotvvmFileSize = /** @class */ (function () {
    function DotvvmFileSize() {
        this.Bytes = ko.observable();
        this.FormattedText = ko.observable();
    }
    return DotvvmFileSize;
}());
var DotvvmGlobalize = /** @class */ (function () {
    function DotvvmGlobalize() {
    }
    DotvvmGlobalize.prototype.getGlobalize = function () {
        var g = window["dotvvm_Globalize"];
        if (!g) {
            throw new Error("Resource 'globalize' is not included (symbol 'dotvvm_Globalize' could not be found).\nIt is usually included automatically when needed, but sometime it's not possible, so you will have to include it in your page using '<dot:RequiredResource Name=\"globalize\" />'");
        }
        return g;
    };
    DotvvmGlobalize.prototype.format = function (format) {
        var _this = this;
        var values = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            values[_i - 1] = arguments[_i];
        }
        return format.replace(/\{([1-9]?[0-9]+)(:[^}]+)?\}/g, function (match, group0, group1) {
            var value = values[parseInt(group0)];
            if (group1) {
                return _this.formatString(group1, value);
            }
            else {
                return value;
            }
        });
    };
    DotvvmGlobalize.prototype.formatString = function (format, value) {
        value = ko.unwrap(value);
        if (value == null || value === "")
            return "";
        if (typeof value === "string") {
            // JSON date in string
            value = this.parseDotvvmDate(value);
            if (value == null) {
                throw new Error("Could not parse " + value + " as a date");
            }
        }
        if (format === "" || format === null) {
            format = "G";
        }
        return this.getGlobalize().format(value, format, dotvvm.culture);
    };
    DotvvmGlobalize.prototype.parseDotvvmDate = function (value) {
        var match = value.match("^([0-9]{4})-([0-9]{2})-([0-9]{2})T([0-9]{2}):([0-9]{2}):([0-9]{2})(\\.[0-9]{3,7})$");
        if (match) {
            return new Date(parseInt(match[1]), parseInt(match[2]) - 1, parseInt(match[3]), parseInt(match[4]), parseInt(match[5]), parseInt(match[6]), match.length > 7 ? parseInt(match[7].substring(1, 4)) : 0);
        }
        return null;
    };
    DotvvmGlobalize.prototype.parseNumber = function (value) {
        return this.getGlobalize().parseFloat(value, 10, dotvvm.culture);
    };
    DotvvmGlobalize.prototype.parseDate = function (value, format, previousValue) {
        return this.getGlobalize().parseDate(value, format, dotvvm.culture, previousValue);
    };
    DotvvmGlobalize.prototype.bindingDateToString = function (value, format) {
        var _this = this;
        if (format === void 0) { format = "G"; }
        if (!value) {
            return "";
        }
        var unwrapDate = function () {
            var unwrappedVal = ko.unwrap(value);
            return typeof unwrappedVal == "string" ? _this.parseDotvvmDate(unwrappedVal) : unwrappedVal;
        };
        var formatDate = function () {
            var unwrappedVal = unwrapDate();
            if (unwrappedVal != null) {
                return _this.getGlobalize().format(unwrappedVal, format, dotvvm.culture);
            }
            return "";
        };
        if (ko.isWriteableObservable(value)) {
            var unwrappedVal = unwrapDate();
            var setter_1 = typeof unwrappedVal == "string" ? function (v) {
                return value(v == null ? null : dotvvm.serialization.serializeDate(v, false));
            } : value;
            return ko.pureComputed({
                read: function () { return formatDate(); },
                write: function (val) { return setter_1(_this.getGlobalize().parseDate(val, format, dotvvm.culture)); }
            });
        }
        else {
            return ko.pureComputed(function () { return formatDate(); });
        }
    };
    DotvvmGlobalize.prototype.bindingNumberToString = function (value, format) {
        var _this = this;
        if (format === void 0) { format = "G"; }
        if (value == null) {
            return "";
        }
        var unwrapNumber = function () {
            var unwrappedVal = ko.unwrap(value);
            return typeof unwrappedVal == "string" ? _this.parseNumber(unwrappedVal) : unwrappedVal;
        };
        var formatNumber = function () {
            var unwrappedVal = unwrapNumber();
            if (unwrappedVal != null) {
                return _this.getGlobalize().format(unwrappedVal, format, dotvvm.culture);
            }
            return "";
        };
        if (ko.isWriteableObservable(value)) {
            return ko.pureComputed({
                read: function () { return formatNumber(); },
                write: function (val) {
                    var parsedFloat = _this.getGlobalize().parseFloat(val, 10, dotvvm.culture), isValid = val == null || (parsedFloat != null && !isNaN(parsedFloat));
                    value(isValid ? parsedFloat : null);
                }
            });
        }
        else {
            return ko.pureComputed(function () { return formatNumber(); });
        }
    };
    return DotvvmGlobalize;
}());
var PostbackOptions = /** @class */ (function () {
    function PostbackOptions(postbackId, sender, args, viewModel, viewModelName) {
        if (args === void 0) { args = []; }
        this.postbackId = postbackId;
        this.sender = sender;
        this.args = args;
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.additionalPostbackData = {};
    }
    return PostbackOptions;
}());
var ConfirmPostBackHandler = /** @class */ (function () {
    function ConfirmPostBackHandler(message) {
        this.message = message;
    }
    ConfirmPostBackHandler.prototype.execute = function (callback, options) {
        var _this = this;
        return new Promise(function (resolve, reject) {
            if (confirm(_this.message)) {
                callback().then(resolve, reject);
            }
            else {
                reject({ type: "handler", handler: _this, message: "The postback was not confirmed" });
            }
        });
    };
    return ConfirmPostBackHandler;
}());
var SuppressPostBackHandler = /** @class */ (function () {
    function SuppressPostBackHandler(suppress) {
        this.suppress = suppress;
    }
    SuppressPostBackHandler.prototype.execute = function (callback, options) {
        var _this = this;
        return new Promise(function (resolve, reject) {
            if (_this.suppress) {
                reject({ type: "handler", handler: _this, message: "The postback was suppressed" });
            }
            else {
                callback().then(resolve, reject);
            }
        });
    };
    return SuppressPostBackHandler;
}());
var DotvvmSerialization = /** @class */ (function () {
    function DotvvmSerialization() {
    }
    DotvvmSerialization.prototype.wrapObservable = function (obj) {
        if (!ko.isObservable(obj))
            return ko.observable(obj);
        return obj;
    };
    DotvvmSerialization.prototype.deserialize = function (viewModel, target, deserializeAll) {
        if (deserializeAll === void 0) { deserializeAll = false; }
        if (ko.isObservable(viewModel)) {
            throw new Error("Parameter viewModel should not be an observable. Maybe you forget to invoke the observable you are passing as a viewModel parameter.");
        }
        if (this.isPrimitive(viewModel)) {
            return this.deserializePrimitive(viewModel, target);
        }
        if (viewModel instanceof Date) {
            return this.deserializeDate(viewModel, target);
        }
        if (viewModel instanceof Array) {
            return this.deserializeArray(viewModel, target, deserializeAll);
        }
        return this.deserializeObject(viewModel, target, deserializeAll);
    };
    DotvvmSerialization.prototype.deserializePrimitive = function (viewModel, target) {
        if (ko.isObservable(target)) {
            target(viewModel);
            return target;
        }
        return viewModel;
    };
    DotvvmSerialization.prototype.deserializeDate = function (viewModel, target) {
        viewModel = dotvvm.serialization.serializeDate(viewModel);
        if (ko.isObservable(target)) {
            target(viewModel);
            return target;
        }
        return viewModel;
    };
    DotvvmSerialization.prototype.deserializeArray = function (viewModel, target, deserializeAll) {
        if (deserializeAll === void 0) { deserializeAll = false; }
        if (this.isObservableArray(target) && target() != null && target().length === viewModel.length) {
            this.updateArrayItems(viewModel, target, deserializeAll);
        }
        else {
            target = this.rebuildArrayFromScratch(viewModel, target, deserializeAll);
        }
        return target;
    };
    DotvvmSerialization.prototype.rebuildArrayFromScratch = function (viewModel, target, deserializeAll) {
        var array = [];
        for (var i = 0; i < viewModel.length; i++) {
            array.push(this.wrapObservableObjectOrArray(this.deserialize(ko.unwrap(viewModel[i]), {}, deserializeAll)));
        }
        if (ko.isObservable(target)) {
            target = this.extendToObservableArrayIfRequired(target);
            target(array);
        }
        else {
            target = array;
        }
        return target;
    };
    DotvvmSerialization.prototype.updateArrayItems = function (viewModel, target, deserializeAll) {
        var targetArray = target();
        for (var i = 0; i < viewModel.length; i++) {
            var targetItem = ko.unwrap(targetArray[i]);
            var deserialized = this.deserialize(ko.unwrap(viewModel[i]), targetItem, deserializeAll);
            if (targetItem !== deserialized) {
                //update the item
                if (ko.isObservable(targetArray[i])) {
                    if (targetArray[i]() !== deserialized) {
                        targetArray[i] = this.extendToObservableArrayIfRequired(targetArray[i]);
                        targetArray[i](deserialized);
                    }
                }
                else {
                    targetArray[i] = this.wrapObservableObjectOrArray(deserialized);
                }
            }
        }
    };
    DotvvmSerialization.prototype.deserializeObject = function (viewModel, target, deserializeAll) {
        var unwrappedTarget = ko.unwrap(target);
        if (this.isPrimitive(unwrappedTarget)) {
            unwrappedTarget = {};
        }
        for (var _i = 0, _a = Object.getOwnPropertyNames(viewModel); _i < _a.length; _i++) {
            var prop = _a[_i];
            if (this.isOptionsProperty(prop)) {
                continue;
            }
            var value = viewModel[prop];
            if (typeof (value) == "undefined") {
                continue;
            }
            if (!ko.isObservable(value) && typeof (value) === "function") {
                continue;
            }
            var options = viewModel[prop + "$options"] || (unwrappedTarget && unwrappedTarget[prop + "$options"]);
            if (!deserializeAll && options && options.doNotUpdate) {
                continue;
            }
            this.copyProperty(value, unwrappedTarget, prop, deserializeAll, options);
        }
        // copy the property options metadata
        for (var _b = 0, _c = Object.getOwnPropertyNames(viewModel); _b < _c.length; _b++) {
            var prop = _c[_b];
            if (!this.isOptionsProperty(prop)) {
                continue;
            }
            this.copyPropertyMetadata(unwrappedTarget, prop, viewModel);
        }
        if (ko.isObservable(target)) {
            //This is so that if we have already updated the instance inside target observable
            //there's no need to force update 
            if (unwrappedTarget !== target()) {
                target(unwrappedTarget);
            }
        }
        else {
            target = unwrappedTarget;
        }
        return target;
    };
    DotvvmSerialization.prototype.copyProperty = function (value, unwrappedTarget, prop, deserializeAll, options) {
        var deserialized = this.deserialize(ko.unwrap(value), unwrappedTarget[prop], deserializeAll);
        if (value instanceof Date) {
            // if we get Date value from API, it was converted to string, but we should note that it was date to convert it back
            unwrappedTarget[prop + "$options"] = __assign({}, unwrappedTarget[prop + "$options"], { isDate: true });
        }
        // update the property
        if (ko.isObservable(deserialized)) { //deserialized is observable <=> its input target is observable
            if (deserialized() !== unwrappedTarget[prop]()) {
                unwrappedTarget[prop] = this.extendToObservableArrayIfRequired(unwrappedTarget[prop]);
                unwrappedTarget[prop](deserialized());
            }
        }
        else {
            unwrappedTarget[prop] = this.wrapObservableObjectOrArray(deserialized);
        }
        if (options && options.clientExtenders && ko.isObservable(unwrappedTarget[prop])) {
            for (var j = 0; j < options.clientExtenders.length; j++) {
                var extenderOptions = {};
                var extenderInfo = options.clientExtenders[j];
                extenderOptions[extenderInfo.name] = extenderInfo.parameter;
                unwrappedTarget[prop].extend(extenderOptions);
            }
        }
    };
    DotvvmSerialization.prototype.copyPropertyMetadata = function (unwrappedTarget, prop, viewModel) {
        unwrappedTarget[prop] = __assign({}, unwrappedTarget[prop], viewModel[prop]);
        var originalName = prop.substring(0, prop.length - "$options".length);
        if (typeof unwrappedTarget[originalName] === "undefined") {
            unwrappedTarget[originalName] = ko.observable();
        }
    };
    DotvvmSerialization.prototype.extendToObservableArrayIfRequired = function (observable) {
        if (!ko.isObservable(observable)) {
            throw new Error("Trying to extend a non-observable to an observable array.");
        }
        if (!this.isObservableArray(observable)) {
            ko.utils.extend(observable, ko.observableArray['fn']);
            observable = observable.extend({ 'trackArrayChanges': true });
        }
        return observable;
    };
    DotvvmSerialization.prototype.wrapObservableObjectOrArray = function (obj) {
        return Array.isArray(obj)
            ? ko.observableArray(obj)
            : ko.observable(obj);
    };
    DotvvmSerialization.prototype.isPrimitive = function (viewModel) {
        return viewModel == null
            || typeof (viewModel) == "string"
            || typeof (viewModel) == "number"
            || typeof (viewModel) == "boolean";
    };
    DotvvmSerialization.prototype.isOptionsProperty = function (prop) {
        return /\$options$/.test(prop);
    };
    DotvvmSerialization.prototype.isObservableArray = function (target) {
        return ko.isObservable(target) && "removeAll" in target;
    };
    DotvvmSerialization.prototype.serialize = function (viewModel, opt) {
        if (opt === void 0) { opt = {}; }
        opt = ko.utils.extend({}, opt);
        if (opt.pathOnly && opt.path && opt.path.length === 0)
            opt.pathOnly = false;
        if (viewModel == null) {
            return null;
        }
        if (typeof (viewModel) === "string" || typeof (viewModel) === "number" || typeof (viewModel) === "boolean") {
            return viewModel;
        }
        if (ko.isObservable(viewModel)) {
            return this.serialize(viewModel(), opt);
        }
        if (typeof (viewModel) === "function") {
            return null;
        }
        if (viewModel instanceof Array) {
            if (opt.pathOnly && opt.path) {
                var index = parseInt(opt.path.pop());
                var array = new Array(index + 1);
                array[index] = this.serialize(viewModel[index], opt);
                opt.path.push(index.toString());
                return array;
            }
            else {
                var array = [];
                for (var i = 0; i < viewModel.length; i++) {
                    array.push(this.serialize(viewModel[i], opt));
                }
                return array;
            }
        }
        if (viewModel instanceof Date) {
            if (opt.restApiTarget) {
                return viewModel;
            }
            else {
                return this.serializeDate(viewModel);
            }
        }
        var pathProp = opt.path && opt.path.pop();
        var result = {};
        for (var prop in viewModel) {
            if (viewModel.hasOwnProperty(prop)) {
                if (opt.pathOnly && prop !== pathProp) {
                    continue;
                }
                var value = viewModel[prop];
                if (opt.ignoreSpecialProperties && prop[0] === "$")
                    continue;
                if (!opt.serializeAll && (/\$options$/.test(prop) || prop === "$validationErrors")) {
                    continue;
                }
                if (typeof (value) === "undefined") {
                    continue;
                }
                if (!ko.isObservable(value) && typeof (value) === "function") {
                    continue;
                }
                var options = viewModel[prop + "$options"];
                if (!opt.serializeAll && options && options.doNotPost) {
                    // continue
                }
                else if (opt.oneLevel) {
                    result[prop] = ko.unwrap(value);
                }
                else if (!opt.serializeAll && options && options.pathOnly && opt.pathMatcher) {
                    var path = options.pathOnly;
                    if (!(path instanceof Array)) {
                        path = opt.path || this.findObject(value, opt.pathMatcher);
                    }
                    if (path) {
                        if (path.length === 0) {
                            result[prop] = this.serialize(value, opt);
                        }
                        else {
                            result[prop] = this.serialize(value, { ignoreSpecialProperties: opt.ignoreSpecialProperties, serializeAll: opt.serializeAll, path: path, pathOnly: true });
                        }
                    }
                }
                else {
                    result[prop] = this.serialize(value, opt);
                }
                if (options && options.type && !this.validateType(result[prop], options.type)) {
                    delete result[prop];
                    options.wasInvalid = true;
                }
            }
        }
        if (pathProp && opt.path)
            opt.path.push(pathProp);
        return result;
    };
    DotvvmSerialization.prototype.validateType = function (value, type) {
        var nullable = type[type.length - 1] === "?";
        if (nullable) {
            type = type.substr(0, type.length - 1);
        }
        if (nullable && (value == null || value == "")) {
            return true;
        }
        if (!nullable && (value === null || typeof value === "undefined")) {
            return false;
        }
        var intmatch = /(u?)int(\d*)/.exec(type);
        if (intmatch) {
            if (!/^-?\d*$/.test(value))
                return false;
            var unsigned = intmatch[1] === "u";
            var bits = parseInt(intmatch[2]);
            var minValue = 0;
            var maxValue = Math.pow(2, bits) - 1;
            if (!unsigned) {
                minValue = -Math.floor(maxValue / 2);
                maxValue = maxValue + minValue;
            }
            var int = parseInt(value);
            return int >= minValue && int <= maxValue && int === parseFloat(value);
        }
        if (type === "number" || type === "single" || type === "double" || type === "decimal") {
            // should check if the value is numeric or number in a string
            return +value === value || (!isNaN(+value) && typeof value === "string");
        }
        return true;
    };
    DotvvmSerialization.prototype.findObject = function (obj, matcher) {
        if (matcher(obj))
            return [];
        obj = ko.unwrap(obj);
        if (matcher(obj))
            return [];
        if (typeof obj != "object")
            return null;
        for (var p in obj) {
            if (obj.hasOwnProperty(p)) {
                var match = this.findObject(obj[p], matcher);
                if (match) {
                    match.push(p);
                    return match;
                }
            }
        }
        return null;
    };
    DotvvmSerialization.prototype.flatSerialize = function (viewModel) {
        return this.serialize(viewModel, { ignoreSpecialProperties: true, oneLevel: true, serializeAll: true });
    };
    DotvvmSerialization.prototype.getPureObject = function (viewModel) {
        viewModel = ko.unwrap(viewModel);
        if (viewModel instanceof Array)
            return viewModel.map(this.getPureObject.bind(this));
        var result = {};
        for (var prop in viewModel) {
            if (prop[0] != '$')
                result[prop] = viewModel[prop];
        }
        return result;
    };
    DotvvmSerialization.prototype.pad = function (value, digits) {
        while (value.length < digits) {
            value = "0" + value;
        }
        return value;
    };
    DotvvmSerialization.prototype.serializeDate = function (date, convertToUtc) {
        if (convertToUtc === void 0) { convertToUtc = true; }
        if (date == null) {
            return null;
        }
        else if (typeof date == "string") {
            // just print in the console if it's invalid
            if (dotvvm.globalize.parseDotvvmDate(date) == null)
                console.error(new Error("Date " + date + " is invalid."));
            return date;
        }
        var date2 = new Date(date.getTime());
        if (convertToUtc) {
            date2.setMinutes(date.getMinutes() + date.getTimezoneOffset());
        }
        else {
            date2 = date;
        }
        var y = this.pad(date2.getFullYear().toString(), 4);
        var m = this.pad((date2.getMonth() + 1).toString(), 2);
        var d = this.pad(date2.getDate().toString(), 2);
        var h = this.pad(date2.getHours().toString(), 2);
        var mi = this.pad(date2.getMinutes().toString(), 2);
        var s = this.pad(date2.getSeconds().toString(), 2);
        var ms = this.pad(date2.getMilliseconds().toString(), 3);
        return y + "-" + m + "-" + d + "T" + h + ":" + mi + ":" + s + "." + ms + "0000";
    };
    return DotvvmSerialization;
}());
/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout/knockout.dotvvm.d.ts" />
/// <reference path="typings/globalize/globalize.d.ts" />
document.getElementByDotvvmId = function (id) {
    return document.querySelector("[data-dotvvm-id='" + id + "']");
};
var DotVVM = /** @class */ (function () {
    function DotVVM() {
        var _this = this;
        this.postBackCounter = 0;
        this.lastStartedPostack = 0;
        // when we perform redirect, we also disable all new postbacks to prevent strange behavior
        this.arePostbacksDisabled = false;
        this.resourceSigns = {};
        this.isViewModelUpdating = true;
        this.spaHistory = new DotvvmSpaHistory();
        // warning this property is referenced in ModelState.cs and KnockoutHelper.cs
        this.viewModelObservables = {};
        this.isSpaReady = ko.observable(false);
        this.viewModels = {};
        this.serialization = new DotvvmSerialization();
        this.postbackHandlers = {
            confirm: function (options) { return new ConfirmPostBackHandler(options.message); },
            suppress: function (options) { return new SuppressPostBackHandler(options.suppress); },
            timeout: function (options) { return options.time ? _this.createWindowSetTimeoutHandler(options.time) : _this.windowSetTimeoutHandler; },
            "concurrency-default": function (o) { return ({
                name: "concurrency-default",
                before: ["setIsPostbackRunning"],
                execute: function (callback, options) {
                    return _this.commonConcurrencyHandler(callback(), options, o.q || "default");
                }
            }); },
            "concurrency-deny": function (o) { return ({
                name: "concurrency-deny",
                before: ["setIsPostbackRunning"],
                execute: function (callback, options) {
                    var queue = o.q || "default";
                    if (dotvvm.getPostbackQueue(queue).noRunning > 0)
                        return Promise.reject({ type: "handler", handler: this, message: "An postback is already running" });
                    return dotvvm.commonConcurrencyHandler(callback(), options, queue);
                }
            }); },
            "concurrency-queue": function (o) { return ({
                name: "concurrency-queue",
                before: ["setIsPostbackRunning"],
                execute: function (callback, options) {
                    var queue = o.q || "default";
                    var handler = function () { return dotvvm.commonConcurrencyHandler(callback(), options, queue); };
                    if (dotvvm.getPostbackQueue(queue).noRunning > 0) {
                        return new Promise(function (resolve) {
                            dotvvm.getPostbackQueue(queue).queue.push(function () { return resolve(handler()); });
                        });
                    }
                    return handler();
                }
            }); },
            "suppressOnUpdating": function (options) { return ({
                name: "suppressOnUpdating",
                before: ["setIsPostbackRunning", "concurrency-default", "concurrency-queue", "concurrency-deny"],
                execute: function (callback, options) {
                    if (dotvvm.isViewModelUpdating)
                        return Promise.reject({ type: "handler", handler: this, message: "ViewModel is updating, so it's probably false onchange event" });
                    else
                        return callback();
                }
            }); }
        };
        this.suppressOnDisabledElementHandler = {
            name: "suppressOnDisabledElement",
            before: ["setIsPostbackRunning", "concurrency-default", "concurrency-queue", "concurrency-deny"],
            execute: function (callback, options) {
                if (options.sender && dotvvm.isPostBackProhibited(options.sender)) {
                    return Promise.reject({ type: "handler", handler: _this, message: "PostBack is prohibited on disabled element" });
                }
                else
                    return callback();
            }
        };
        this.beforePostbackEventPostbackHandler = {
            execute: function (callback, options) {
                // trigger beforePostback event
                var beforePostbackArgs = new DotvvmBeforePostBackEventArgs(options.sender, options.viewModel, options.viewModelName, options.postbackId);
                _this.events.beforePostback.trigger(beforePostbackArgs);
                if (beforePostbackArgs.cancel) {
                    return Promise.reject({ type: "event", options: options });
                }
                return callback();
            }
        };
        this.isPostBackRunningHandler = (function () {
            var postbackCount = 0;
            return {
                name: "setIsPostbackRunning",
                before: ["eventInvoke-postbackHandlersStarted"],
                execute: function (callback, options) {
                    _this.isPostbackRunning(true);
                    postbackCount++;
                    var promise = callback();
                    promise.then(function () { return _this.isPostbackRunning(!!--postbackCount); }, function () { return _this.isPostbackRunning(!!--postbackCount); });
                    return promise;
                }
            };
        })();
        this.windowSetTimeoutHandler = this.createWindowSetTimeoutHandler(0);
        this.commonConcurrencyHandler = function (promise, options, queueName) {
            var queue = _this.getPostbackQueue(queueName);
            queue.noRunning++;
            dotvvm.updateProgressChangeCounter(dotvvm.updateProgressChangeCounter() + 1);
            var dispatchNext = function (args) {
                var drop = function () {
                    // run the next postback after everything about this one is finished (after, error events, ...)
                    Promise.resolve().then(function () {
                        queue.noRunning--;
                        dotvvm.updateProgressChangeCounter(dotvvm.updateProgressChangeCounter() - 1);
                        if (queue.queue.length > 0) {
                            var callback = queue.queue.shift();
                            callback();
                        }
                    });
                };
                if (args instanceof DotvvmAfterPostBackWithRedirectEventArgs && args.redirectPromise) {
                    args.redirectPromise.then(drop, drop);
                }
                else {
                    drop();
                }
            };
            return promise.then(function (result) {
                var p = _this.lastStartedPostack == options.postbackId ?
                    result :
                    function () { return Promise.reject(null); };
                return function () {
                    var pr = p();
                    pr.then(dispatchNext, dispatchNext);
                    return pr;
                };
            }, function (error) {
                dispatchNext(error);
                return Promise.reject(error);
            });
        };
        this.defaultConcurrencyPostbackHandler = this.postbackHandlers["concurrency-default"]({});
        this.postbackQueues = {};
        this.postbackHandlersStartedEventHandler = {
            name: "eventInvoke-postbackHandlersStarted",
            execute: function (callback, options) {
                dotvvm.events.postbackHandlersStarted.trigger(options);
                return callback();
            }
        };
        this.postbackHandlersCompletedEventHandler = {
            name: "eventInvoke-postbackHandlersCompleted",
            after: ["eventInvoke-postbackHandlersStarted"],
            execute: function (callback, options) {
                dotvvm.events.postbackHandlersCompleted.trigger(options);
                return callback();
            }
        };
        this.globalPostbackHandlers = [this.suppressOnDisabledElementHandler, this.isPostBackRunningHandler, this.postbackHandlersStartedEventHandler];
        this.globalLaterPostbackHandlers = [this.postbackHandlersCompletedEventHandler, this.beforePostbackEventPostbackHandler];
        this.events = new DotvvmEvents();
        this.globalize = new DotvvmGlobalize();
        this.evaluator = new DotvvmEvaluator();
        this.domUtils = new DotvvmDomUtils();
        this.fileUpload = new DotvvmFileUpload();
        this.extensions = {};
        this.isPostbackRunning = ko.observable(false);
        this.isSpaNavigationRunning = ko.observable(false);
        this.updateProgressChangeCounter = ko.observable(0);
        this.diffEqual = {};
    }
    DotVVM.prototype.createWindowSetTimeoutHandler = function (time) {
        return {
            name: "timeout",
            before: ["eventInvoke-postbackHandlersStarted", "setIsPostbackRunning"],
            execute: function (callback, options) {
                return new Promise(function (resolve, reject) { return window.setTimeout(resolve, time); })
                    .then(function () { return callback(); });
            }
        };
    };
    DotVVM.prototype.getPostbackQueue = function (name) {
        if (name === void 0) { name = "default"; }
        if (!this.postbackQueues[name])
            this.postbackQueues[name] = { queue: [], noRunning: 0 };
        return this.postbackQueues[name];
    };
    DotVVM.prototype.init = function (viewModelName, culture) {
        var _this = this;
        this.addKnockoutBindingHandlers();
        // load the viewmodel
        var thisViewModel = this.viewModels[viewModelName] = JSON.parse(document.getElementById("__dot_viewmodel_" + viewModelName).value);
        if (thisViewModel.resources) {
            for (var r in thisViewModel.resources) {
                this.resourceSigns[r] = true;
            }
        }
        if (thisViewModel.renderedResources) {
            thisViewModel.renderedResources.forEach(function (r) { return _this.resourceSigns[r] = true; });
        }
        var idFragment = thisViewModel.resultIdFragment;
        // store server-side cached viewmodel
        if (thisViewModel.viewModelCacheId) {
            thisViewModel.viewModelCache = this.viewModels[viewModelName].viewModel;
        }
        var viewModel = thisViewModel.viewModel = this.serialization.deserialize(this.viewModels[viewModelName].viewModel, {}, true);
        // initialize services
        this.culture = culture;
        this.validation = new DotvvmValidation(this);
        // wrap it in the observable
        this.viewModelObservables[viewModelName] = ko.observable(viewModel);
        ko.applyBindings(this.viewModelObservables[viewModelName], document.documentElement);
        // trigger the init event
        this.events.init.trigger({ viewModel: viewModel });
        // handle SPA requests
        var spaPlaceHolder = this.getSpaPlaceHolder();
        if (spaPlaceHolder != null) {
            var hashChangeHandler = function (initialLoad) { return _this.handleHashChange(viewModelName, spaPlaceHolder, initialLoad); };
            this.useHistoryApiSpaNavigation = JSON.parse(spaPlaceHolder.getAttribute("data-dotvvm-spacontentplaceholder-usehistoryapi"));
            if (this.useHistoryApiSpaNavigation) {
                hashChangeHandler = function (initialLoad) { return _this.handleHashChangeWithHistory(viewModelName, spaPlaceHolder, initialLoad); };
            }
            var spaChangedHandler = function () { return hashChangeHandler(false); };
            this.domUtils.attachEvent(window, "hashchange", spaChangedHandler);
            hashChangeHandler(true);
        }
        window.addEventListener('popstate', function (event) { return _this.handlePopState(viewModelName, event, spaPlaceHolder != null); });
        this.isViewModelUpdating = false;
        if (idFragment) {
            if (spaPlaceHolder) {
                var element = document.getElementById(idFragment);
                if (element && "function" == typeof element.scrollIntoView)
                    element.scrollIntoView(true);
            }
            else
                location.hash = idFragment;
        }
        // persist the viewmodel in the hidden field so the Back button will work correctly
        this.domUtils.attachEvent(window, "beforeunload", function (e) {
            _this.persistViewModel(viewModelName);
        });
    };
    DotVVM.prototype.handlePopState = function (viewModelName, event, inSpaPage) {
        if (this.spaHistory.isSpaPage(event.state)) {
            var historyRecord = this.spaHistory.getHistoryRecord(event.state);
            if (inSpaPage)
                this.navigateCore(viewModelName, historyRecord.url);
            else
                this.performRedirect(historyRecord.url, true);
            event.preventDefault();
        }
    };
    DotVVM.prototype.handleHashChangeWithHistory = function (viewModelName, spaPlaceHolder, isInitialPageLoad) {
        var _this = this;
        if (document.location.hash.indexOf("#!/") === 0) {
            // the user requested navigation to another SPA page
            this.navigateCore(viewModelName, document.location.hash.substring(2), function (url) { _this.spaHistory.replacePage(url); });
        }
        else {
            var defaultUrl = spaPlaceHolder.getAttribute("data-dotvvm-spacontentplaceholder-defaultroute");
            var containsContent = spaPlaceHolder.hasAttribute("data-dotvvm-spacontentplaceholder-content");
            if (!containsContent && defaultUrl) {
                this.navigateCore(viewModelName, "/" + defaultUrl, function (url) { return _this.spaHistory.replacePage(url); });
            }
            else {
                this.isSpaReady(true);
                spaPlaceHolder.style.display = "";
                var currentRelativeUrl = location.pathname + location.search + location.hash;
                this.spaHistory.replacePage(currentRelativeUrl);
            }
        }
    };
    DotVVM.prototype.handleHashChange = function (viewModelName, spaPlaceHolder, isInitialPageLoad) {
        if (document.location.hash.indexOf("#!/") === 0) {
            // the user requested navigation to another SPA page
            this.navigateCore(viewModelName, document.location.hash.substring(2));
        }
        else {
            var url = spaPlaceHolder.getAttribute("data-dotvvm-spacontentplaceholder-defaultroute");
            if (url) {
                // perform redirect to default page
                url = "#!/" + url;
                url = this.fixSpaUrlPrefix(url);
                this.performRedirect(url, isInitialPageLoad);
            }
            else if (!isInitialPageLoad) {
                // get startup URL and redirect there
                url = document.location.toString();
                var slashIndex = url.indexOf('/', 'https://'.length);
                if (slashIndex > 0) {
                    url = url.substring(slashIndex);
                }
                else {
                    url = "/";
                }
                this.navigateCore(viewModelName, url);
            }
            else {
                // the page was loaded for the first time
                this.isSpaReady(true);
                spaPlaceHolder.style.display = "";
            }
        }
    };
    DotVVM.prototype.persistViewModel = function (viewModelName) {
        var viewModel = this.viewModels[viewModelName];
        var persistedViewModel = {};
        for (var p in viewModel) {
            if (viewModel.hasOwnProperty(p)) {
                persistedViewModel[p] = viewModel[p];
            }
        }
        persistedViewModel["viewModel"] = this.serialization.serialize(persistedViewModel["viewModel"], { serializeAll: true });
        document.getElementById("__dot_viewmodel_" + viewModelName).value = JSON.stringify(persistedViewModel);
    };
    DotVVM.prototype.backUpPostBackConter = function () {
        return ++this.postBackCounter;
    };
    DotVVM.prototype.isPostBackStillActive = function (currentPostBackCounter) {
        return this.postBackCounter === currentPostBackCounter;
    };
    DotVVM.prototype.fetchCsrfToken = function (viewModelName) {
        return __awaiter(this, void 0, void 0, function () {
            var vm, response, _a;
            return __generator(this, function (_b) {
                switch (_b.label) {
                    case 0:
                        vm = this.viewModels[viewModelName].viewModel;
                        if (!(vm.$csrfToken == null)) return [3 /*break*/, 3];
                        return [4 /*yield*/, fetch((this.viewModels[viewModelName].virtualDirectory || "") + "/___dotvvm-create-csrf-token___")];
                    case 1:
                        response = _b.sent();
                        if (response.status != 200)
                            throw new Error("Can't fetch CSRF token: " + response.statusText);
                        _a = vm;
                        return [4 /*yield*/, response.text()];
                    case 2:
                        _a.$csrfToken = _b.sent();
                        _b.label = 3;
                    case 3: return [2 /*return*/, vm.$csrfToken];
                }
            });
        });
    };
    DotVVM.prototype.staticCommandPostback = function (viewModelName, sender, command, args, callback, errorCallback) {
        var _this = this;
        if (callback === void 0) { callback = function (_) { }; }
        if (errorCallback === void 0) { errorCallback = function (errorInfo) { }; }
        (function () { return __awaiter(_this, void 0, void 0, function () {
            var csrfToken, err_1, data;
            var _this = this;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 2, , 3]);
                        return [4 /*yield*/, this.fetchCsrfToken(viewModelName)];
                    case 1:
                        csrfToken = _a.sent();
                        return [3 /*break*/, 3];
                    case 2:
                        err_1 = _a.sent();
                        this.events.error.trigger(new DotvvmErrorEventArgs(sender, this.viewModels[viewModelName].viewModel, viewModelName, null, null));
                        console.warn("CSRF token fetch failed.");
                        errorCallback({ error: err_1 });
                        return [2 /*return*/];
                    case 3:
                        data = this.serialization.serialize({
                            args: args,
                            command: command,
                            "$csrfToken": csrfToken
                        });
                        dotvvm.events.staticCommandMethodInvoking.trigger(data);
                        this.postJSON(this.viewModels[viewModelName].url, "POST", ko.toJSON(data), function (response) {
                            try {
                                _this.isViewModelUpdating = true;
                                var responseObj = JSON.parse(response.responseText);
                                if ("action" in responseObj) {
                                    if (responseObj.action == "redirect") {
                                        // redirect
                                        _this.handleRedirect(responseObj, viewModelName);
                                        errorCallback({ xhr: response, error: "redirect" });
                                        return;
                                    }
                                    else {
                                        throw new Error("Invalid action " + responseObj.action);
                                    }
                                }
                                var result = responseObj.result;
                                dotvvm.events.staticCommandMethodInvoked.trigger(__assign({}, data, { result: result, xhr: response }));
                                callback(result);
                            }
                            catch (error) {
                                dotvvm.events.staticCommandMethodFailed.trigger(__assign({}, data, { xhr: response, error: error }));
                                errorCallback({ xhr: response, error: error });
                            }
                            finally {
                                _this.isViewModelUpdating = false;
                            }
                        }, function (xhr) {
                            if (/^application\/json(;|$)/.test(xhr.getResponseHeader("Content-Type"))) {
                                var errObject = JSON.parse(xhr.responseText);
                                if (errObject.action === "invalidCsrfToken") {
                                    // ok, renew the token and try again. Do that before any event is triggered
                                    _this.viewModels[viewModelName].viewModel.$csrfToken = null;
                                    console.log("Resending postback due to invalid CSRF token."); // this may loop indefinitely (in some extreme case), we don't currently have any loop detection mechanism, so at least we can log it.
                                    _this.staticCommandPostback(viewModelName, sender, command, args, callback, errorCallback);
                                    return;
                                }
                            }
                            _this.events.error.trigger(new DotvvmErrorEventArgs(sender, _this.viewModels[viewModelName].viewModel, viewModelName, xhr, null));
                            console.warn("StaticCommand postback failed: " + xhr.status + " - " + xhr.statusText, xhr);
                            errorCallback({ xhr: xhr });
                            dotvvm.events.staticCommandMethodFailed.trigger(__assign({}, data, { xhr: xhr }));
                        }, function (xhr) {
                            xhr.setRequestHeader("X-PostbackType", "StaticCommand");
                        });
                        return [2 /*return*/];
                }
            });
        }); })();
    };
    DotVVM.prototype.processPassedId = function (id, context) {
        if (typeof id == "string" || id == null)
            return id;
        if (typeof id == "object" && id.expr)
            return this.evaluator.evaluateOnViewModel(context, id.expr);
        throw new Error("invalid argument");
    };
    DotVVM.prototype.getPostbackHandler = function (name) {
        var handler = this.postbackHandlers[name];
        if (handler) {
            return handler;
        }
        else {
            throw new Error("Could not find postback handler of name '" + name + "'");
        }
    };
    DotVVM.prototype.isPostbackHandler = function (obj) {
        return obj && typeof obj.execute == "function";
    };
    DotVVM.prototype.findPostbackHandlers = function (knockoutContext, config) {
        var _this = this;
        var createHandler = function (name, options) { return options.enabled === false ? null : _this.getPostbackHandler(name)(options); };
        return config.map(function (h) {
            return typeof h == 'string' ? createHandler(h, {}) :
                _this.isPostbackHandler(h) ? h :
                    h instanceof Array ? (function () {
                        var name = h[0], opt = h[1];
                        return createHandler(name, typeof opt == "function" ? opt(knockoutContext, knockoutContext.$data) : opt);
                    })() :
                        createHandler(h.name, h.options && h.options(knockoutContext));
        })
            .filter(function (h) { return h != null; });
    };
    DotVVM.prototype.sortHandlers = function (handlers) {
        var getHandler = (function () {
            var handlerMap = {};
            for (var _i = 0, handlers_2 = handlers; _i < handlers_2.length; _i++) {
                var h = handlers_2[_i];
                if (h.name != null) {
                    handlerMap[h.name] = h;
                }
            }
            return function (s) { return typeof s == "string" ? handlerMap[s] : s; };
        })();
        var dependencies = handlers.map(function (handler, i) { return (handler["@sort_index"] = i, ({ handler: handler, deps: (handler.after || []).map(getHandler) })); });
        for (var _i = 0, handlers_1 = handlers; _i < handlers_1.length; _i++) {
            var h = handlers_1[_i];
            if (h.before)
                for (var _a = 0, _b = h.before.map(getHandler); _a < _b.length; _a++) {
                    var before = _b[_a];
                    if (before) {
                        var index = before["@sort_index"];
                        dependencies[index].deps.push(h);
                    }
                }
        }
        var result = [];
        var doneBitmap = new Uint8Array(dependencies.length);
        var addToResult = function (index) {
            switch (doneBitmap[index]) {
                case 0: break;
                case 1: throw new Error("Cyclic PostbackHandler dependency found.");
                case 2: return; // it's already in the list
                default: throw new Error("");
            }
            if (doneBitmap[index] == 1)
                return;
            doneBitmap[index] = 1;
            var _a = dependencies[index], handler = _a.handler, deps = _a.deps;
            for (var _i = 0, deps_1 = deps; _i < deps_1.length; _i++) {
                var d = deps_1[_i];
                addToResult(d["@sort_index"]);
            }
            doneBitmap[index] = 2;
            result.push(handler);
        };
        for (var i = 0; i < dependencies.length; i++) {
            addToResult(i);
        }
        return result;
    };
    DotVVM.prototype.applyPostbackHandlersCore = function (callback, options, handlers) {
        var processResult = function (t) { return typeof t == "function" ? t : (function () { return Promise.resolve(new DotvvmAfterPostBackEventArgs(options, null, t)); }); };
        if (handlers == null || handlers.length === 0) {
            return callback(options).then(processResult, function (r) { return Promise.reject(r); });
        }
        else {
            var sortedHandlers = this.sortHandlers(handlers);
            return sortedHandlers
                .reduceRight(function (prev, val, index) { return function () {
                return val.execute(prev, options);
            }; }, function () { return callback(options).then(processResult, function (r) { return Promise.reject(r); }); })();
        }
    };
    DotVVM.prototype.applyPostbackHandlers = function (callback, sender, handlers, args, context, viewModel, viewModelName) {
        if (args === void 0) { args = []; }
        if (context === void 0) { context = ko.contextFor(sender); }
        if (viewModel === void 0) { viewModel = context.$root; }
        if (dotvvm.isSpaNavigationRunning())
            return Promise.reject({ type: "handler" });
        var options = new PostbackOptions(this.backUpPostBackConter(), sender, args, viewModel, viewModelName);
        var promise = this.applyPostbackHandlersCore(callback, options, this.findPostbackHandlers(context, this.globalPostbackHandlers.concat(handlers || []).concat(this.globalLaterPostbackHandlers)))
            .then(function (r) { return r(); }, function (r) { return Promise.reject(r); });
        promise.catch(function (reason) { if (reason)
            console.log("Promise rejected. ", reason); });
        return promise;
    };
    DotVVM.prototype.postbackCore = function (options, path, command, controlUniqueId, context, commandArgs) {
        var _this = this;
        return new Promise(function (resolve, reject) { return __awaiter(_this, void 0, void 0, function () {
            var viewModelName, viewModel, err_2, data, completeViewModel, errorAction;
            var _this = this;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        viewModelName = options.viewModelName;
                        viewModel = this.viewModels[viewModelName].viewModel;
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 3, , 4]);
                        return [4 /*yield*/, this.fetchCsrfToken(viewModelName)];
                    case 2:
                        _a.sent();
                        return [3 /*break*/, 4];
                    case 3:
                        err_2 = _a.sent();
                        reject({ type: 'network', options: options, args: new DotvvmErrorEventArgs(options.sender, viewModel, viewModelName, null, options.postbackId) });
                        return [2 /*return*/];
                    case 4:
                        if (this.arePostbacksDisabled) {
                            reject({ type: 'handler' });
                        }
                        this.lastStartedPostack = options.postbackId;
                        // perform the postback
                        this.updateDynamicPathFragments(context, path);
                        data = {
                            currentPath: path,
                            command: command,
                            controlUniqueId: this.processPassedId(controlUniqueId, context),
                            additionalData: options.additionalPostbackData,
                            renderedResources: this.viewModels[viewModelName].renderedResources,
                            commandArgs: commandArgs
                        };
                        completeViewModel = this.serialization.serialize(viewModel, { pathMatcher: function (val) { return context && val == context.$data; } });
                        // if the viewmodel is cached on the server, send only the diff
                        if (this.viewModels[viewModelName].viewModelCache) {
                            data.viewModelDiff = this.diff(this.viewModels[viewModelName].viewModelCache, completeViewModel);
                            data.viewModelCacheId = this.viewModels[viewModelName].viewModelCacheId;
                        }
                        else {
                            data.viewModel = completeViewModel;
                        }
                        errorAction = function (xhr) {
                            if (/^application\/json(;|$)/.test(xhr.getResponseHeader("Content-Type"))) {
                                var errObject = JSON.parse(xhr.responseText);
                                if (errObject.action === "invalidCsrfToken") {
                                    // ok, renew the token and try again. Do that before any event is triggered
                                    _this.viewModels[viewModelName].viewModel.$csrfToken = null;
                                    console.log("Resending postback due to invalid CSRF token."); // this may loop indefinitely (in some extreme case), we don't currently have any loop detection mechanism, so at least we can log it.
                                    _this.postbackCore(options, path, command, controlUniqueId, context, commandArgs).then(resolve, reject);
                                    return;
                                }
                            }
                            reject({ type: 'network', options: options, args: new DotvvmErrorEventArgs(options.sender, viewModel, viewModelName, xhr, options.postbackId) });
                        };
                        this.postJSON(this.viewModels[viewModelName].url, "POST", ko.toJSON(data), function (result) {
                            var resultObject = {};
                            var successAction = function (actualResult) {
                                dotvvm.events.postbackResponseReceived.trigger({});
                                resolve(function () { return new Promise(function (resolve, reject) {
                                    dotvvm.events.postbackCommitInvoked.trigger({});
                                    if (!resultObject.viewModel && resultObject.viewModelDiff) {
                                        // TODO: patch (~deserialize) it to ko.observable viewModel
                                        resultObject.viewModel = _this.patch(completeViewModel, resultObject.viewModelDiff);
                                    }
                                    _this.loadResourceList(resultObject.resources, function () {
                                        var isSuccess = false;
                                        if (resultObject.action === "successfulCommand") {
                                            try {
                                                _this.isViewModelUpdating = true;
                                                // store server-side cached viewmodel
                                                if (resultObject.viewModelCacheId) {
                                                    _this.viewModels[viewModelName].viewModelCacheId = resultObject.viewModelCacheId;
                                                    _this.viewModels[viewModelName].viewModelCache = resultObject.viewModel;
                                                }
                                                else {
                                                    delete _this.viewModels[viewModelName].viewModelCacheId;
                                                    delete _this.viewModels[viewModelName].viewModelCache;
                                                }
                                                // remove updated controls
                                                var updatedControls = _this.cleanUpdatedControls(resultObject);
                                                // update the viewmodel
                                                if (resultObject.viewModel) {
                                                    ko.delaySync.pause();
                                                    _this.serialization.deserialize(resultObject.viewModel, _this.viewModels[viewModelName].viewModel);
                                                    ko.delaySync.resume();
                                                }
                                                isSuccess = true;
                                                // remove updated controls which were previously hidden
                                                _this.cleanUpdatedControls(resultObject, updatedControls);
                                                // add updated controls
                                                _this.restoreUpdatedControls(resultObject, updatedControls, true);
                                            }
                                            finally {
                                                _this.isViewModelUpdating = false;
                                            }
                                            dotvvm.events.postbackViewModelUpdated.trigger({});
                                        }
                                        else if (resultObject.action === "redirect") {
                                            // redirect
                                            var promise = _this.handleRedirect(resultObject, viewModelName);
                                            var redirectAfterPostBackArgs = new DotvvmAfterPostBackWithRedirectEventArgs(options, resultObject, resultObject.commandResult, result, promise);
                                            resolve(redirectAfterPostBackArgs);
                                            return;
                                        }
                                        var idFragment = resultObject.resultIdFragment;
                                        if (idFragment) {
                                            if (_this.getSpaPlaceHolder() || location.hash == "#" + idFragment) {
                                                var element = document.getElementById(idFragment);
                                                if (element && "function" == typeof element.scrollIntoView)
                                                    element.scrollIntoView(true);
                                            }
                                            else
                                                location.hash = idFragment;
                                        }
                                        // trigger afterPostback event
                                        if (!isSuccess) {
                                            reject(new DotvvmErrorEventArgs(options.sender, viewModel, viewModelName, actualResult, options.postbackId, resultObject));
                                        }
                                        else {
                                            var afterPostBackArgs = new DotvvmAfterPostBackEventArgs(options, resultObject, resultObject.commandResult, result);
                                            resolve(afterPostBackArgs);
                                        }
                                    });
                                }); });
                            };
                            var parseResultObject = function (actualResult) {
                                var locationHeader = actualResult.getResponseHeader("Location");
                                resultObject = locationHeader != null && locationHeader.length > 0 ?
                                    { action: "redirect", url: locationHeader } :
                                    JSON.parse(actualResult.responseText);
                            };
                            parseResultObject(result);
                            if (resultObject.action === "viewModelNotCached") {
                                // repeat request with full viewModel
                                delete _this.viewModels[viewModelName].viewModelCache;
                                delete _this.viewModels[viewModelName].viewModelCacheId;
                                delete data.viewModelDiff;
                                delete data.viewModelCacheId;
                                data.viewModel = completeViewModel;
                                return _this.postJSON(_this.viewModels[viewModelName].url, "POST", ko.toJSON(data), function (result2) {
                                    parseResultObject(result2);
                                    successAction(result2);
                                }, errorAction);
                            }
                            else {
                                // process the response
                                successAction(result);
                            }
                        }, errorAction);
                        return [2 /*return*/];
                }
            });
        }); });
    };
    DotVVM.prototype.handleSpaNavigation = function (element) {
        var target = element.getAttribute('target');
        if (target == "_blank") {
            return Promise.resolve(new DotvvmNavigationEventArgs(this.viewModels.root.viewModel, "root", null));
        }
        return this.handleSpaNavigationCore(element.getAttribute('href'));
    };
    DotVVM.prototype.handleSpaNavigationCore = function (url) {
        var _this = this;
        return new Promise(function (resolve, reject) {
            if (url && url.indexOf("/") === 0) {
                var viewModelName = "root";
                url = _this.removeVirtualDirectoryFromUrl(url, viewModelName);
                _this.navigateCore(viewModelName, url, function (navigatedUrl) {
                    if (!history.state || history.state.url != navigatedUrl) {
                        _this.spaHistory.pushPage(navigatedUrl);
                    }
                }).then(resolve, reject);
            }
            else {
                reject();
            }
        });
    };
    DotVVM.prototype.postBack = function (viewModelName, sender, path, command, controlUniqueId, context, handlers, commandArgs) {
        var _this = this;
        if (dotvvm.isSpaNavigationRunning())
            return Promise.reject({ type: "handler" });
        context = context || ko.contextFor(sender);
        var preparedHandlers = this.findPostbackHandlers(context, this.globalPostbackHandlers.concat(handlers || []).concat(this.globalLaterPostbackHandlers));
        if (preparedHandlers.filter(function (h) { return h.name && h.name.indexOf("concurrency-") == 0; }).length == 0) {
            // add a default concurrency handler if none is specified
            preparedHandlers.push(this.defaultConcurrencyPostbackHandler);
        }
        var options = new PostbackOptions(this.backUpPostBackConter(), sender, commandArgs, context.$data, viewModelName);
        var promise = this.applyPostbackHandlersCore(function (options) {
            return _this.postbackCore(options, path, command, controlUniqueId, context, commandArgs);
        }, options, preparedHandlers);
        var result = promise.then(function (r) { return r().then(function (r) { return r; }, function (error) { return Promise.reject({ type: "commit", args: error }); }); }, function (r) { return Promise.reject(r); });
        result.then(function (r) { return r && _this.events.afterPostback.trigger(r); }, function (error) {
            var afterPostBackArgsCanceled = new DotvvmAfterPostBackEventArgs(options, error.type == "commit" && error.args ? error.args.serverResponseObject : null, options.postbackId);
            if (error.type == "handler" || error.type == "event") {
                // trigger afterPostback event
                afterPostBackArgsCanceled.wasInterrupted = true;
                _this.events.postbackRejected.trigger({});
            }
            else if (error.type == "network") {
                _this.events.error.trigger(error.args);
            }
            _this.events.afterPostback.trigger(afterPostBackArgsCanceled);
        });
        return result;
    };
    DotVVM.prototype.loadResourceList = function (resources, callback) {
        var html = "";
        for (var name in resources) {
            if (!/^__noname_\d+$/.test(name)) {
                if (this.resourceSigns[name])
                    continue;
                this.resourceSigns[name] = true;
            }
            html += resources[name] + " ";
        }
        if (html.trim() === "") {
            setTimeout(callback, 4);
            return;
        }
        else {
            var tmp = document.createElement("div");
            tmp.innerHTML = html;
            var elements = [];
            for (var i = 0; i < tmp.children.length; i++) {
                elements.push(tmp.children.item(i));
            }
            this.loadResourceElements(elements, 0, callback);
        }
    };
    DotVVM.prototype.loadResourceElements = function (elements, offset, callback) {
        var _this = this;
        if (offset >= elements.length) {
            callback();
            return;
        }
        var element = elements[offset];
        var waitForScriptLoaded = false;
        if (element.tagName.toLowerCase() == "script") {
            var originalScript = element;
            var script = document.createElement("script");
            if (originalScript.src) {
                script.src = originalScript.src;
                waitForScriptLoaded = true;
            }
            if (originalScript.type) {
                script.type = originalScript.type;
            }
            if (originalScript.text) {
                script.text = originalScript.text;
            }
            if (element.id) {
                script.id = element.id;
            }
            element = script;
        }
        else if (element.tagName.toLowerCase() == "link") {
            // create link
            var originalLink = element;
            var link = document.createElement("link");
            if (originalLink.href) {
                link.href = originalLink.href;
            }
            if (originalLink.rel) {
                link.rel = originalLink.rel;
            }
            if (originalLink.type) {
                link.type = originalLink.type;
            }
            element = link;
        }
        // load next script when this is finished
        if (waitForScriptLoaded) {
            element.addEventListener("load", function () { return _this.loadResourceElements(elements, offset + 1, callback); });
            element.addEventListener("error", function () { return _this.loadResourceElements(elements, offset + 1, callback); });
        }
        document.head.appendChild(element);
        if (!waitForScriptLoaded) {
            this.loadResourceElements(elements, offset + 1, callback);
        }
    };
    DotVVM.prototype.getSpaPlaceHolder = function () {
        var elements = document.getElementsByName("__dot_SpaContentPlaceHolder");
        if (elements.length == 1) {
            return elements[0];
        }
        return null;
    };
    DotVVM.prototype.navigateCore = function (viewModelName, url, handlePageNavigating) {
        var _this = this;
        return new Promise(function (resolve, reject) {
            var viewModel = _this.viewModels[viewModelName].viewModel;
            // prevent double postbacks
            var currentPostBackCounter = _this.backUpPostBackConter();
            _this.isSpaNavigationRunning(true);
            // trigger spaNavigating event
            var spaNavigatingArgs = new DotvvmSpaNavigatingEventArgs(viewModel, viewModelName, url);
            _this.events.spaNavigating.trigger(spaNavigatingArgs);
            if (spaNavigatingArgs.cancel) {
                return;
            }
            var virtualDirectory = _this.viewModels[viewModelName].virtualDirectory || "";
            // add virtual directory prefix
            var spaUrl = "/___dotvvm-spa___" + _this.addLeadingSlash(url);
            var fullUrl = _this.addLeadingSlash(_this.concatUrl(virtualDirectory, spaUrl));
            // find SPA placeholder
            var spaPlaceHolder = _this.getSpaPlaceHolder();
            if (!spaPlaceHolder) {
                document.location.href = fullUrl;
                return;
            }
            if (handlePageNavigating) {
                handlePageNavigating(_this.addLeadingSlash(_this.concatUrl(virtualDirectory, _this.addLeadingSlash(url))));
            }
            // send the request
            var spaPlaceHolderUniqueId = spaPlaceHolder.attributes["data-dotvvm-spacontentplaceholder"].value;
            _this.getJSON(fullUrl, "GET", spaPlaceHolderUniqueId, function (result) {
                // if another postback has already been passed, don't do anything
                if (!_this.isPostBackStillActive(currentPostBackCounter))
                    return;
                var resultObject = JSON.parse(result.responseText);
                _this.loadResourceList(resultObject.resources, function () {
                    var isSuccess = false;
                    if (resultObject.action === "successfulCommand" || !resultObject.action) {
                        try {
                            _this.isViewModelUpdating = true;
                            // remove updated controls
                            var updatedControls = _this.cleanUpdatedControls(resultObject);
                            // update the viewmodel
                            _this.viewModels[viewModelName] = {};
                            for (var p in resultObject) {
                                if (resultObject.hasOwnProperty(p)) {
                                    _this.viewModels[viewModelName][p] = resultObject[p];
                                }
                            }
                            // store server-side cached viewmodel
                            if (resultObject.viewModelCacheId) {
                                _this.viewModels[viewModelName].viewModelCache = resultObject.viewModel;
                            }
                            else {
                                delete _this.viewModels[viewModelName].viewModelCache;
                            }
                            ko.delaySync.pause();
                            _this.serialization.deserialize(resultObject.viewModel, _this.viewModels[viewModelName].viewModel);
                            ko.delaySync.resume();
                            isSuccess = true;
                            // add updated controls
                            _this.viewModelObservables[viewModelName](_this.viewModels[viewModelName].viewModel);
                            _this.restoreUpdatedControls(resultObject, updatedControls, true);
                            _this.isSpaReady(true);
                        }
                        finally {
                            _this.isViewModelUpdating = false;
                        }
                    }
                    else if (resultObject.action === "redirect") {
                        _this.isSpaNavigationRunning(false);
                        _this.handleRedirect(resultObject, viewModelName, true).then(resolve, reject);
                        return;
                    }
                    // trigger spaNavigated event
                    var spaNavigatedArgs = new DotvvmSpaNavigatedEventArgs(_this.viewModels[viewModelName].viewModel, viewModelName, resultObject, result);
                    _this.events.spaNavigated.trigger(spaNavigatedArgs);
                    _this.isSpaNavigationRunning(false);
                    if (!isSuccess && !spaNavigatedArgs.isHandled) {
                        reject();
                        throw "Invalid response from server!";
                    }
                    resolve(spaNavigatedArgs);
                });
            }, function (xhr) {
                // if another postback has already been passed, don't do anything
                if (!_this.isPostBackStillActive(currentPostBackCounter))
                    return;
                // execute error handlers
                var errArgs = new DotvvmErrorEventArgs(undefined, viewModel, viewModelName, xhr, -1, undefined, true);
                _this.events.error.trigger(errArgs);
                if (!errArgs.handled) {
                    alert(xhr.responseText);
                }
                _this.isSpaNavigationRunning(false);
                reject(errArgs);
            });
        });
    };
    DotVVM.prototype.handleRedirect = function (resultObject, viewModelName, replace) {
        if (replace === void 0) { replace = false; }
        if (resultObject.replace != null)
            replace = resultObject.replace;
        var url;
        // redirect
        if (this.getSpaPlaceHolder() && !this.useHistoryApiSpaNavigation && resultObject.url.indexOf("//") < 0 && resultObject.allowSpa) {
            // relative URL - keep in SPA mode, but remove the virtual directory
            url = "#!" + this.removeVirtualDirectoryFromUrl(resultObject.url, viewModelName);
            if (url === "#!") {
                url = "#!/";
            }
            // verify that the URL prefix is correct, if not - add it before the fragment
            url = this.fixSpaUrlPrefix(url);
        }
        else {
            // absolute URL - load the URL
            url = resultObject.url;
        }
        // trigger redirect event
        var redirectArgs = new DotvvmRedirectEventArgs(dotvvm.viewModels[viewModelName], viewModelName, url, replace);
        this.events.redirect.trigger(redirectArgs);
        return this.performRedirect(url, replace, resultObject.allowSpa && this.useHistoryApiSpaNavigation);
    };
    DotVVM.prototype.disablePostbacks = function () {
        this.lastStartedPostack = -1; // this stops further commits
        for (var q in this.postbackQueues) {
            if (this.postbackQueues.hasOwnProperty(q)) {
                var postbackQueue = this.postbackQueues[q];
                postbackQueue.queue.length = 0;
                postbackQueue.noRunning = 0;
            }
        }
        // disable all other postbacks
        // but not in SPA mode, since we'll need them for the next page
        // and user might want to try another postback in case this navigation hangs
        if (!this.getSpaPlaceHolder()) {
            this.arePostbacksDisabled = true;
        }
    };
    DotVVM.prototype.performRedirect = function (url, replace, useHistoryApiSpaRedirect) {
        var _this = this;
        return new Promise(function (resolve, reject) {
            _this.disablePostbacks();
            if (replace) {
                location.replace(url);
                resolve();
            }
            else if (useHistoryApiSpaRedirect) {
                _this.handleSpaNavigationCore(url).then(resolve, reject);
            }
            else {
                var fakeAnchor = _this.fakeRedirectAnchor;
                if (!fakeAnchor) {
                    fakeAnchor = document.createElement("a");
                    fakeAnchor.style.display = "none";
                    fakeAnchor.setAttribute("data-dotvvm-fake-id", "dotvvm_fake_redirect_anchor_87D7145D_8EA8_47BA_9941_82B75EE88CDB");
                    document.body.appendChild(fakeAnchor);
                    _this.fakeRedirectAnchor = fakeAnchor;
                }
                fakeAnchor.href = url;
                fakeAnchor.click();
                resolve();
            }
        });
    };
    DotVVM.prototype.fixSpaUrlPrefix = function (url) {
        var attr = this.getSpaPlaceHolder().attributes["data-dotvvm-spacontentplaceholder-urlprefix"];
        if (!attr) {
            return url;
        }
        var correctPrefix = attr.value;
        var currentPrefix = document.location.pathname;
        if (correctPrefix !== currentPrefix) {
            if (correctPrefix === "") {
                correctPrefix = "/";
            }
            url = correctPrefix + url;
        }
        return url;
    };
    DotVVM.prototype.removeVirtualDirectoryFromUrl = function (url, viewModelName) {
        var virtualDirectory = "/" + this.viewModels[viewModelName].virtualDirectory;
        if (url.indexOf(virtualDirectory) == 0) {
            return this.addLeadingSlash(url.substring(virtualDirectory.length));
        }
        else {
            return url;
        }
    };
    DotVVM.prototype.addLeadingSlash = function (url) {
        if (url.length > 0 && url.substring(0, 1) != "/") {
            return "/" + url;
        }
        return url;
    };
    DotVVM.prototype.concatUrl = function (url1, url2) {
        if (url1.length > 0 && url1.substring(url1.length - 1) == "/") {
            url1 = url1.substring(0, url1.length - 1);
        }
        return url1 + this.addLeadingSlash(url2);
    };
    DotVVM.prototype.patch = function (source, patch) {
        var _this = this;
        if (source instanceof Array && patch instanceof Array) {
            return patch.map(function (val, i) {
                return _this.patch(source[i], val);
            });
        }
        else if (source instanceof Array || patch instanceof Array)
            return patch;
        else if (typeof source == "object" && typeof patch == "object" && source && patch) {
            for (var p in patch) {
                if (patch[p] == null)
                    source[p] = null;
                else if (source[p] == null)
                    source[p] = patch[p];
                else
                    source[p] = this.patch(source[p], patch[p]);
            }
        }
        else
            return patch;
        return source;
    };
    DotVVM.prototype.diff = function (source, modified) {
        var _this = this;
        if (source instanceof Array && modified instanceof Array) {
            var diffArray = modified.map(function (el, index) { return _this.diff(source[index], el); });
            if (source.length === modified.length
                && diffArray.every(function (el, index) { return el === _this.diffEqual || source[index] === modified[index]; })) {
                return this.diffEqual;
            }
            else {
                return diffArray;
            }
        }
        else if (source instanceof Array || modified instanceof Array) {
            return modified;
        }
        else if (typeof source == "object" && typeof modified == "object" && source && modified) {
            var result = this.diffEqual;
            for (var p in modified) {
                var propertyDiff = this.diff(source[p], modified[p]);
                if (propertyDiff !== this.diffEqual && source[p] !== modified[p]) {
                    if (result === this.diffEqual) {
                        result = {};
                    }
                    result[p] = propertyDiff;
                }
                else if (p[0] === "$") {
                    if (result == this.diffEqual) {
                        result = {};
                    }
                    result[p] = modified[p];
                }
            }
            return result;
        }
        else if (source === modified) {
            if (typeof source == "object") {
                return this.diffEqual;
            }
            else {
                return source;
            }
        }
        else {
            return modified;
        }
    };
    DotVVM.prototype.updateDynamicPathFragments = function (context, path) {
        for (var i = path.length - 1; i >= 0; i--) {
            if (path[i].indexOf("[$index]") >= 0) {
                path[i] = path[i].replace("[$index]", "[" + context.$index() + "]");
            }
            if (path[i].indexOf("[$indexPath]") >= 0) {
                path[i] = path[i].replace("[$indexPath]", "[" + context.$indexPath.map(function (i) { return i(); }).join("]/[") + "]");
            }
            context = context.$parentContext;
        }
    };
    DotVVM.prototype.postJSON = function (url, method, postData, success, error, preprocessRequest) {
        if (preprocessRequest === void 0) { preprocessRequest = function (xhr) { }; }
        var xhr = this.getXHR();
        xhr.open(method, url, true);
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.setRequestHeader("X-DotVVM-PostBack", "true");
        xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
        preprocessRequest(xhr);
        xhr.onreadystatechange = function () {
            if (xhr.readyState !== XMLHttpRequest.DONE)
                return;
            if (xhr.status && xhr.status < 400) {
                success(xhr);
            }
            else {
                error(xhr);
            }
        };
        xhr.send(postData);
    };
    DotVVM.prototype.getJSON = function (url, method, spaPlaceHolderUniqueId, success, error) {
        var xhr = this.getXHR();
        xhr.open(method, url, true);
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.setRequestHeader("X-DotVVM-SpaContentPlaceHolder", spaPlaceHolderUniqueId);
        xhr.onreadystatechange = function () {
            if (xhr.readyState !== XMLHttpRequest.DONE)
                return;
            if (xhr.status && xhr.status < 400) {
                success(xhr);
            }
            else {
                error(xhr);
            }
        };
        xhr.send();
    };
    DotVVM.prototype.getXHR = function () {
        return XMLHttpRequest ? new XMLHttpRequest() : new (window["ActiveXObject"])("Microsoft.XMLHTTP");
    };
    DotVVM.prototype.cleanUpdatedControls = function (resultObject, updatedControls) {
        if (updatedControls === void 0) { updatedControls = {}; }
        for (var id in resultObject.updatedControls) {
            if (resultObject.updatedControls.hasOwnProperty(id)) {
                var control = document.getElementByDotvvmId(id);
                if (control) {
                    var dataContext = ko.contextFor(control);
                    var nextSibling = control.nextSibling;
                    var parent = control.parentNode;
                    ko.removeNode(control);
                    updatedControls[id] = { control: control, nextSibling: nextSibling, parent: parent, dataContext: dataContext };
                }
            }
        }
        return updatedControls;
    };
    DotVVM.prototype.restoreUpdatedControls = function (resultObject, updatedControls, applyBindingsOnEachControl) {
        var _this = this;
        for (var id in resultObject.updatedControls) {
            if (resultObject.updatedControls.hasOwnProperty(id)) {
                var updatedControl = updatedControls[id];
                if (updatedControl) {
                    var wrapper = document.createElement(updatedControls[id].parent.tagName || "div");
                    wrapper.innerHTML = resultObject.updatedControls[id];
                    if (wrapper.childElementCount > 1)
                        throw new Error("Postback.Update control can not render more than one element");
                    var element = wrapper.firstElementChild;
                    if (element.id == null)
                        throw new Error("Postback.Update control always has to render id attribute.");
                    if (element.id !== updatedControls[id].control.id)
                        console.log("Postback.Update control changed id from '" + updatedControls[id].control.id + "' to '" + element.id + "'");
                    wrapper.removeChild(element);
                    if (updatedControl.nextSibling) {
                        updatedControl.parent.insertBefore(element, updatedControl.nextSibling);
                    }
                    else {
                        updatedControl.parent.appendChild(element);
                    }
                }
            }
        }
        if (applyBindingsOnEachControl) {
            window.setTimeout(function () {
                try {
                    _this.isViewModelUpdating = true;
                    for (var id in resultObject.updatedControls) {
                        var updatedControl = document.getElementByDotvvmId(id);
                        if (updatedControl) {
                            ko.applyBindings(updatedControls[id].dataContext, updatedControl);
                        }
                    }
                }
                finally {
                    _this.isViewModelUpdating = false;
                }
            }, 0);
        }
    };
    DotVVM.prototype.unwrapArrayExtension = function (array) {
        return ko.unwrap(ko.unwrap(array));
    };
    DotVVM.prototype.buildRouteUrl = function (routePath, params) {
        // prepend url with backslash to correctly handle optional parameters at start
        routePath = '/' + routePath;
        var url = routePath.replace(/(\/[^\/]*?)\{([^\}]+?)\??(:(.+?))?\}/g, function (s, prefix, paramName, _, type) {
            if (!paramName)
                return "";
            var x = ko.unwrap(params[paramName.toLowerCase()]);
            return x == null ? "" : prefix + x;
        });
        if (url.indexOf('/') === 0) {
            return url.substring(1);
        }
        return url;
    };
    DotVVM.prototype.buildUrlSuffix = function (urlSuffix, query) {
        var resultSuffix, hashSuffix;
        if (urlSuffix.indexOf("#") !== -1) {
            resultSuffix = urlSuffix.substring(0, urlSuffix.indexOf("#"));
            hashSuffix = urlSuffix.substring(urlSuffix.indexOf("#"));
        }
        else {
            resultSuffix = urlSuffix;
            hashSuffix = "";
        }
        for (var property in query) {
            if (query.hasOwnProperty(property)) {
                if (!property)
                    continue;
                var queryParamValue = ko.unwrap(query[property]);
                if (queryParamValue == null)
                    continue;
                resultSuffix = resultSuffix.concat(resultSuffix.indexOf("?") !== -1
                    ? "&" + property + "=" + queryParamValue
                    : "?" + property + "=" + queryParamValue);
            }
        }
        return resultSuffix.concat(hashSuffix);
    };
    DotVVM.prototype.isPostBackProhibited = function (element) {
        if (element && element.tagName && ["a", "input", "button"].indexOf(element.tagName.toLowerCase()) > -1 && element.getAttribute("disabled")) {
            return true;
        }
        return false;
    };
    DotVVM.prototype.addKnockoutBindingHandlers = function () {
        function createWrapperComputed(accessor, propertyDebugInfo) {
            if (propertyDebugInfo === void 0) { propertyDebugInfo = null; }
            var computed = ko.pureComputed({
                read: function () {
                    var property = accessor();
                    var propertyValue = ko.unwrap(property); // has to call that always as it is a dependency
                    return propertyValue;
                },
                write: function (value) {
                    var val = accessor();
                    if (ko.isObservable(val)) {
                        val(value);
                    }
                    else {
                        console.warn("Attempted to write to readonly property" + (propertyDebugInfo == null ? "" : " " + propertyDebugInfo) + ".");
                    }
                }
            });
            computed["wrappedProperty"] = accessor;
            return computed;
        }
        ko.virtualElements.allowedBindings["dotvvm_withControlProperties"] = true;
        ko.bindingHandlers["dotvvm_withControlProperties"] = {
            init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                if (!bindingContext)
                    throw new Error();
                var value = valueAccessor();
                for (var prop in value) {
                    value[prop] = createWrapperComputed(function () {
                        var property = valueAccessor()[this.prop];
                        return !ko.isObservable(property) ? dotvvm.serialization.deserialize(property) : property;
                    }.bind({ prop: prop }), "'" + prop + "' at '" + valueAccessor.toString() + "'");
                }
                var innerBindingContext = bindingContext.extend({ $control: value });
                element.innerBindingContext = innerBindingContext;
                ko.applyBindingsToDescendants(innerBindingContext, element);
                return { controlsDescendantBindings: true }; // do not apply binding again
            },
            update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            }
        };
        ko.virtualElements.allowedBindings["dotvvm_introduceAlias"] = true;
        ko.bindingHandlers["dotvvm_introduceAlias"] = {
            init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                if (!bindingContext)
                    throw new Error();
                var value = valueAccessor();
                var extendBy = {};
                for (var prop in value) {
                    var propPath = prop.split('.');
                    var obj = extendBy;
                    for (var i = 0; i < propPath.length - 1; i) {
                        obj = extendBy[propPath[i]] || (extendBy[propPath[i]] = {});
                    }
                    obj[propPath[propPath.length - 1]] = createWrapperComputed(function () { return valueAccessor()[this.prop]; }.bind({ prop: prop }), "'" + prop + "' at '" + valueAccessor.toString() + "'");
                }
                var innerBindingContext = bindingContext.extend(extendBy);
                element.innerBindingContext = innerBindingContext;
                ko.applyBindingsToDescendants(innerBindingContext, element);
                return { controlsDescendantBindings: true }; // do not apply binding again
            }
        };
        var makeUpdatableChildrenContextHandler = function (makeContextCallback, shouldDisplay) { return function (element, valueAccessor, allBindings, _viewModel, bindingContext) {
            if (!bindingContext)
                throw new Error();
            var savedNodes;
            var isInitial = true;
            ko.computed(function () {
                var rawValue = valueAccessor();
                ko.unwrap(rawValue); // we have to touch the observable in the binding so that the `getDependenciesCount` call knows about this dependency. If would be unwrapped only later (in the makeContextCallback) we would not have the savedNodes.
                // Save a copy of the inner nodes on the initial update, but only if we have dependencies.
                if (isInitial && ko.computedContext.getDependenciesCount()) {
                    savedNodes = ko.utils.cloneNodes(ko.virtualElements.childNodes(element), true /* shouldCleanNodes */);
                }
                if (shouldDisplay(rawValue)) {
                    if (!isInitial) {
                        ko.virtualElements.setDomNodeChildren(element, ko.utils.cloneNodes(savedNodes));
                    }
                    ko.applyBindingsToDescendants(makeContextCallback(bindingContext, rawValue, allBindings), element);
                }
                else {
                    ko.virtualElements.emptyNode(element);
                }
                isInitial = false;
            }, null, { disposeWhenNodeIsRemoved: element });
            return { controlsDescendantBindings: true }; // do not apply binding again
        }; };
        var foreachCollectionSymbol = "$foreachCollectionSymbol";
        ko.virtualElements.allowedBindings["dotvvm-SSR-foreach"] = true;
        ko.bindingHandlers["dotvvm-SSR-foreach"] = {
            init: makeUpdatableChildrenContextHandler(function (bindingContext, rawValue) {
                var _a;
                return bindingContext.extend((_a = {}, _a[foreachCollectionSymbol] = rawValue.data, _a));
            }, function (v) { return v.data != null; })
        };
        ko.virtualElements.allowedBindings["dotvvm-SSR-item"] = true;
        ko.bindingHandlers["dotvvm-SSR-item"] = {
            init: function (element, valueAccessor, _allBindings, _viewModel, bindingContext) {
                if (!bindingContext)
                    throw new Error();
                var collection = bindingContext[foreachCollectionSymbol];
                var innerBindingContext = bindingContext.createChildContext(function () {
                    return ko.unwrap((ko.unwrap(collection) || [])[valueAccessor()]);
                }).extend({ $index: ko.pureComputed(valueAccessor) });
                element.innerBindingContext = innerBindingContext;
                ko.applyBindingsToDescendants(innerBindingContext, element);
                return { controlsDescendantBindings: true }; // do not apply binding again
            },
            update: function (element) {
                if (element.seenUpdate)
                    console.error("dotvvm-SSR-item binding did not expect to see a update");
                element.seenUpdate = 1;
            }
        };
        ko.virtualElements.allowedBindings["withGridViewDataSet"] = true;
        ko.bindingHandlers["withGridViewDataSet"] = {
            init: makeUpdatableChildrenContextHandler(function (bindingContext, value, allBindings) {
                var _a;
                return bindingContext.extend((_a = {
                        $gridViewDataSet: value
                    },
                    _a[foreachCollectionSymbol] = dotvvm.evaluator.getDataSourceItems(value),
                    _a.$gridViewDataSetHelper = {
                        columnMapping: allBindings.get("gridViewDataSetColumnMapping"),
                        isInEditMode: function ($context) {
                            var columnName = $context.$gridViewDataSet().RowEditOptions().PrimaryKeyPropertyName();
                            columnName = this.columnMapping[columnName] || columnName;
                            return $context.$gridViewDataSet().RowEditOptions().EditRowId() === $context.$data[columnName]();
                        }
                    },
                    _a));
            }, function (_) { return true; })
        };
        ko.bindingHandlers['dotvvmEnable'] = {
            'update': function (element, valueAccessor) {
                var value = ko.utils.unwrapObservable(valueAccessor());
                if (value && element.disabled) {
                    element.disabled = false;
                    element.removeAttribute("disabled");
                }
                else if ((!value) && (!element.disabled)) {
                    element.disabled = true;
                    element.setAttribute("disabled", "disabled");
                }
            }
        };
        ko.bindingHandlers['dotvvm-checkbox-updateAfterPostback'] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                dotvvm.events.afterPostback.subscribe(function (e) {
                    var bindings = allBindingsAccessor();
                    if (bindings["dotvvm-checked-pointer"]) {
                        var checked = bindings[bindings["dotvvm-checked-pointer"]];
                        if (ko.isObservable(checked)) {
                            if (checked.valueHasMutated) {
                                checked.valueHasMutated();
                            }
                            else {
                                checked.notifySubscribers();
                            }
                        }
                    }
                });
            }
        };
        ko.bindingHandlers['dotvvm-checked-pointer'] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            }
        };
        ko.bindingHandlers["dotvvm-checkedItems"] = {
            after: ko.bindingHandlers.checked.after,
            init: ko.bindingHandlers.checked.init,
            options: ko.bindingHandlers.checked.options,
            update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                var value = valueAccessor();
                if (!Array.isArray(ko.unwrap(value))) {
                    throw Error("The value of a `checkedItems` binding must be an array (i.e. not null nor undefined).");
                }
                // Note: As of now, the `checked` binding doesn't have an `update`. If that changes, invoke it here.
            }
        };
        ko.bindingHandlers["dotvvm-UpdateProgress-Visible"] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                element.style.display = "none";
                var delay = element.getAttribute("data-delay");
                var includedQueues = (element.getAttribute("data-included-queues") || "").split(",").filter(function (i) { return i.length > 0; });
                var excludedQueues = (element.getAttribute("data-excluded-queues") || "").split(",").filter(function (i) { return i.length > 0; });
                var timeout;
                var running = false;
                var show = function () {
                    running = true;
                    if (delay == null) {
                        element.style.display = "";
                    }
                    else {
                        timeout = setTimeout(function (e) {
                            element.style.display = "";
                        }, delay);
                    }
                };
                var hide = function () {
                    running = false;
                    clearTimeout(timeout);
                    element.style.display = "none";
                };
                dotvvm.updateProgressChangeCounter.subscribe(function (e) {
                    var shouldRun = false;
                    if (includedQueues.length === 0) {
                        for (var queue in dotvvm.postbackQueues) {
                            if (excludedQueues.indexOf(queue) < 0 && dotvvm.postbackQueues[queue].noRunning > 0) {
                                shouldRun = true;
                                break;
                            }
                        }
                    }
                    else {
                        shouldRun = includedQueues.some(function (q) { return dotvvm.postbackQueues[q] && dotvvm.postbackQueues[q].noRunning > 0; });
                    }
                    if (shouldRun) {
                        if (!running) {
                            show();
                        }
                    }
                    else {
                        hide();
                    }
                });
            }
        };
        ko.bindingHandlers['dotvvm-table-columnvisible'] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                var lastDisplay = "";
                var currentVisible = true;
                function changeVisibility(table, columnIndex, visible) {
                    if (currentVisible == visible)
                        return;
                    currentVisible = visible;
                    for (var i = 0; i < table.rows.length; i++) {
                        var row_1 = table.rows.item(i);
                        var style = row_1.cells[columnIndex].style;
                        if (visible) {
                            style.display = lastDisplay;
                        }
                        else {
                            lastDisplay = style.display || "";
                            style.display = "none";
                        }
                    }
                }
                if (!(element instanceof HTMLTableCellElement))
                    return;
                // find parent table
                var table = element;
                while (!(table instanceof HTMLTableElement))
                    table = table.parentElement;
                var row = table.rows.item(0);
                if (!row)
                    throw Error("Table with dotvvm-table-columnvisible binding handler must not be empty.");
                var colIndex = [].slice.call(row.cells).indexOf(element);
                element['dotvvmChangeVisibility'] = changeVisibility.bind(null, table, colIndex);
            },
            update: function (element, valueAccessor) {
                element.dotvvmChangeVisibility(ko.unwrap(valueAccessor()));
            }
        };
        ko.bindingHandlers['dotvvm-textbox-text'] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                var obs = valueAccessor(), valueUpdate = allBindingsAccessor.get("valueUpdate");
                //generate metadata func
                var elmMetadata = new DotvvmValidationElementMetadata();
                elmMetadata.element = element;
                elmMetadata.dataType = (element.attributes["data-dotvvm-value-type"] || { value: "" }).value;
                elmMetadata.format = (element.attributes["data-dotvvm-format"] || { value: "" }).value;
                //add metadata for validation
                var metadata = null;
                if (ko.isObservable(obs)) {
                    if (!obs.dotvvmMetadata) {
                        obs.dotvvmMetadata = new DotvvmValidationObservableMetadata();
                        obs.dotvvmMetadata.elementsMetadata = [elmMetadata];
                    }
                    else {
                        if (!obs.dotvvmMetadata.elementsMetadata) {
                            obs.dotvvmMetadata.elementsMetadata = [];
                        }
                        obs.dotvvmMetadata.elementsMetadata.push(elmMetadata);
                    }
                    metadata = obs.dotvvmMetadata.elementsMetadata;
                }
                else {
                    metadata = new DotvvmValidationObservableMetadata();
                }
                setTimeout(function (metaArray, element) {
                    // remove element from collection when its removed from dom
                    ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                        for (var _i = 0, metaArray_1 = metaArray; _i < metaArray_1.length; _i++) {
                            var meta = metaArray_1[_i];
                            if (meta.element === element) {
                                metaArray.splice(metaArray.indexOf(meta), 1);
                                break;
                            }
                        }
                    });
                }, 0, metadata, element);
                dotvvm.domUtils.attachEvent(element, "change", function () {
                    if (!ko.isObservable(obs))
                        return;
                    // parse the value
                    var result, isEmpty, newValue;
                    if (elmMetadata.dataType === "datetime") {
                        // parse date
                        var currentValue = obs();
                        if (currentValue != null) {
                            currentValue = dotvvm.globalize.parseDotvvmDate(currentValue);
                        }
                        result = dotvvm.globalize.parseDate(element.value, elmMetadata.format, currentValue);
                        isEmpty = result == null;
                        newValue = isEmpty ? null : dotvvm.serialization.serializeDate(result, false);
                    }
                    else {
                        // parse number
                        result = dotvvm.globalize.parseNumber(element.value);
                        isEmpty = result === null || isNaN(result);
                        newValue = isEmpty ? null : result;
                    }
                    // update element validation metadata
                    if (newValue == null && element.value !== null && element.value !== "") {
                        element.attributes["data-invalid-value"] = element.value;
                        element.attributes["data-dotvvm-value-type-valid"] = false;
                        elmMetadata.elementValidationState = false;
                    }
                    else {
                        element.attributes["data-invalid-value"] = null;
                        element.attributes["data-dotvvm-value-type-valid"] = true;
                        elmMetadata.elementValidationState = true;
                    }
                    if (obs() === newValue) {
                        if (obs.valueHasMutated) {
                            obs.valueHasMutated();
                        }
                        else {
                            obs.notifySubscribers();
                        }
                    }
                    else {
                        obs(newValue);
                    }
                });
            },
            update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                var obs = valueAccessor(), format = (element.attributes["data-dotvvm-format"] || { value: "" }).value, value = ko.unwrap(obs);
                if (format) {
                    var formatted = dotvvm.globalize.formatString(format, value), invalidValue = element.attributes["data-invalid-value"];
                    if (invalidValue == null) {
                        element.value = formatted || "";
                        if (obs.dotvvmMetadata && obs.dotvvmMetadata.elementsMetadata) {
                            var elemsMetadata = obs.dotvvmMetadata.elementsMetadata;
                            for (var _i = 0, elemsMetadata_1 = elemsMetadata; _i < elemsMetadata_1.length; _i++) {
                                var elemMetadata = elemsMetadata_1[_i];
                                if (elemMetadata.element === element) {
                                    element.attributes["data-dotvvm-value-type-valid"] = true;
                                    elemMetadata.elementValidationState = true;
                                }
                            }
                        }
                    }
                    else {
                        element.attributes["data-invalid-value"] = null;
                        element.value = invalidValue;
                    }
                }
                else {
                    element.value = value;
                }
            }
        };
        ko.bindingHandlers["dotvvm-textbox-select-all-on-focus"] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                element.$selectAllOnFocusHandler = function () {
                    element.select();
                };
            },
            update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                var value = ko.unwrap(valueAccessor());
                if (value === true) {
                    element.addEventListener("focus", element.$selectAllOnFocusHandler);
                }
                else {
                    element.removeEventListener("focus", element.$selectAllOnFocusHandler);
                }
            }
        };
        ko.bindingHandlers["dotvvm-CheckState"] = {
            init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                ko.getBindingHandler("checked").init(element, valueAccessor, allBindings, viewModel, bindingContext);
            },
            update: function (element, valueAccessor, allBindings) {
                var value = ko.unwrap(valueAccessor());
                element.indeterminate = value == null;
            }
        };
    };
    return DotVVM;
}());
/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="DotVVM.ts" />
var DotvvmValidationContext = /** @class */ (function () {
    function DotvvmValidationContext(valueToValidate, parentViewModel, parameters) {
        this.valueToValidate = valueToValidate;
        this.parentViewModel = parentViewModel;
        this.parameters = parameters;
    }
    return DotvvmValidationContext;
}());
var DotvvmValidationObservableMetadata = /** @class */ (function () {
    function DotvvmValidationObservableMetadata() {
    }
    return DotvvmValidationObservableMetadata;
}());
var DotvvmValidationElementMetadata = /** @class */ (function () {
    function DotvvmValidationElementMetadata() {
        this.elementValidationState = true;
    }
    return DotvvmValidationElementMetadata;
}());
var DotvvmValidatorBase = /** @class */ (function () {
    function DotvvmValidatorBase() {
    }
    DotvvmValidatorBase.prototype.isValid = function (context, property) {
        return false;
    };
    DotvvmValidatorBase.prototype.isEmpty = function (value) {
        return value == null || (typeof value == "string" && value.trim() === "");
    };
    DotvvmValidatorBase.prototype.getValidationMetadata = function (property) {
        return property.dotvvmMetadata;
    };
    return DotvvmValidatorBase;
}());
var DotvvmRequiredValidator = /** @class */ (function (_super) {
    __extends(DotvvmRequiredValidator, _super);
    function DotvvmRequiredValidator() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DotvvmRequiredValidator.prototype.isValid = function (context) {
        var value = context.valueToValidate;
        return !this.isEmpty(value);
    };
    return DotvvmRequiredValidator;
}(DotvvmValidatorBase));
var DotvvmRegularExpressionValidator = /** @class */ (function (_super) {
    __extends(DotvvmRegularExpressionValidator, _super);
    function DotvvmRegularExpressionValidator() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DotvvmRegularExpressionValidator.prototype.isValid = function (context) {
        var value = context.valueToValidate;
        var expr = context.parameters[0];
        return this.isEmpty(value) || new RegExp(expr).test(value);
    };
    return DotvvmRegularExpressionValidator;
}(DotvvmValidatorBase));
var DotvvmIntRangeValidator = /** @class */ (function (_super) {
    __extends(DotvvmIntRangeValidator, _super);
    function DotvvmIntRangeValidator() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DotvvmIntRangeValidator.prototype.isValid = function (context) {
        var val = context.valueToValidate;
        var from = context.parameters[0];
        var to = context.parameters[1];
        return val % 1 === 0 && val >= from && val <= to;
    };
    return DotvvmIntRangeValidator;
}(DotvvmValidatorBase));
var DotvvmEnforceClientFormatValidator = /** @class */ (function (_super) {
    __extends(DotvvmEnforceClientFormatValidator, _super);
    function DotvvmEnforceClientFormatValidator() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DotvvmEnforceClientFormatValidator.prototype.isValid = function (context, property) {
        // parameters order: AllowNull, AllowEmptyString, AllowEmptyStringOrWhitespaces
        var valid = true;
        if (!context.parameters[0] && context.valueToValidate == null) // AllowNull
         {
            valid = false;
        }
        if (!context.parameters[1] && context.valueToValidate.length === 0) // AllowEmptyString
         {
            valid = false;
        }
        if (!context.parameters[2] && this.isEmpty(context.valueToValidate)) // AllowEmptyStringOrWhitespaces
         {
            valid = false;
        }
        var metadata = this.getValidationMetadata(property);
        if (metadata && metadata.elementsMetadata) {
            for (var _i = 0, _a = metadata.elementsMetadata; _i < _a.length; _i++) {
                var metaElement = _a[_i];
                if (!metaElement.elementValidationState) {
                    valid = false;
                }
            }
        }
        return valid;
    };
    return DotvvmEnforceClientFormatValidator;
}(DotvvmValidatorBase));
var DotvvmRangeValidator = /** @class */ (function (_super) {
    __extends(DotvvmRangeValidator, _super);
    function DotvvmRangeValidator() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DotvvmRangeValidator.prototype.isValid = function (context, property) {
        var val = context.valueToValidate;
        var from = context.parameters[0];
        var to = context.parameters[1];
        return val >= from && val <= to;
    };
    return DotvvmRangeValidator;
}(DotvvmValidatorBase));
var DotvvmNotNullValidator = /** @class */ (function (_super) {
    __extends(DotvvmNotNullValidator, _super);
    function DotvvmNotNullValidator() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DotvvmNotNullValidator.prototype.isValid = function (context) {
        return context.valueToValidate !== null && context.valueToValidate !== undefined;
    };
    return DotvvmNotNullValidator;
}(DotvvmValidatorBase));
var DotvvmEmailAddressValidator = /** @class */ (function (_super) {
    __extends(DotvvmEmailAddressValidator, _super);
    function DotvvmEmailAddressValidator() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DotvvmEmailAddressValidator.prototype.isValid = function (context) {
        var value = context.valueToValidate;
        if (value == null)
            return true;
        if (typeof value !== "string")
            return false;
        var found = false;
        for (var i = 0; i < value.length; i++) {
            if (value[i] == '@') {
                if (found || i == 0 || i == value.length - 1) {
                    return false;
                }
                found = true;
            }
        }
        return found;
    };
    return DotvvmEmailAddressValidator;
}(DotvvmValidatorBase));
var ErrorsPropertyName = "validationErrors";
var ValidationError = /** @class */ (function () {
    function ValidationError(errorMessage, validatedObservable) {
        this.errorMessage = errorMessage;
        this.validatedObservable = validatedObservable;
    }
    ValidationError.attach = function (errorMessage, observable) {
        if (!errorMessage) {
            throw new Error("String \"" + errorMessage + "\" is not a valid ValidationError message.");
        }
        if (!observable) {
            throw new Error("ValidationError cannot be attached to \"" + observable + "\".");
        }
        if ("wrappedProperty" in observable) {
            observable = observable["wrappedProperty"]();
        }
        if (!observable.hasOwnProperty(ErrorsPropertyName)) {
            observable[ErrorsPropertyName] = [];
        }
        var error = new ValidationError(errorMessage, observable);
        observable[ErrorsPropertyName].push(error);
        dotvvm.validation.errors.push(error);
        return error;
    };
    ValidationError.prototype.detach = function () {
        var observableIndex = this.validatedObservable.validationErrors.indexOf(this);
        this.validatedObservable.validationErrors.splice(observableIndex, 1);
        var arrayIndex = dotvvm.validation.errors.indexOf(this);
        dotvvm.validation.errors.splice(arrayIndex, 1);
    };
    return ValidationError;
}());
var DotvvmValidation = /** @class */ (function () {
    function DotvvmValidation(dotvvm) {
        var _this = this;
        this.rules = {
            "required": new DotvvmRequiredValidator(),
            "regularExpression": new DotvvmRegularExpressionValidator(),
            "intrange": new DotvvmIntRangeValidator(),
            "range": new DotvvmRangeValidator(),
            "emailAddress": new DotvvmEmailAddressValidator(),
            "notnull": new DotvvmNotNullValidator(),
            "enforceClientFormat": new DotvvmEnforceClientFormatValidator()
        };
        this.errors = [];
        this.events = {
            validationErrorsChanged: new DotvvmEvent("dotvvm.validation.events.validationErrorsChanged")
        };
        this.elementUpdateFunctions = {
            // shows the element when it is valid
            hideWhenValid: function (element, errorMessages, param) {
                if (errorMessages.length > 0) {
                    element.style.display = "";
                }
                else {
                    element.style.display = "none";
                }
            },
            // adds a CSS class when the element is not valid
            invalidCssClass: function (element, errorMessages, className) {
                var classes = className.split(/\s+/).filter(function (c) { return c.length > 0; });
                for (var i = 0; i < classes.length; i++) {
                    var className_1 = classes[i];
                    if (errorMessages.length > 0) {
                        element.classList.add(className_1);
                    }
                    else {
                        element.classList.remove(className_1);
                    }
                }
            },
            // sets the error message as the title attribute
            setToolTipText: function (element, errorMessages, param) {
                if (errorMessages.length > 0) {
                    element.title = errorMessages.join(" ");
                }
                else {
                    element.title = "";
                }
            },
            // displays the error message
            showErrorMessageText: function (element, errorMessages, param) {
                element[element.innerText ? "innerText" : "textContent"] = errorMessages.join(" ");
            }
        };
        var createValidationHandler = function (path) { return ({
            execute: function (callback, options) {
                if (path) {
                    options.additionalPostbackData.validationTargetPath = path;
                    // resolve target
                    var context = ko.contextFor(options.sender);
                    var validationTarget = dotvvm.evaluator.evaluateOnViewModel(context, path);
                    _this.detachAllErrors();
                    _this.validateViewModel(validationTarget);
                    _this.events.validationErrorsChanged.trigger({ viewModel: options.viewModel });
                    if (_this.errors.length > 0) {
                        console.log("Validation failed: postback aborted; errors: ", _this.errors);
                        return Promise.reject({ type: "handler", handler: _this, message: "Validation failed" });
                    }
                }
                return callback();
            }
        }); };
        dotvvm.postbackHandlers["validate"] = function (opt) { return createValidationHandler(opt.path); };
        dotvvm.postbackHandlers["validate-root"] = function () { return createValidationHandler("dotvvm.viewModelObservables['root']"); };
        dotvvm.postbackHandlers["validate-this"] = function () { return createValidationHandler("$data"); };
        dotvvm.events.afterPostback.subscribe(function (args) {
            if (!args.wasInterrupted && args.serverResponseObject) {
                if (args.serverResponseObject.action === "successfulCommand") {
                    // merge validation rules from postback with those we already have (required when a new type appears in the view model)
                    _this.mergeValidationRules(args);
                    args.isHandled = true;
                }
                else if (args.serverResponseObject.action === "validationErrors") {
                    // apply validation errors from server
                    _this.detachAllErrors();
                    _this.showValidationErrorsFromServer(args);
                    _this.events.validationErrorsChanged.trigger(args);
                    args.isHandled = true;
                }
            }
        });
        dotvvm.events.spaNavigating.subscribe(function (args) {
            _this.detachAllErrors();
            _this.events.validationErrorsChanged.trigger({ viewModel: args.viewModel });
        });
        // Validator
        ko.bindingHandlers["dotvvmValidation"] = {
            init: function (element, valueAccessor, allBindingsAccessor) {
                dotvvm.validation.events.validationErrorsChanged.subscribe(function (e) {
                    _this.applyValidatorOptions(element, valueAccessor(), allBindingsAccessor.get("dotvvmValidationOptions"));
                });
            },
            update: function (element, valueAccessor, allBindingsAccessor) {
                _this.applyValidatorOptions(element, valueAccessor(), allBindingsAccessor.get("dotvvmValidationOptions"));
            }
        };
        // ValidationSummary
        ko.bindingHandlers["dotvvm-validationSummary"] = {
            init: function (element, valueAccessor) {
                var binding = valueAccessor();
                dotvvm.validation.events.validationErrorsChanged.subscribe(function (e) {
                    element.innerHTML = "";
                    var errors = dotvvm.validation.getValidationErrors(binding.target, binding.includeErrorsFromChildren, binding.includeErrorsFromTarget);
                    for (var _i = 0, errors_1 = errors; _i < errors_1.length; _i++) {
                        var error = errors_1[_i];
                        var item = document.createElement("li");
                        item.innerText = error.errorMessage;
                        element.appendChild(item);
                    }
                    if (binding.hideWhenValid) {
                        if (errors.length > 0) {
                            element.style.display = "";
                        }
                        else {
                            element.style.display = "none";
                        }
                    }
                });
            }
        };
    }
    /**
     * Validates the specified view model
    */
    DotvvmValidation.prototype.validateViewModel = function (viewModel) {
        if (ko.isObservable(viewModel)) {
            viewModel = ko.unwrap(viewModel);
        }
        if (!viewModel) {
            return;
        }
        // find validation rules for the property type
        var rootRules = dotvvm.viewModels['root'].validationRules || {};
        var type = ko.unwrap(viewModel.$type);
        var rules = rootRules[type] || {};
        // validate all properties
        for (var propertyName in viewModel) {
            if (!viewModel.hasOwnProperty(propertyName) || propertyName.indexOf("$") === 0) {
                continue;
            }
            var observable = viewModel[propertyName];
            if (!observable || !ko.isObservable(observable)) {
                continue;
            }
            var propertyValue = observable();
            // run validators
            if (rules.hasOwnProperty(propertyName)) {
                this.validateProperty(viewModel, observable, propertyValue, rules[propertyName]);
            }
            // check the value is even valid for the given type
            var options = viewModel[propertyName + "$options"];
            if (options
                && options.type
                && !DotvvmValidation.hasErrors(observable)
                && !dotvvm.serialization.validateType(propertyValue, options.type)) {
                ValidationError.attach("The value of property " + propertyName + " (" + propertyValue + ") is invalid value for type " + options.type + ".", observable);
            }
            if (!propertyValue) {
                continue;
            }
            // recurse
            if (Array.isArray(propertyValue)) {
                // handle collections
                for (var _i = 0, propertyValue_1 = propertyValue; _i < propertyValue_1.length; _i++) {
                    var item = propertyValue_1[_i];
                    this.validateViewModel(item);
                }
            }
            else if (propertyValue && propertyValue instanceof Object) {
                // handle nested objects
                this.validateViewModel(propertyValue);
            }
        }
    };
    // validates the specified property in the viewModel
    DotvvmValidation.prototype.validateProperty = function (viewModel, property, value, propertyRules) {
        for (var _i = 0, propertyRules_1 = propertyRules; _i < propertyRules_1.length; _i++) {
            var rule = propertyRules_1[_i];
            // validate the rules
            var validator = this.rules[rule.ruleName];
            var context = new DotvvmValidationContext(value, viewModel, rule.parameters);
            if (!validator.isValid(context, property)) {
                ValidationError.attach(rule.errorMessage, property);
            }
        }
    };
    // merge validation rules
    DotvvmValidation.prototype.mergeValidationRules = function (args) {
        if (args.serverResponseObject.validationRules) {
            var existingRules = dotvvm.viewModels[args.viewModelName].validationRules;
            if (typeof existingRules === "undefined") {
                dotvvm.viewModels[args.viewModelName].validationRules = {};
                existingRules = dotvvm.viewModels[args.viewModelName].validationRules;
            }
            for (var type in args.serverResponseObject.validationRules) {
                if (!args.serverResponseObject.validationRules.hasOwnProperty(type))
                    continue;
                existingRules[type] = args.serverResponseObject.validationRules[type];
            }
        }
    };
    /**
     * Clears validation errors from the passed viewModel, from its children
     * and from the DotvvmValidation.errors array
     */
    DotvvmValidation.prototype.clearValidationErrors = function (observable) {
        if (!observable || !ko.isObservable(observable)) {
            return;
        }
        if (observable[ErrorsPropertyName]) {
            // clone the array as `detach` mutates it
            var errors = observable[ErrorsPropertyName].concat([]);
            for (var _i = 0, errors_2 = errors; _i < errors_2.length; _i++) {
                var error = errors_2[_i];
                error.detach();
            }
        }
        var validatedObject = ko.unwrap(observable);
        if (!validatedObject) {
            return;
        }
        if (Array.isArray(validatedObject)) {
            // element recursion
            for (var _a = 0, validatedObject_1 = validatedObject; _a < validatedObject_1.length; _a++) {
                var item = validatedObject_1[_a];
                this.clearValidationErrors(item);
            }
        }
        for (var propertyName in validatedObject) {
            // property recursion
            if (!validatedObject.hasOwnProperty(propertyName) || propertyName.indexOf("$") === 0) {
                continue;
            }
            var property = validatedObject[propertyName];
            this.clearValidationErrors(property);
        }
    };
    DotvvmValidation.prototype.detachAllErrors = function () {
        while (this.errors.length > 0) {
            this.errors[0].detach();
        }
    };
    /**
     * Gets validation errors from the passed object and its children.
     * @param validationTargetObservable Object that is supposed to contain the errors or properties with the errors
     * @param includeErrorsFromGrandChildren Is called "IncludeErrorsFromChildren" in ValidationSummary.cs
     * @returns By default returns only errors from the viewModel's immediate children
     */
    DotvvmValidation.prototype.getValidationErrors = function (validationTargetObservable, includeErrorsFromGrandChildren, includeErrorsFromTarget, includeErrorsFromChildren) {
        if (includeErrorsFromChildren === void 0) { includeErrorsFromChildren = true; }
        if (!validationTargetObservable) {
            return [];
        }
        var errors = [];
        if (includeErrorsFromTarget && validationTargetObservable.hasOwnProperty(ErrorsPropertyName)) {
            errors = errors.concat(validationTargetObservable[ErrorsPropertyName]);
        }
        if (!includeErrorsFromChildren) {
            return errors;
        }
        var validationTarget = ko.unwrap(validationTargetObservable);
        if (Array.isArray(validationTarget)) {
            for (var _i = 0, validationTarget_1 = validationTarget; _i < validationTarget_1.length; _i++) {
                var item = validationTarget_1[_i];
                // the next children are grandchildren
                errors = errors.concat(this.getValidationErrors(item, includeErrorsFromGrandChildren, true, includeErrorsFromGrandChildren));
            }
            return errors;
        }
        for (var propertyName in validationTarget) {
            if (!validationTarget.hasOwnProperty(propertyName) || propertyName.indexOf("$") === 0) {
                continue;
            }
            var property = validationTarget[propertyName];
            if (!property || !ko.isObservable(property)) {
                continue;
            }
            // consider nested properties to be children
            errors = errors.concat(this.getValidationErrors(property, includeErrorsFromGrandChildren, true, includeErrorsFromGrandChildren));
        }
        return errors;
    };
    /**
     * Adds validation errors from the server to the appropriate arrays
     */
    DotvvmValidation.prototype.showValidationErrorsFromServer = function (args) {
        // resolve validation target
        var context = ko.contextFor(args.sender);
        var validationTarget = dotvvm.evaluator.evaluateOnViewModel(context, args.postbackOptions.additionalPostbackData.validationTargetPath);
        if (!validationTarget) {
            return;
        }
        // add validation errors
        var modelState = args.serverResponseObject.modelState;
        for (var i = 0; i < modelState.length; i++) {
            // find the property
            var propertyPath = modelState[i].propertyPath;
            var property;
            if (propertyPath) {
                if (ko.isObservable(validationTarget)) {
                    validationTarget = ko.unwrap(validationTarget);
                }
                property = dotvvm.evaluator.evaluateOnViewModel(validationTarget, propertyPath);
            }
            else {
                property = validationTarget;
            }
            ValidationError.attach(modelState[i].errorMessage, property);
        }
    };
    DotvvmValidation.hasErrors = function (observable) {
        if ("wrappedProperty" in observable) {
            observable = observable["wrappedProperty"]();
        }
        return observable.hasOwnProperty(ErrorsPropertyName)
            && observable[ErrorsPropertyName].length > 0;
    };
    DotvvmValidation.prototype.applyValidatorOptions = function (validator, observable, validatorOptions) {
        if (!ko.isObservable(observable)) {
            return;
        }
        if ("wrappedProperty" in observable) {
            observable = observable["wrappedProperty"]();
        }
        var errors = observable[ErrorsPropertyName] || [];
        var errorMessages = errors.map(function (v) { return v.errorMessage; });
        for (var option in validatorOptions) {
            if (validatorOptions.hasOwnProperty(option)) {
                dotvvm.validation.elementUpdateFunctions[option](validator, errorMessages, validatorOptions[option]);
            }
        }
    };
    return DotvvmValidation;
}());
;
var DotvvmEvaluator = /** @class */ (function () {
    function DotvvmEvaluator() {
    }
    DotvvmEvaluator.prototype.evaluateOnViewModel = function (context, expression) {
        var result;
        if (context && context.$data) {
            result = eval("(function ($context) { with($context) { with ($data) { return " + expression + "; } } })")(context);
        }
        else {
            result = eval("(function ($context) { var $data=$context; with($context) { return " + expression + "; } })")(context);
        }
        if (result && result.$data) {
            result = result.$data;
        }
        return result;
    };
    DotvvmEvaluator.prototype.evaluateOnContext = function (context, expression) {
        var startsWithProperty = false;
        for (var prop in context) {
            if (expression.indexOf(prop) === 0) {
                startsWithProperty = true;
                break;
            }
        }
        if (!startsWithProperty)
            expression = "$data." + expression;
        return this.evaluateOnViewModel(context, expression);
    };
    DotvvmEvaluator.prototype.getDataSourceItems = function (viewModel) {
        var value = ko.unwrap(viewModel);
        if (typeof value === "undefined" || value == null)
            return [];
        return ko.unwrap(value.Items || value);
    };
    DotvvmEvaluator.prototype.tryEval = function (func) {
        try {
            return func();
        }
        catch (error) {
            return null;
        }
    };
    DotvvmEvaluator.prototype.isObservableArray = function (instance) {
        if (ko.isComputed(instance)) {
            return Array.isArray(instance.peek());
        }
        else if (ko.isObservable(instance)) {
            return "push" in instance;
        }
        return false;
    };
    DotvvmEvaluator.prototype.wrapObservable = function (func, isArray) {
        var _this = this;
        var wrapper = ko.pureComputed({
            read: function () { return ko.unwrap(_this.getExpressionResult(func)); },
            write: function (value) { return _this.updateObservable(func, value); }
        });
        if (isArray) {
            wrapper.push = function () {
                var args = [];
                for (var _i = 0; _i < arguments.length; _i++) {
                    args[_i] = arguments[_i];
                }
                return _this.updateObservableArray(func, "push", args);
            };
            wrapper.pop = function () {
                var args = [];
                for (var _i = 0; _i < arguments.length; _i++) {
                    args[_i] = arguments[_i];
                }
                return _this.updateObservableArray(func, "pop", args);
            };
            wrapper.unshift = function () {
                var args = [];
                for (var _i = 0; _i < arguments.length; _i++) {
                    args[_i] = arguments[_i];
                }
                return _this.updateObservableArray(func, "unshift", args);
            };
            wrapper.shift = function () {
                var args = [];
                for (var _i = 0; _i < arguments.length; _i++) {
                    args[_i] = arguments[_i];
                }
                return _this.updateObservableArray(func, "shift", args);
            };
            wrapper.reverse = function () {
                var args = [];
                for (var _i = 0; _i < arguments.length; _i++) {
                    args[_i] = arguments[_i];
                }
                return _this.updateObservableArray(func, "reverse", args);
            };
            wrapper.sort = function () {
                var args = [];
                for (var _i = 0; _i < arguments.length; _i++) {
                    args[_i] = arguments[_i];
                }
                return _this.updateObservableArray(func, "sort", args);
            };
            wrapper.splice = function () {
                var args = [];
                for (var _i = 0; _i < arguments.length; _i++) {
                    args[_i] = arguments[_i];
                }
                return _this.updateObservableArray(func, "splice", args);
            };
            wrapper.slice = function () {
                var args = [];
                for (var _i = 0; _i < arguments.length; _i++) {
                    args[_i] = arguments[_i];
                }
                return _this.updateObservableArray(func, "slice", args);
            };
            wrapper.replace = function () {
                var args = [];
                for (var _i = 0; _i < arguments.length; _i++) {
                    args[_i] = arguments[_i];
                }
                return _this.updateObservableArray(func, "replace", args);
            };
            wrapper.indexOf = function () {
                var args = [];
                for (var _i = 0; _i < arguments.length; _i++) {
                    args[_i] = arguments[_i];
                }
                return _this.updateObservableArray(func, "indexOf", args);
            };
            wrapper.remove = function () {
                var args = [];
                for (var _i = 0; _i < arguments.length; _i++) {
                    args[_i] = arguments[_i];
                }
                return _this.updateObservableArray(func, "remove", args);
            };
            wrapper.removeAll = function () {
                var args = [];
                for (var _i = 0; _i < arguments.length; _i++) {
                    args[_i] = arguments[_i];
                }
                return _this.updateObservableArray(func, "removeAll", args);
            };
            wrapper = wrapper.extend({ trackArrayChanges: true });
        }
        return wrapper.extend({ notify: "always" });
    };
    DotvvmEvaluator.prototype.updateObservable = function (getObservable, value) {
        var result = this.getExpressionResult(getObservable);
        if (!ko.isWriteableObservable(result)) {
            console.error("Cannot write a value to ko.computed because the expression '" + getObservable + "' does not return a writable observable.");
        }
        else {
            result(value);
        }
    };
    DotvvmEvaluator.prototype.updateObservableArray = function (getObservableArray, fnName, args) {
        var result = this.getExpressionResult(getObservableArray);
        if (!this.isObservableArray(result)) {
            console.error("Cannot execute '" + fnName + "' function on ko.computed because the expression '" + getObservableArray + "' does not return an observable array.");
        }
        else {
            result[fnName].apply(result, args);
        }
    };
    DotvvmEvaluator.prototype.getExpressionResult = function (func) {
        var result = func();
        if (ko.isComputed(result) && "wrappedProperty" in result) {
            result = result["wrappedProperty"](); // workaround for dotvvm_withControlProperties handler
        }
        return result;
    };
    return DotvvmEvaluator;
}());
/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="DotVVM.ts" />
var DotvvmEventHub = /** @class */ (function () {
    function DotvvmEventHub() {
        this.map = {};
    }
    DotvvmEventHub.prototype.notify = function (id) {
        if (id in this.map)
            this.map[id].notifySubscribers();
        else
            this.map[id] = ko.observable(0);
    };
    DotvvmEventHub.prototype.get = function (id) {
        return this.map[id] || (this.map[id] = ko.observable(0));
    };
    return DotvvmEventHub;
}());
function basicAuthenticatedFetch(input, init) {
    function requestAuth() {
        var a = prompt("You credentials for " + (input["url"] || input)) || "";
        sessionStorage.setItem("someAuth", a);
        return a;
    }
    var auth = sessionStorage.getItem("someAuth");
    if (auth != null) {
        if (init == null)
            init = {};
        if (init.headers == null)
            init.headers = {};
        if (init.headers['Authorization'] == null)
            init.headers["Authorization"] = 'Basic ' + btoa(auth);
    }
    if (init == null)
        init = {};
    if (!init.cache)
        init.cache = "no-cache";
    return window.fetch(input, init).then(function (response) {
        if (response.status === 401 && auth == null) {
            if (sessionStorage.getItem("someAuth") == null)
                requestAuth();
            return basicAuthenticatedFetch(input, init);
        }
        else {
            return response;
        }
    });
}
(function () {
    var cachedValues = {};
    DotVVM.prototype.invokeApiFn = function (target, methodName, argsProvider, refreshTriggers, notifyTriggers, element, sharingKeyProvider) {
        var args = ko.ignoreDependencies(argsProvider);
        var callback = function () { return target[methodName].apply(target, args); };
        // the function gets re-evaluated when the observable changes - thus we need to cache the values
        // GET requests can be cached globally, POST and other request must be cached on per-element scope
        var sharingKeyValue = methodName + ":" + sharingKeyProvider(args);
        var cache = element ? (element["apiCachedValues"] || (element["apiCachedValues"] = {})) : cachedValues;
        var cachedValue = cache[sharingKeyValue] || (cache[sharingKeyValue] = ko.observable(null));
        var load = function () {
            try {
                var result = window["Promise"].resolve(ko.ignoreDependencies(callback));
                return { type: 'result', result: result.then(function (val) {
                        if (val) {
                            cachedValue(ko.unwrap(dotvvm.serialization.deserialize(val)));
                            cachedValue.notifySubscribers();
                        }
                        for (var _i = 0, _a = notifyTriggers(args); _i < _a.length; _i++) {
                            var t = _a[_i];
                            dotvvm.eventHub.notify(t);
                        }
                        return val;
                    }, console.warn) };
            }
            catch (e) {
                console.warn(e);
                return { type: 'error', error: e };
            }
        };
        var cmp = ko.pureComputed(function () { return cachedValue(); });
        cmp.refreshValue = function (throwOnError) {
            var promise = cachedValue["promise"];
            if (!cachedValue["isLoading"]) {
                cachedValue["isLoading"] = true;
                promise = load();
                cachedValue["promise"] = promise;
            }
            if (promise.type == 'error') {
                cachedValue["isLoading"] = false;
                if (throwOnError)
                    throw promise.error;
                else
                    return;
            }
            else {
                promise.result.then(function (p) { return cachedValue["isLoading"] = false; }, function (p) { return cachedValue["isLoading"] = false; });
                return promise.result;
            }
        };
        if (!cachedValue.peek())
            cmp.refreshValue();
        ko.computed(function () { return refreshTriggers(args).map(function (f) { return typeof f == "string" ? dotvvm.eventHub.get(f)() : f(); }); }).subscribe(function (p) { return cmp.refreshValue(); });
        return cmp;
    };
    DotVVM.prototype.apiRefreshOn = function (value, refreshOn) {
        if (typeof value.refreshValue != "function")
            console.error("The object is not refreshable");
        refreshOn.subscribe(function () {
            if (typeof value.refreshValue != "function")
                console.error("The object is not refreshable");
            value.refreshValue && value.refreshValue();
        });
        return value;
    };
    DotVVM.prototype.api = {};
    DotVVM.prototype.eventHub = new DotvvmEventHub();
}());
//# sourceMappingURL=DotVVM.js.map