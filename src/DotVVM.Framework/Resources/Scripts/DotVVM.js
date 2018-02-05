var __assign = (this && this.__assign) || Object.assign || function(t) {
    for (var s, i = 1, n = arguments.length; i < n; i++) {
        s = arguments[i];
        for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
            t[p] = s[p];
    }
    return t;
};
var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
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
        this.handlers.push(handler);
        if (this.triggerMissedEventsOnSubscribe) {
            for (var i = 0; i < this.history.length; i++) {
                handler(history[i]);
            }
        }
    };
    DotvvmEvent.prototype.unsubscribe = function (handler) {
        var index = this.handlers.indexOf(handler);
        if (index >= 0) {
            this.handlers = this.handlers.splice(index, 1);
        }
    };
    DotvvmEvent.prototype.trigger = function (data) {
        for (var i = 0; i < this.handlers.length; i++) {
            this.handlers[i](data);
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
var DotvvmSpaNavigatingEventArgs = /** @class */ (function () {
    function DotvvmSpaNavigatingEventArgs(viewModel, viewModelName, newUrl) {
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.newUrl = newUrl;
        this.cancel = false;
    }
    return DotvvmSpaNavigatingEventArgs;
}());
var DotvvmSpaNavigatedEventArgs = /** @class */ (function () {
    function DotvvmSpaNavigatedEventArgs(viewModel, viewModelName, serverResponseObject, xhr) {
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.serverResponseObject = serverResponseObject;
        this.xhr = xhr;
        this.isHandled = false;
    }
    return DotvvmSpaNavigatedEventArgs;
}());
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
        var fileUpload = iframe.contentWindow.document.getElementById('upload');
        fileUpload.click();
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
    DotvvmGlobalize.prototype.format = function (format) {
        var _this = this;
        var values = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            values[_i - 1] = arguments[_i];
        }
        return format.replace(/\{([1-9]?[0-9]+)(:[^}])?\}/g, function (match, group0, group1) {
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
        if (value == null)
            return "";
        if (typeof value === "string") {
            // JSON date in string
            value = this.parseDotvvmDate(value);
        }
        if (format === "" || format === null) {
            format = "G";
        }
        return dotvvm_Globalize.format(value, format, dotvvm.culture);
    };
    DotvvmGlobalize.prototype.parseDotvvmDate = function (value) {
        var match = value.match("^([0-9]{4})-([0-9]{2})-([0-9]{2})T([0-9]{2}):([0-9]{2}):([0-9]{2})(\\.[0-9]{3,7})$");
        if (match) {
            return new Date(parseInt(match[1]), parseInt(match[2]) - 1, parseInt(match[3]), parseInt(match[4]), parseInt(match[5]), parseInt(match[6]), match.length > 7 ? parseInt(match[7].substring(1, 4)) : 0);
        }
        return null;
    };
    DotvvmGlobalize.prototype.parseNumber = function (value) {
        return dotvvm_Globalize.parseFloat(value, 10, dotvvm.culture);
    };
    DotvvmGlobalize.prototype.parseDate = function (value, format, previousValue) {
        return dotvvm_Globalize.parseDate(value, format, dotvvm.culture, previousValue);
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
                return dotvvm_Globalize.format(unwrappedVal, format, dotvvm.culture);
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
                write: function (val) { return setter_1(dotvvm_Globalize.parseDate(val, format, dotvvm.culture)); }
            });
        }
        else {
            return ko.pureComputed(function () { return formatDate(); });
        }
    };
    DotvvmGlobalize.prototype.bindingNumberToString = function (value, format) {
        var _this = this;
        if (format === void 0) { format = "G"; }
        if (!value) {
            return "";
        }
        var unwrapNumber = function () {
            var unwrappedVal = ko.unwrap(value);
            return typeof unwrappedVal == "string" ? _this.parseNumber(unwrappedVal) : unwrappedVal;
        };
        var formatNumber = function () {
            var unwrappedVal = unwrapNumber();
            if (unwrappedVal != null) {
                return dotvvm_Globalize.format(unwrappedVal, format, dotvvm.culture);
            }
            return "";
        };
        if (ko.isWriteableObservable(value)) {
            return ko.pureComputed({
                read: function () { return formatNumber(); },
                write: function (val) {
                    var parsedFloat = dotvvm_Globalize.parseFloat(val, 10, dotvvm.culture), isValid = val == null || (parsedFloat != null && !isNaN(parsedFloat));
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
var DotvvmSerialization = /** @class */ (function () {
    function DotvvmSerialization() {
    }
    DotvvmSerialization.prototype.deserialize = function (viewModel, target, deserializeAll) {
        if (deserializeAll === void 0) { deserializeAll = false; }
        if (typeof (viewModel) == "undefined" || viewModel == null) {
            if (ko.isObservable(target)) {
                target(viewModel);
            }
            return viewModel;
        }
        if (typeof (viewModel) == "string" || typeof (viewModel) == "number" || typeof (viewModel) == "boolean") {
            if (ko.isObservable(target)) {
                target(viewModel);
            }
            return viewModel;
        }
        if (viewModel instanceof Date) {
            viewModel = dotvvm.serialization.serializeDate(viewModel);
            if (ko.isObservable(target)) {
                target(viewModel);
            }
            return viewModel;
        }
        // handle arrays
        if (viewModel instanceof Array) {
            if (ko.isObservable(target) && "removeAll" in target && target() != null && target().length === viewModel.length) {
                // the array has the same number of items, update it
                var targetArray = target();
                for (var i = 0; i < viewModel.length; i++) {
                    var targetItem = targetArray[i]();
                    var deserialized = this.deserialize(viewModel[i], targetItem, deserializeAll);
                    if (targetItem !== deserialized) {
                        // update the observable only if the item has changed
                        targetArray[i](deserialized);
                    }
                }
            }
            else {
                // rebuild the array because it is different
                var array = [];
                for (var i = 0; i < viewModel.length; i++) {
                    array.push(this.wrapObservable(this.deserialize(viewModel[i], {}, deserializeAll)));
                }
                if (ko.isObservable(target)) {
                    if (!("removeAll" in target)) {
                        // if the previous value was null, the property is not an observable array - make it
                        ko.utils.extend(target, ko.observableArray['fn']);
                        target = target.extend({ 'trackArrayChanges': true });
                    }
                    target(array);
                }
                else {
                    target = ko.observableArray(array);
                }
            }
            return target;
        }
        // handle objects
        if (typeof (target) === "undefined") {
            target = {};
        }
        var result = ko.unwrap(target);
        var updateTarget = false;
        if (result == null) {
            result = {};
            if (ko.isObservable(target)) {
                updateTarget = true;
            }
            else {
                target = result;
            }
        }
        for (var prop in viewModel) {
            if (viewModel.hasOwnProperty(prop) && !/\$options$/.test(prop)) {
                var value = viewModel[prop];
                if (typeof (value) === "undefined") {
                    continue;
                }
                if (!ko.isObservable(value) && typeof (value) === "function") {
                    continue;
                }
                var options = viewModel[prop + "$options"];
                if (!deserializeAll && options && options.doNotUpdate) {
                    continue;
                }
                // deserialize value
                var deserialized = ko.isObservable(value) ? value : this.deserialize(value, result[prop], deserializeAll);
                if (value instanceof Date) {
                    // if we get Date value from API, it was converted to string, but we should note that it was date to convert it back
                    result[prop + "$options"] = result[prop + "$options"] || {};
                    result[prop + "$options"].isDate = true;
                }
                // update the property
                if (ko.isObservable(deserialized)) {
                    if (ko.isObservable(result[prop])) {
                        if (deserialized() !== result[prop]()) {
                            result[prop](deserialized());
                        }
                    }
                    else {
                        var unwrapped = ko.unwrap(deserialized);
                        result[prop] = Array.isArray(unwrapped) ? ko.observableArray(unwrapped) : ko.observable(unwrapped); // don't reuse the same observable from the source
                    }
                }
                else {
                    if (ko.isObservable(result[prop])) {
                        if (deserialized !== result[prop]())
                            result[prop](deserialized);
                    }
                    else {
                        result[prop] = ko.observable(deserialized);
                    }
                }
                if (options && options.clientExtenders && ko.isObservable(result[prop])) {
                    for (var j = 0; j < options.clientExtenders.length; j++) {
                        var extenderOptions = {};
                        var extenderInfo = options.clientExtenders[j];
                        extenderOptions[extenderInfo.name] = extenderInfo.parameter;
                        result[prop].extend(extenderOptions);
                    }
                }
            }
        }
        // copy the property options metadata
        for (var prop in viewModel) {
            if (viewModel.hasOwnProperty(prop) && /\$options$/.test(prop)) {
                result[prop] = result[prop] || {};
                for (var optProp in viewModel[prop]) {
                    if (viewModel[prop].hasOwnProperty(optProp)) {
                        result[prop][optProp] = viewModel[prop][optProp];
                    }
                }
                var originalName = prop.substring(0, prop.length - "$options".length);
                if (typeof result[originalName] === "undefined") {
                    result[originalName] = ko.observable();
                }
            }
        }
        if (updateTarget) {
            target(result);
        }
        return target;
    };
    DotvvmSerialization.prototype.wrapObservable = function (obj) {
        if (!ko.isObservable(obj))
            return ko.observable(obj);
        return obj;
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
                minValue = -((maxValue / 2) | 0);
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
        this.resourceSigns = {};
        this.isViewModelUpdating = true;
        // warning this property is referenced in ModelState.cs and KnockoutHelper.cs
        this.viewModelObservables = {};
        this.isSpaReady = ko.observable(false);
        this.viewModels = {};
        this.serialization = new DotvvmSerialization();
        this.postbackHandlers = {
            confirm: function (options) { return new ConfirmPostBackHandler(options.message); },
            timeout: function (options) { return options.time ? _this.createWindowSetTimeoutHandler(options.time) : _this.windowSetTimeoutHandler; },
            "concurrency-none": function (o) { return ({
                name: "concurrency-none",
                before: ["setIsPostackRunning"],
                execute: function (callback, options) {
                    return _this.commonConcurrencyHandler(callback(), options, o.q || "default");
                }
            }); },
            "concurrency-deny": function (o) { return ({
                name: "concurrency-deny",
                before: ["setIsPostackRunning"],
                execute: function (callback, options) {
                    var queue = o.q || "default";
                    if (dotvvm.getPostbackQueue(queue).noRunning > 0)
                        return Promise.reject({ type: "handler", handler: this, message: "An postback is already running" });
                    return dotvvm.commonConcurrencyHandler(callback(), options, queue);
                }
            }); },
            "concurrency-queue": function (o) { return ({
                name: "concurrency-queue",
                before: ["setIsPostackRunning"],
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
                before: ["setIsPostackRunning", "concurrency-none", "concurrency-queue", "concurrency-deny"],
                execute: function (callback, options) {
                    if (dotvvm.isViewModelUpdating)
                        return Promise.reject({ type: "handler", handler: this, message: "ViewModel is updating, so it's probably false onchange event" });
                    else
                        return callback();
                }
            }); }
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
            var dispatchNext = function () {
                queue.noRunning--;
                if (queue.queue.length > 0) {
                    var callback = queue.queue.shift();
                    window.setTimeout(callback, 0);
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
                dispatchNext();
                return Promise.reject(error);
            });
        };
        this.defaultConcurrencyPostbackHandler = this.postbackHandlers["concurrency-none"]({});
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
        this.globalPostbackHandlers = [this.isPostBackRunningHandler, this.postbackHandlersStartedEventHandler];
        this.globalLaterPostbackHandlers = [this.postbackHandlersCompletedEventHandler, this.beforePostbackEventPostbackHandler];
        this.events = new DotvvmEvents();
        this.globalize = new DotvvmGlobalize();
        this.evaluator = new DotvvmEvaluator();
        this.domUtils = new DotvvmDomUtils();
        this.fileUpload = new DotvvmFileUpload();
        this.extensions = {};
        this.isPostbackRunning = ko.observable(false);
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
            this.domUtils.attachEvent(window, "hashchange", function () { return _this.handleHashChange(viewModelName, spaPlaceHolder, false); });
            this.handleHashChange(viewModelName, spaPlaceHolder, true);
        }
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
    DotVVM.prototype.staticCommandPostback = function (viewModelName, sender, command, args, callback, errorCallback) {
        var _this = this;
        if (callback === void 0) { callback = function (_) { }; }
        if (errorCallback === void 0) { errorCallback = function (xhr, error) { }; }
        if (this.isPostBackProhibited(sender))
            return;
        var data = this.serialization.serialize({
            "args": args,
            "command": command,
            "$csrfToken": this.viewModels[viewModelName].viewModel.$csrfToken
        });
        dotvvm.events.staticCommandMethodInvoking.trigger(data);
        this.postJSON(this.viewModels[viewModelName].url, "POST", ko.toJSON(data), function (response) {
            try {
                _this.isViewModelUpdating = true;
                var result = JSON.parse(response.responseText);
                dotvvm.events.staticCommandMethodInvoked.trigger(__assign({}, data, { result: result }));
                callback(result);
            }
            catch (error) {
                dotvvm.events.staticCommandMethodFailed.trigger(__assign({}, data, { xhr: response, error: error }));
                errorCallback(response, error);
            }
            finally {
                _this.isViewModelUpdating = false;
            }
        }, function (xhr) {
            console.warn("StaticCommand postback failed: " + xhr.status + " - " + xhr.statusText, xhr);
            errorCallback(xhr);
            dotvvm.events.staticCommandMethodFailed.trigger(__assign({}, data, { xhr: xhr }));
        }, function (xhr) {
            xhr.setRequestHeader("X-PostbackType", "StaticCommand");
        });
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
            for (var _i = 0, handlers_1 = handlers; _i < handlers_1.length; _i++) {
                var h = handlers_1[_i];
                if (h.name != null) {
                    handlerMap[h.name] = h;
                }
            }
            return function (s) { return typeof s == "string" ? handlerMap[s] : s; };
        })();
        var dependencies = handlers.map(function (handler, i) { return (handler["@sort_index"] = i, ({ handler: handler, deps: (handler.after || []).map(getHandler) })); });
        for (var _i = 0, handlers_2 = handlers; _i < handlers_2.length; _i++) {
            var h = handlers_2[_i];
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
        var options = new PostbackOptions(this.backUpPostBackConter(), sender, args, viewModel, viewModelName);
        var promise = this.applyPostbackHandlersCore(callback, options, this.findPostbackHandlers(context, this.globalPostbackHandlers.concat(handlers || []).concat(this.globalLaterPostbackHandlers)))
            .then(function (r) { return r(); }, function (r) { return Promise.reject(r); });
        promise.catch(function (reason) { if (reason)
            console.log("Rejected: " + reason); });
        return promise;
    };
    DotVVM.prototype.postbackCore = function (options, path, command, controlUniqueId, context, commandArgs) {
        var _this = this;
        return new Promise(function (resolve, reject) {
            var viewModelName = options.viewModelName;
            var viewModel = _this.viewModels[viewModelName].viewModel;
            _this.lastStartedPostack = options.postbackId;
            // perform the postback
            _this.updateDynamicPathFragments(context, path);
            var data = {
                viewModel: _this.serialization.serialize(viewModel, { pathMatcher: function (val) { return context && val == context.$data; } }),
                currentPath: path,
                command: command,
                controlUniqueId: _this.processPassedId(controlUniqueId, context),
                additionalData: options.additionalPostbackData,
                renderedResources: _this.viewModels[viewModelName].renderedResources,
                commandArgs: commandArgs
            };
            _this.postJSON(_this.viewModels[viewModelName].url, "POST", ko.toJSON(data), function (result) {
                dotvvm.events.postbackResponseReceived.trigger({});
                resolve(function () { return new Promise(function (resolve, reject) {
                    dotvvm.events.postbackCommitInvoked.trigger({});
                    var locationHeader = result.getResponseHeader("Location");
                    var resultObject = locationHeader != null && locationHeader.length > 0 ?
                        { action: "redirect", url: locationHeader } :
                        JSON.parse(result.responseText);
                    if (!resultObject.viewModel && resultObject.viewModelDiff) {
                        // TODO: patch (~deserialize) it to ko.observable viewModel
                        resultObject.viewModel = _this.patch(data.viewModel, resultObject.viewModelDiff);
                    }
                    _this.loadResourceList(resultObject.resources, function () {
                        var isSuccess = false;
                        if (resultObject.action === "successfulCommand") {
                            try {
                                _this.isViewModelUpdating = true;
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
                            _this.handleRedirect(resultObject, viewModelName);
                            return resolve();
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
                            reject(new DotvvmErrorEventArgs(options.sender, viewModel, viewModelName, result, options.postbackId, resultObject));
                        }
                        else {
                            var afterPostBackArgs = new DotvvmAfterPostBackEventArgs(options, resultObject, resultObject.commandResult, result);
                            resolve(afterPostBackArgs);
                        }
                    });
                }); });
            }, function (xhr) {
                reject({ type: 'network', options: options, args: new DotvvmErrorEventArgs(options.sender, viewModel, viewModelName, xhr, options.postbackId) });
            });
        });
    };
    DotVVM.prototype.postBack = function (viewModelName, sender, path, command, controlUniqueId, context, handlers, commandArgs) {
        var _this = this;
        if (this.isPostBackProhibited(sender)) {
            var rejectedPromise = new Promise(function (resolve, reject) { return reject("rejected"); });
            rejectedPromise.catch(function () { return console.log("Postback probihited"); });
            return rejectedPromise;
        }
        context = context || ko.contextFor(sender);
        var preparedHandlers = this.findPostbackHandlers(context, this.globalPostbackHandlers.concat(handlers || []).concat(this.globalLaterPostbackHandlers));
        if (preparedHandlers.filter(function (h) { return h.name && h.name.indexOf("concurrency-") == 0; }).length == 0) {
            // add a default concurrency handler if none is specthis.globalPostbackHandlers.concat(handlers || []).concat(this.globalLaterPostbackHandlers)ified
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
        var el = elements[offset];
        var waitForScriptLoaded = false;
        if (el.tagName.toLowerCase() == "script") {
            // create the script element
            var script = document.createElement("script");
            if (el.src) {
                script.src = el.src;
                waitForScriptLoaded = true;
            }
            if (el.type) {
                script.type = el.type;
            }
            if (el.text) {
                script.text = el.text;
            }
            el = script;
        }
        else if (el.tagName.toLowerCase() == "link") {
            // create link
            var link = document.createElement("link");
            if (el.href) {
                link.href = el.href;
            }
            if (el.rel) {
                link.rel = el.rel;
            }
            if (el.type) {
                link.type = el.type;
            }
            el = link;
        }
        // load next script when this is finished
        if (waitForScriptLoaded) {
            el.onload = function () { return _this.loadResourceElements(elements, offset + 1, callback); };
        }
        document.head.appendChild(el);
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
    DotVVM.prototype.navigateCore = function (viewModelName, url) {
        var _this = this;
        var viewModel = this.viewModels[viewModelName].viewModel;
        // prevent double postbacks
        var currentPostBackCounter = this.backUpPostBackConter();
        // trigger spaNavigating event
        var spaNavigatingArgs = new DotvvmSpaNavigatingEventArgs(viewModel, viewModelName, url);
        this.events.spaNavigating.trigger(spaNavigatingArgs);
        if (spaNavigatingArgs.cancel) {
            return;
        }
        // add virtual directory prefix
        url = "/___dotvvm-spa___" + this.addLeadingSlash(url);
        var fullUrl = this.addLeadingSlash(this.concatUrl(this.viewModels[viewModelName].virtualDirectory || "", url));
        // find SPA placeholder
        var spaPlaceHolder = this.getSpaPlaceHolder();
        if (!spaPlaceHolder) {
            document.location.href = fullUrl;
            return;
        }
        // send the request
        var spaPlaceHolderUniqueId = spaPlaceHolder.attributes["data-dotvvm-spacontentplaceholder"].value;
        this.getJSON(fullUrl, "GET", spaPlaceHolderUniqueId, function (result) {
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
                    _this.handleRedirect(resultObject, viewModelName, true);
                    return;
                }
                // trigger spaNavigated event
                var spaNavigatedArgs = new DotvvmSpaNavigatedEventArgs(viewModel, viewModelName, resultObject, result);
                _this.events.spaNavigated.trigger(spaNavigatedArgs);
                if (!isSuccess && !spaNavigatedArgs.isHandled) {
                    throw "Invalid response from server!";
                }
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
        });
    };
    DotVVM.prototype.handleRedirect = function (resultObject, viewModelName, replace) {
        if (replace === void 0) { replace = false; }
        if (resultObject.replace != null)
            replace = resultObject.replace;
        var url;
        // redirect
        if (this.getSpaPlaceHolder() && resultObject.url.indexOf("//") < 0 && resultObject.allowSpa) {
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
        this.performRedirect(url, replace);
    };
    DotVVM.prototype.performRedirect = function (url, replace) {
        if (replace) {
            location.replace(url);
        }
        else {
            var fakeAnchor = this.fakeRedirectAnchor;
            if (!fakeAnchor) {
                fakeAnchor = document.createElement("a");
                fakeAnchor.style.display = "none";
                fakeAnchor.setAttribute("data-dotvvm-fake-id", "dotvvm_fake_redirect_anchor_87D7145D_8EA8_47BA_9941_82B75EE88CDB");
                document.body.appendChild(fakeAnchor);
                this.fakeRedirectAnchor = fakeAnchor;
            }
            fakeAnchor.href = url;
            fakeAnchor.click();
        }
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
        else if (typeof source == "object" && typeof patch == "object") {
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
            if (xhr.status < 400) {
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
            if (xhr.status < 400) {
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
        if (element && element.tagName && element.tagName.toLowerCase() === "a" && element.getAttribute("disabled")) {
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
                    value[prop] = createWrapperComputed(function () { return valueAccessor()[this.prop]; }.bind({ prop: prop }), "'" + prop + "' at '" + valueAccessor.toString() + "'");
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
        ko.virtualElements.allowedBindings["withGridViewDataSet"] = true;
        ko.bindingHandlers["withGridViewDataSet"] = {
            init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                if (!bindingContext)
                    throw new Error();
                var value = valueAccessor();
                var innerBindingContext = bindingContext.extend({ $gridViewDataSet: value });
                element.innerBindingContext = innerBindingContext;
                ko.applyBindingsToDescendants(innerBindingContext, element);
                return { controlsDescendantBindings: true }; // do not apply binding again
            },
            update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            }
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
        ko.bindingHandlers["dotvvm-UpdateProgress-Visible"] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                element.style.display = "none";
                var delay = element.getAttribute("data-delay");
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
                dotvvm.isPostbackRunning.subscribe(function (e) {
                    if (e) {
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
                        var row = table.rows.item(i);
                        var style = row.cells[columnIndex].style;
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
                var colIndex = [].slice.call(table.rows.item(0).cells).indexOf(element);
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
                }, 0, obs.dotvvmMetadata.elementsMetadata, element);
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
        if (!context.parameters[0] && context.valueToValidate == null) {
            valid = false;
        }
        if (!context.parameters[1] && context.valueToValidate.length === 0) {
            valid = false;
        }
        if (!context.parameters[2] && this.isEmpty(context.valueToValidate)) {
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
var ValidationError = /** @class */ (function () {
    function ValidationError(validatedObservable, errorMessage) {
        this.validatedObservable = validatedObservable;
        this.errorMessage = errorMessage;
    }
    ValidationError.getOrCreate = function (validatedObservable) {
        if (validatedObservable.wrappedProperty) {
            var wrapped = validatedObservable.wrappedProperty();
            if (ko.isObservable(wrapped))
                validatedObservable = wrapped;
        }
        if (!validatedObservable.validationErrors) {
            validatedObservable.validationErrors = ko.observableArray();
        }
        return validatedObservable.validationErrors;
    };
    ValidationError.isValid = function (validatedObservable) {
        return !validatedObservable.validationErrors || validatedObservable.validationErrors().length === 0;
    };
    ValidationError.prototype.clear = function (validation) {
        var localErrors = this.validatedObservable.validationErrors;
        localErrors.remove(this);
        validation.errors.remove(this);
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
            "notnull": new DotvvmNotNullValidator(),
            "enforceClientFormat": new DotvvmEnforceClientFormatValidator()
        };
        this.errors = ko.observableArray([]);
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
                if (errorMessages.length > 0) {
                    element.className += " " + className;
                }
                else {
                    element.className = element.className.split(' ').filter(function (c) { return c != className; }).join(' ');
                }
            },
            // sets the error message as the title attribute
            setToolTipText: function (element, errorMessages, param) {
                if (errorMessages.length > 0) {
                    element.title = errorMessages.join(", ");
                }
                else {
                    element.title = "";
                }
            },
            // displays the error message
            showErrorMessageText: function (element, errorMessages, param) {
                element[element.innerText ? "innerText" : "textContent"] = errorMessages.join(", ");
            }
        };
        var createValidationHandler = function (path) { return ({
            execute: function (callback, options) {
                if (path) {
                    options.additionalPostbackData.validationTargetPath = path;
                    // resolve target
                    var context = ko.contextFor(options.sender);
                    var validationTarget = dotvvm.evaluator.evaluateOnViewModel(context, path);
                    // validate the object
                    _this.clearValidationErrors(dotvvm.viewModelObservables[options.viewModelName || 'root']);
                    _this.validateViewModel(validationTarget);
                    if (_this.errors().length > 0) {
                        console.log("Validation failed: postback aborted; errors: ", _this.errors());
                        return Promise.reject({ type: "handler", handler: _this, message: "Validation failed" });
                    }
                    _this.events.validationErrorsChanged.trigger({ viewModel: options.viewModel });
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
                    _this.showValidationErrorsFromServer(args);
                    args.isHandled = true;
                }
            }
            _this.events.validationErrorsChanged.trigger(args);
        });
        dotvvm.events.spaNavigating.subscribe(function (args) {
            _this.clearValidationErrors(dotvvm.viewModelObservables[args.viewModelName]);
        });
        // add knockout binding handler
        ko.bindingHandlers["dotvvmValidation"] = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                var observableProperty = valueAccessor();
                if (ko.isObservable(observableProperty)) {
                    // try to get the options
                    var options = allBindingsAccessor.get("dotvvmValidationOptions");
                    var updateFunction = function (element, errorMessages) {
                        for (var option in options) {
                            if (options.hasOwnProperty(option)) {
                                _this.elementUpdateFunctions[option](element, errorMessages.map(function (v) { return v.errorMessage; }), options[option]);
                            }
                        }
                    };
                    // subscribe to the observable property changes
                    var validationErrors = ValidationError.getOrCreate(observableProperty);
                    validationErrors.subscribe(function (newValue) { return updateFunction(element, newValue); });
                    updateFunction(element, validationErrors());
                }
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
        if (!viewModel || !dotvvm.viewModels['root'].validationRules)
            return;
        // find validation rules
        var type = ko.unwrap(viewModel.$type);
        if (!type)
            return;
        var rulesForType = dotvvm.viewModels['root'].validationRules[type] || {};
        // validate all properties
        for (var property in viewModel) {
            if (!viewModel.hasOwnProperty(property) || property.indexOf("$") === 0)
                continue;
            var viewModelProperty = viewModel[property];
            if (!viewModelProperty || !ko.isObservable(viewModelProperty))
                continue;
            var value = viewModel[property]();
            // run validation rules
            if (rulesForType.hasOwnProperty(property)) {
                this.validateProperty(viewModel, viewModelProperty, value, rulesForType[property]);
            }
            var options = viewModel[property + "$options"];
            if (options && options.type && ValidationError.isValid(viewModelProperty) && !dotvvm.serialization.validateType(value, options.type)) {
                var error = new ValidationError(viewModelProperty, "The value of property " + property + " (" + value + ") is invalid value for type " + options.type + ".");
                this.addValidationError(viewModelProperty, error);
            }
            if (value) {
                if (Array.isArray(value)) {
                    // handle collections
                    for (var _i = 0, value_1 = value; _i < value_1.length; _i++) {
                        var item = value_1[_i];
                        this.validateViewModel(item);
                    }
                }
                else if (value.$type) {
                    // handle nested objects
                    this.validateViewModel(value);
                }
            }
        }
    };
    // validates the specified property in the viewModel
    DotvvmValidation.prototype.validateProperty = function (viewModel, property, value, rulesForProperty) {
        for (var _i = 0, rulesForProperty_1 = rulesForProperty; _i < rulesForProperty_1.length; _i++) {
            var rule = rulesForProperty_1[_i];
            // validate the rules
            var ruleTemplate = this.rules[rule.ruleName];
            var context = new DotvvmValidationContext(value, viewModel, rule.parameters);
            if (!ruleTemplate.isValid(context, property)) {
                var validationErrors = ValidationError.getOrCreate(property);
                // add error message
                var validationError = new ValidationError(property, rule.errorMessage);
                this.addValidationError(property, validationError);
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
    DotvvmValidation.prototype.clearValidationErrors = function (validatedObservable) {
        if (!validatedObservable || !ko.isObservable(validatedObservable))
            return;
        if (validatedObservable.validationErrors) {
            for (var _i = 0, _a = validatedObservable.validationErrors(); _i < _a.length; _i++) {
                var error = _a[_i];
                error.clear(this);
            }
        }
        var validatedObject = validatedObservable();
        if (!validatedObject)
            return;
        // Do the same for every object in the array
        if (Array.isArray(validatedObject)) {
            for (var _b = 0, validatedObject_1 = validatedObject; _b < validatedObject_1.length; _b++) {
                var item = validatedObject_1[_b];
                this.clearValidationErrors(item);
            }
        }
        // Do the same for every subordinate property
        for (var propertyName in validatedObject) {
            if (!validatedObject.hasOwnProperty(propertyName) || propertyName.indexOf("$") === 0)
                continue;
            var property = validatedObject[propertyName];
            this.clearValidationErrors(property);
        }
    };
    /**
     * Gets validation errors from the passed object and its children.
     * @param target Object that is supposed to contain the errors or properties with the errors
     * @param includeErrorsFromGrandChildren Is called "IncludeErrorsFromChildren" in ValidationSummary.cs
     * @param includeErrorsFromChildren Sets whether to include errors from children at all
     * @returns By default returns only errors from the viewModel's immediate children
     */
    DotvvmValidation.prototype.getValidationErrors = function (validationTargetObservable, includeErrorsFromGrandChildren, includeErrorsFromTarget, includeErrorsFromChildren) {
        if (includeErrorsFromChildren === void 0) { includeErrorsFromChildren = true; }
        // Check the passed viewModel
        if (!validationTargetObservable)
            return [];
        var errors = [];
        // Include errors from the validation target
        if (includeErrorsFromTarget) {
            errors = errors.concat(ValidationError.getOrCreate(validationTargetObservable)());
        }
        if (includeErrorsFromChildren) {
            var validationTarget = ko.unwrap(validationTargetObservable);
            if (Array.isArray(validationTarget)) {
                for (var _i = 0, validationTarget_1 = validationTarget; _i < validationTarget_1.length; _i++) {
                    var item = validationTarget_1[_i];
                    // This is correct because in the next children and further all children are grandchildren
                    errors = errors.concat(this.getValidationErrors(item, includeErrorsFromGrandChildren, true, includeErrorsFromGrandChildren));
                }
            }
            else {
                for (var propertyName in validationTarget) {
                    if (!validationTarget.hasOwnProperty(propertyName) || propertyName.indexOf("$") === 0)
                        continue;
                    var property = validationTarget[propertyName];
                    if (!property || !ko.isObservable(property))
                        continue;
                    // Nested properties are children too
                    errors = errors.concat(this.getValidationErrors(property, includeErrorsFromGrandChildren, true, includeErrorsFromGrandChildren));
                }
            }
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
        if (!validationTarget)
            return;
        // add validation errors
        this.clearValidationErrors(dotvvm.viewModelObservables[args.viewModelName]);
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
            // add the error to appropriate collections
            var error = new ValidationError(property, modelState[i].errorMessage);
            this.addValidationError(property, error);
        }
    };
    DotvvmValidation.prototype.addValidationError = function (validatedProperty, error) {
        var errors = ValidationError.getOrCreate(validatedProperty);
        if (errors.indexOf(error) < 0) {
            validatedProperty.validationErrors.push(error);
            this.errors.push(error);
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
    DotvvmEvaluator.prototype.wrapKnockoutExpression = function (func) {
        var _this = this;
        var wrapper;
        var result = this.getExpressionResult(func), isWriteableObservable = ko.isWriteableObservable(result), isObservableArray = this.isObservableArray(result);
        if (isWriteableObservable) {
            wrapper = ko.pureComputed({
                read: function () { return ko.unwrap(_this.getExpressionResult(func)); },
                write: function (value) { return _this.updateObservable(func, value); }
            });
            if (isObservableArray) {
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
            }
        }
        else {
            wrapper = ko.pureComputed(function () { return ko.unwrap(_this.getExpressionResult(func)); });
        }
        if (isObservableArray) {
            wrapper = wrapper.extend({ trackArrayChanges: true }); // properly track changes in wrapped arrays
        }
        return wrapper.extend({ notify: "always" });
    };
    DotvvmEvaluator.prototype.updateObservable = function (getObservable, value) {
        var result = this.getExpressionResult(getObservable);
        if (!ko.isWriteableObservable(result)) {
            throw Error("Cannot write a value to ko.computed because the expression '" + getObservable + "' does not return a writable observable.");
        }
        result(value);
    };
    DotvvmEvaluator.prototype.updateObservableArray = function (getObservableArray, fnName, args) {
        var result = this.getExpressionResult(getObservableArray);
        if (!this.isObservableArray(result)) {
            throw Error("Cannot execute '" + fnName + "' function on ko.computed because the '" + getObservableArray + "' does not return an observable array.");
        }
        result[fnName].apply(result, args);
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
    DotVVM.prototype.invokeApiFn = function (callback, refreshTriggers, notifyTriggers, commandId) {
        if (refreshTriggers === void 0) { refreshTriggers = []; }
        if (notifyTriggers === void 0) { notifyTriggers = []; }
        if (commandId === void 0) { commandId = callback.toString(); }
        var cachedValue = cachedValues[commandId] || (cachedValues[commandId] = ko.observable(null));
        var load = function () {
            try {
                var result = window["Promise"].resolve(ko.ignoreDependencies(callback));
                return { type: 'result', result: result.then(function (val) {
                        if (val) {
                            cachedValue(ko.unwrap(dotvvm.serialization.deserialize(val, cachedValue)));
                            cachedValue.notifySubscribers();
                        }
                        for (var _i = 0, notifyTriggers_1 = notifyTriggers; _i < notifyTriggers_1.length; _i++) {
                            var t = notifyTriggers_1[_i];
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
        ko.computed(function () { return refreshTriggers.map(function (f) { return typeof f == "string" ? dotvvm.eventHub.get(f)() : f(); }); }).subscribe(function (p) { return cmp.refreshValue(); });
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