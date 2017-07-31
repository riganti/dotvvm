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
var DotvvmDomUtils = (function () {
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
var DotvvmEvents = (function () {
    function DotvvmEvents() {
        this.init = new DotvvmEvent("dotvvm.events.init", true);
        this.beforePostback = new DotvvmEvent("dotvvm.events.beforePostback");
        this.afterPostback = new DotvvmEvent("dotvvm.events.afterPostback");
        this.error = new DotvvmEvent("dotvvm.events.error");
        this.spaNavigating = new DotvvmEvent("dotvvm.events.spaNavigating");
        this.spaNavigated = new DotvvmEvent("dotvvm.events.spaNavigated");
        this.redirect = new DotvvmEvent("dotvvm.events.redirect");
    }
    return DotvvmEvents;
}());
// DotvvmEvent is used because CustomEvent is not browser compatible and does not support 
// calling missed events for handler that subscribed too late.
var DotvvmEvent = (function () {
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
var DotvvmEventArgs = (function () {
    function DotvvmEventArgs(viewModel) {
        this.viewModel = viewModel;
    }
    return DotvvmEventArgs;
}());
var DotvvmErrorEventArgs = (function (_super) {
    __extends(DotvvmErrorEventArgs, _super);
    function DotvvmErrorEventArgs(viewModel, xhr, isSpaNavigationError) {
        if (isSpaNavigationError === void 0) { isSpaNavigationError = false; }
        var _this = _super.call(this, viewModel) || this;
        _this.viewModel = viewModel;
        _this.xhr = xhr;
        _this.isSpaNavigationError = isSpaNavigationError;
        _this.handled = false;
        return _this;
    }
    return DotvvmErrorEventArgs;
}(DotvvmEventArgs));
var DotvvmBeforePostBackEventArgs = (function (_super) {
    __extends(DotvvmBeforePostBackEventArgs, _super);
    function DotvvmBeforePostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, postbackClientId) {
        var _this = _super.call(this, viewModel) || this;
        _this.sender = sender;
        _this.viewModel = viewModel;
        _this.viewModelName = viewModelName;
        _this.validationTargetPath = validationTargetPath;
        _this.postbackClientId = postbackClientId;
        _this.cancel = false;
        _this.clientValidationFailed = false;
        return _this;
    }
    return DotvvmBeforePostBackEventArgs;
}(DotvvmEventArgs));
var DotvvmAfterPostBackEventArgs = (function (_super) {
    __extends(DotvvmAfterPostBackEventArgs, _super);
    function DotvvmAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, serverResponseObject, postbackClientId) {
        var _this = _super.call(this, viewModel) || this;
        _this.sender = sender;
        _this.viewModel = viewModel;
        _this.viewModelName = viewModelName;
        _this.validationTargetPath = validationTargetPath;
        _this.serverResponseObject = serverResponseObject;
        _this.postbackClientId = postbackClientId;
        _this.isHandled = false;
        _this.wasInterrupted = false;
        return _this;
    }
    return DotvvmAfterPostBackEventArgs;
}(DotvvmEventArgs));
var DotvvmSpaNavigatingEventArgs = (function (_super) {
    __extends(DotvvmSpaNavigatingEventArgs, _super);
    function DotvvmSpaNavigatingEventArgs(viewModel, viewModelName, newUrl) {
        var _this = _super.call(this, viewModel) || this;
        _this.viewModel = viewModel;
        _this.viewModelName = viewModelName;
        _this.newUrl = newUrl;
        _this.cancel = false;
        return _this;
    }
    return DotvvmSpaNavigatingEventArgs;
}(DotvvmEventArgs));
var DotvvmSpaNavigatedEventArgs = (function (_super) {
    __extends(DotvvmSpaNavigatedEventArgs, _super);
    function DotvvmSpaNavigatedEventArgs(viewModel, viewModelName, serverResponseObject) {
        var _this = _super.call(this, viewModel) || this;
        _this.viewModel = viewModel;
        _this.viewModelName = viewModelName;
        _this.serverResponseObject = serverResponseObject;
        _this.isHandled = false;
        return _this;
    }
    return DotvvmSpaNavigatedEventArgs;
}(DotvvmEventArgs));
var DotvvmRedirectEventArgs = (function (_super) {
    __extends(DotvvmRedirectEventArgs, _super);
    function DotvvmRedirectEventArgs(viewModel, viewModelName, url, replace) {
        var _this = _super.call(this, viewModel) || this;
        _this.viewModel = viewModel;
        _this.viewModelName = viewModelName;
        _this.url = url;
        _this.replace = replace;
        _this.isHandled = false;
        return _this;
    }
    return DotvvmRedirectEventArgs;
}(DotvvmEventArgs));
var DotvvmEvaluator = (function () {
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
    return DotvvmEvaluator;
}());
var DotvvmGlobalize = (function () {
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
    return DotvvmGlobalize;
}());
var DotvvmPostBackHandler = (function () {
    function DotvvmPostBackHandler() {
    }
    DotvvmPostBackHandler.prototype.execute = function (callback, sender) {
    };
    return DotvvmPostBackHandler;
}());
var ConfirmPostBackHandler = (function (_super) {
    __extends(ConfirmPostBackHandler, _super);
    function ConfirmPostBackHandler(message) {
        var _this = _super.call(this) || this;
        _this.message = message;
        return _this;
    }
    ConfirmPostBackHandler.prototype.execute = function (callback, sender) {
        if (confirm(this.message)) {
            callback();
        }
    };
    return ConfirmPostBackHandler;
}(DotvvmPostBackHandler));
var DotvvmPostBackHandlers = (function () {
    function DotvvmPostBackHandlers() {
        this.confirm = function (options) { return new ConfirmPostBackHandler(options.message); };
    }
    return DotvvmPostBackHandlers;
}());
var DotvvmPromiseState;
(function (DotvvmPromiseState) {
    DotvvmPromiseState[DotvvmPromiseState["Pending"] = 0] = "Pending";
    DotvvmPromiseState[DotvvmPromiseState["Done"] = 1] = "Done";
    DotvvmPromiseState[DotvvmPromiseState["Failed"] = 2] = "Failed";
})(DotvvmPromiseState || (DotvvmPromiseState = {}));
var DotvvmPromise = (function () {
    function DotvvmPromise() {
        this.callbacks = [];
        this.errorCallbacks = [];
        this.state = DotvvmPromiseState.Pending;
    }
    DotvvmPromise.prototype.done = function (callback, forceAsync) {
        var _this = this;
        if (forceAsync === void 0) { forceAsync = false; }
        if (this.state === DotvvmPromiseState.Done) {
            if (forceAsync)
                setTimeout(function () { return callback(_this.argument); }, 4);
            else
                callback(this.argument);
        }
        else if (this.state === DotvvmPromiseState.Pending) {
            this.callbacks.push(callback);
        }
    };
    DotvvmPromise.prototype.fail = function (callback, forceAsync) {
        var _this = this;
        if (forceAsync === void 0) { forceAsync = false; }
        if (this.state === DotvvmPromiseState.Failed) {
            if (forceAsync)
                setTimeout(function () { return callback(_this.error); }, 4);
            else
                callback(this.error);
        }
        else if (this.state === DotvvmPromiseState.Pending) {
            this.errorCallbacks.push(callback);
        }
        return this;
    };
    DotvvmPromise.prototype.resolve = function (arg) {
        if (this.state !== DotvvmPromiseState.Pending)
            throw new Error("Can not resolve " + this.state + " promise.");
        this.state = DotvvmPromiseState.Done;
        this.argument = arg;
        for (var _i = 0, _a = this.callbacks; _i < _a.length; _i++) {
            var c = _a[_i];
            c(arg);
        }
        this.callbacks = null;
        this.errorCallbacks = null;
        return this;
    };
    DotvvmPromise.prototype.reject = function (error) {
        if (this.state != DotvvmPromiseState.Pending)
            throw new Error("Can not reject " + this.state + " promise.");
        this.state = DotvvmPromiseState.Failed;
        this.error = error;
        for (var _i = 0, _a = this.errorCallbacks; _i < _a.length; _i++) {
            var c = _a[_i];
            c(error);
        }
        this.callbacks = null;
        this.errorCallbacks = null;
        return this;
    };
    DotvvmPromise.prototype.chainFrom = function (promise) {
        var _this = this;
        promise.done(function (a) { return _this.resolve(a); });
        promise.fail(function (e) { return _this.reject(e); });
        return this;
    };
    return DotvvmPromise;
}());
var DotvvmSerialization = (function () {
    function DotvvmSerialization() {
    }
    DotvvmSerialization.prototype.deserialize = function (viewModel, target, deserializeAll) {
        if (deserializeAll === void 0) { deserializeAll = false; }
        if (typeof (viewModel) == "undefined" || viewModel == null) {
            return viewModel;
        }
        if (typeof (viewModel) == "string" || typeof (viewModel) == "number" || typeof (viewModel) == "boolean") {
            return viewModel;
        }
        if (viewModel instanceof Date) {
            return viewModel;
        }
        // handle arrays
        if (viewModel instanceof Array) {
            if (ko.isObservable(target) && target.removeAll && target() != null && target().length === viewModel.length) {
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
                    if (!target.removeAll) {
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
        if (result == null) {
            target = result = {};
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
                // update the property
                if (ko.isObservable(deserialized)) {
                    if (ko.isObservable(result[prop])) {
                        if (deserialized() !== result[prop]()) {
                            result[prop](deserialized());
                        }
                    }
                    else {
                        result[prop] = deserialized;
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
                result[prop] = viewModel[prop];
                var originalName = prop.substring(0, prop.length - "$options".length);
                if (typeof result[originalName] === "undefined") {
                    result[originalName] = ko.observable();
                }
            }
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
        if (typeof (viewModel) === "undefined" || viewModel == null) {
            return null;
        }
        if (typeof (viewModel) === "string" || typeof (viewModel) === "number" || typeof (viewModel) === "boolean") {
            return viewModel;
        }
        if (ko.isObservable(viewModel)) {
            return this.serialize(ko.unwrap(viewModel), opt);
        }
        if (typeof (viewModel) === "function") {
            return null;
        }
        if (viewModel instanceof Array) {
            if (opt.pathOnly) {
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
            return this.serializeDate(viewModel);
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
                if (!opt.serializeAll && (/\$options$/.test(prop) || prop == "$validationErrors")) {
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
                else if (!opt.serializeAll && options && options.pathOnly) {
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
        if (pathProp)
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
            return +value === value || (!isNaN(+value) && typeof value == "string");
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
/// <reference path="DotVVM.ts" />
var DotvvmValidationContext = (function () {
    function DotvvmValidationContext(valueToValidate, parentViewModel, parameters) {
        this.valueToValidate = valueToValidate;
        this.parentViewModel = parentViewModel;
        this.parameters = parameters;
    }
    return DotvvmValidationContext;
}());
var DotvvmValidationObservableMetadata = (function () {
    function DotvvmValidationObservableMetadata() {
    }
    return DotvvmValidationObservableMetadata;
}());
var DotvvmValidationElementMetadata = (function () {
    function DotvvmValidationElementMetadata() {
        this.elementValidationState = true;
    }
    return DotvvmValidationElementMetadata;
}());
var DotvvmValidatorBase = (function () {
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
var DotvvmRequiredValidator = (function (_super) {
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
var DotvvmRegularExpressionValidator = (function (_super) {
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
var DotvvmIntRangeValidator = (function (_super) {
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
var DotvvmEnforceClientFormatValidator = (function (_super) {
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
var DotvvmRangeValidator = (function (_super) {
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
var DotvvmNotNullValidator = (function (_super) {
    __extends(DotvvmNotNullValidator, _super);
    function DotvvmNotNullValidator() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    DotvvmNotNullValidator.prototype.isValid = function (context) {
        return context.valueToValidate !== null && context.valueToValidate !== undefined;
    };
    return DotvvmNotNullValidator;
}(DotvvmValidatorBase));
var ValidationError = (function () {
    function ValidationError(validatedObservable, errorMessage) {
        this.validatedObservable = validatedObservable;
        this.errorMessage = errorMessage;
    }
    ValidationError.getOrCreate = function (validatedObservable) {
        if (validatedObservable["wrappedProperty"]) {
            var wrapped = validatedObservable["wrappedProperty"]();
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
var DotvvmValidation = (function () {
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
        // perform the validation before postback
        dotvvm.events.beforePostback.subscribe(function (args) {
            if (args.validationTargetPath) {
                // resolve target
                var context = ko.contextFor(args.sender);
                var validationTarget = dotvvm.evaluator.evaluateOnViewModel(context, args.validationTargetPath);
                // validate the object
                _this.clearValidationErrors(dotvvm.viewModelObservables[args.viewModelName]);
                _this.validateViewModel(validationTarget);
                if (_this.errors().length > 0) {
                    console.log("Validation failed: postback aborted; errors: ", _this.errors());
                    args.cancel = true;
                    args.clientValidationFailed = true;
                }
            }
            _this.events.validationErrorsChanged.trigger(args);
        });
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
            for (var type in args.serverResponseObject) {
                if (!args.serverResponseObject.hasOwnProperty(type))
                    continue;
                existingRules[type] = args.serverResponseObject[type];
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
        var validationTarget = dotvvm.evaluator.evaluateOnViewModel(context, args.validationTargetPath);
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
/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout/knockout.dotvvm.d.ts" />
/// <reference path="typings/knockout.mapper/knockout.mapper.d.ts" />
/// <reference path="typings/globalize/globalize.d.ts" />
/// <reference path="dotvvm.domutils.ts" />
/// <reference path="dotvvm.evaluator.ts" />
/// <reference path="dotvvm.events.ts" />
/// <reference path="dotvvm.fileupload.ts" />
/// <reference path="dotvvm.globalize.ts" />
/// <reference path="dotvvm.postbackhandlers.ts" />
/// <reference path="dotvvm.promise.ts" />
/// <reference path="dotvvm.serialization.ts" />
/// <reference path="dotvvm.validation.ts" />
document.getElementByDotvvmId = function (id) {
    return document.querySelector("[data-dotvvm-id='" + id + "']");
};
var DotVVM = (function () {
    function DotVVM() {
        this.postBackCounter = 0;
        this.resourceSigns = {};
        this.isViewModelUpdating = true;
        // warning this property is referenced in ModelState.cs and KnockoutHelper.cs
        this.viewModelObservables = {};
        this.isSpaReady = ko.observable(false);
        this.viewModels = {};
        this.serialization = new DotvvmSerialization();
        this.postBackHandlers = new DotvvmPostBackHandlers();
        this.events = new DotvvmEvents();
        this.globalize = new DotvvmGlobalize();
        this.evaluator = new DotvvmEvaluator();
        this.domUtils = new DotvvmDomUtils();
        this.fileUpload = new DotvvmFileUpload();
        this.extensions = {};
        this.isPostbackRunning = ko.observable(false);
    }
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
        this.events.init.trigger(new DotvvmEventArgs(viewModel));
        // handle SPA requests
        var spaPlaceHolder = this.getSpaPlaceHolder();
        if (spaPlaceHolder) {
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
    // binding helpers
    DotVVM.prototype.postbackScript = function (bindingId) {
        var _this = this;
        return function (pageArea, sender, pathFragments, controlId, useWindowSetTimeout, validationTarget, context, handlers) {
            _this.postBack(pageArea, sender, pathFragments, bindingId, controlId, useWindowSetTimeout, validationTarget, context, handlers);
        };
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
        this.postBackCounter++;
        return this.postBackCounter;
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
        // TODO: events for static command postback
        // prevent double postbacks
        var currentPostBackCounter = this.backUpPostBackConter();
        var data = this.serialization.serialize({
            "args": args,
            "command": command,
            "$csrfToken": this.viewModels[viewModelName].viewModel.$csrfToken
        });
        this.postJSON(this.viewModels[viewModelName].url, "POST", ko.toJSON(data), function (response) {
            if (!_this.isPostBackStillActive(currentPostBackCounter))
                return;
            try {
                _this.isViewModelUpdating = true;
                callback(JSON.parse(response.responseText));
            }
            catch (error) {
                errorCallback(response, error);
            }
            finally {
                _this.isViewModelUpdating = false;
            }
        }, function (xhr) {
            console.warn("StaticCommand postback failed: " + xhr.status + " - " + xhr.statusText, xhr);
            errorCallback(xhr);
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
    DotVVM.prototype.postBack = function (viewModelName, sender, path, command, controlUniqueId, useWindowSetTimeout, validationTargetPath, context, handlers) {
        var _this = this;
        if (this.isPostBackProhibited(sender))
            return new DotvvmPromise().reject("rejected");
        var promise = new DotvvmPromise();
        this.isPostbackRunning(true);
        promise.done(function () { return _this.isPostbackRunning(false); });
        promise.fail(function () { return _this.isPostbackRunning(false); });
        if (useWindowSetTimeout) {
            window.setTimeout(function () { return promise.chainFrom(_this.postBack(viewModelName, sender, path, command, controlUniqueId, false, validationTargetPath, context, handlers)); }, 0);
            return promise;
        }
        // apply postback handlers
        if (handlers && handlers.length > 0) {
            var handler = this.postBackHandlers[handlers[0].name];
            var options = this.evaluator.evaluateOnViewModel(ko.contextFor(sender), "(" + handlers[0].options.toString() + ")()");
            var handlerInstance = handler(options);
            var nextHandler = function () { return promise.chainFrom(_this.postBack(viewModelName, sender, path, command, controlUniqueId, false, validationTargetPath, context, handlers.slice(1))); };
            if (options.enabled) {
                handlerInstance.execute(nextHandler, sender);
            }
            else {
                nextHandler();
            }
            return promise;
        }
        var viewModel = this.viewModels[viewModelName].viewModel;
        // prevent double postbacks
        var currentPostBackCounter = this.backUpPostBackConter();
        // trigger beforePostback event
        var beforePostbackArgs = new DotvvmBeforePostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, currentPostBackCounter);
        this.events.beforePostback.trigger(beforePostbackArgs);
        if (beforePostbackArgs.cancel) {
            // trigger afterPostback event
            var afterPostBackArgsCanceled = new DotvvmAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, null, currentPostBackCounter);
            afterPostBackArgsCanceled.wasInterrupted = true;
            this.events.afterPostback.trigger(afterPostBackArgsCanceled);
            return promise.reject("canceled");
        }
        // perform the postback
        if (!context) {
            context = ko.contextFor(sender);
        }
        this.updateDynamicPathFragments(context, path);
        var data = {
            viewModel: this.serialization.serialize(viewModel, { pathMatcher: function (val) { return context && val == context.$data; } }),
            currentPath: path,
            command: command,
            controlUniqueId: this.processPassedId(controlUniqueId, context),
            validationTargetPath: validationTargetPath || null,
            renderedResources: this.viewModels[viewModelName].renderedResources
        };
        this.postJSON(this.viewModels[viewModelName].url, "POST", ko.toJSON(data), function (result) {
            // if another postback has already been passed, don't do anything
            if (!_this.isPostBackStillActive(currentPostBackCounter)) {
                var afterPostBackArgsCanceled = new DotvvmAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, null, currentPostBackCounter);
                afterPostBackArgsCanceled.wasInterrupted = true;
                _this.events.afterPostback.trigger(afterPostBackArgsCanceled);
                promise.reject("postback collision");
                return;
            }
            try {
                var resultObject, locationHeader = result.getResponseHeader("Location");
                if (locationHeader != null && locationHeader.length > 0) {
                    resultObject = { action: "redirect", url: locationHeader };
                }
                else {
                    resultObject = JSON.parse(result.responseText);
                }
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
                    }
                    else if (resultObject.action === "redirect") {
                        // redirect
                        _this.handleRedirect(resultObject, viewModelName);
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
                    var afterPostBackArgs = new DotvvmAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, resultObject, currentPostBackCounter);
                    promise.resolve(afterPostBackArgs);
                    _this.events.afterPostback.trigger(afterPostBackArgs);
                    if (!isSuccess && !afterPostBackArgs.isHandled) {
                        _this.error(viewModel, result, promise);
                    }
                });
            }
            catch (error) {
                _this.error(viewModel, result, promise);
            }
        }, function (xhr) {
            // if another postback has already been passed, don't do anything
            if (!_this.isPostBackStillActive(currentPostBackCounter))
                return;
            _this.error(viewModel, xhr, promise);
        });
        return promise;
    };
    DotVVM.prototype.error = function (viewModel, xhr, promise) {
        if (promise === void 0) { promise = null; }
        // execute error handlers
        var errArgs = new DotvvmErrorEventArgs(viewModel, xhr);
        if (promise != null)
            promise.reject(errArgs);
        this.events.error.trigger(errArgs);
        if (!errArgs.handled) {
            alert("unhandled error during postback");
        }
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
                var spaNavigatedArgs = new DotvvmSpaNavigatedEventArgs(viewModel, viewModelName, resultObject);
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
            var errArgs = new DotvvmErrorEventArgs(viewModel, xhr, true);
            _this.events.error.trigger(errArgs);
            if (!errArgs.handled) {
                alert(xhr.responseText);
            }
        });
    };
    DotVVM.prototype.handleRedirect = function (resultObject, viewModelName, replace) {
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
        return XMLHttpRequest ? new XMLHttpRequest() : new ActiveXObject("Microsoft.XMLHTTP");
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
        return routePath.replace(/\{([^\}]+?)\??(:(.+?))?\}/g, function (s, paramName, hsjdhsj, type) {
            if (!paramName)
                return "";
            return ko.unwrap(params[paramName.toLowerCase()]) || "";
        });
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
                dotvvm.events.beforePostback.subscribe(function (e) {
                    element.style.display = "";
                });
                dotvvm.events.spaNavigating.subscribe(function (e) {
                    element.style.display = "";
                });
                dotvvm.events.afterPostback.subscribe(function (e) {
                    element.style.display = "none";
                });
                dotvvm.events.redirect.subscribe(function (e) {
                    element.style.display = "none";
                });
                dotvvm.events.spaNavigated.subscribe(function (e) {
                    element.style.display = "none";
                });
                dotvvm.events.error.subscribe(function (e) {
                    element.style.display = "none";
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
                            lastDisplay = style.display;
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
                var obs = valueAccessor();
                //generate metadata func 
                var elmMetadata = new DotvvmValidationElementMetadata();
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
                dotvvm.domUtils.attachEvent(element, "blur", function () {
                    // parse the value
                    var result, isEmpty, newValue;
                    if (elmMetadata.dataType === "datetime") {
                        // parse date
                        var currentValue = obs();
                        if (currentValue != null) {
                            currentValue = dotvvm.globalize.parseDotvvmDate(currentValue);
                        }
                        result = dotvvm.globalize.parseDate(element.value, elmMetadata.format, currentValue);
                        isEmpty = result === null;
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
                        element.attributes["data-dotvvm-value-type-valid"] = false;
                        elmMetadata.elementValidationState = false;
                    }
                    else {
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
                var value = ko.unwrap(valueAccessor());
                if (element.attributes["data-dotvvm-value-type-valid"] != false) {
                    var format = (element.attributes["data-dotvvm-format"] || { value: "" }).value;
                    if (format) {
                        element.value = dotvvm.globalize.formatString(format, value);
                    }
                    else {
                        element.value = value;
                    }
                }
            }
        };
        ko.bindingHandlers["dotvvm-CheckState"] = {
            init: function (element, valueAccessor, allBindings) {
                ko.getBindingHandler("checked").init(element, valueAccessor, allBindings);
            },
            update: function (element, valueAccessor, allBindings) {
                var value = ko.unwrap(valueAccessor());
                element.indeterminate = value == null;
            }
        };
    };
    return DotVVM;
}());
/// <reference path="dotvvm.ts" />
var DotvvmFileUpload = (function () {
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
var DotvvmFileUploadCollection = (function () {
    function DotvvmFileUploadCollection() {
        this.Files = ko.observableArray();
        this.Progress = ko.observable(0);
        this.Error = ko.observable();
        this.IsBusy = ko.observable();
    }
    return DotvvmFileUploadCollection;
}());
var DotvvmFileUploadData = (function () {
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
var DotvvmFileSize = (function () {
    function DotvvmFileSize() {
        this.Bytes = ko.observable();
        this.FormattedText = ko.observable();
    }
    return DotvvmFileSize;
}());
//# sourceMappingURL=DotVVM.js.map