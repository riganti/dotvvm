/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout.mapper/knockout.mapper.d.ts" />
/// <reference path="typings/globalize/globalize.d.ts" />
var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var Redwood = (function () {
    function Redwood() {
        this.postBackCounter = 0;
        this.extensions = {};
        this.viewModels = {};
        this.events = {
            init: new RedwoodEvent("redwood.events.init", true),
            beforePostback: new RedwoodEvent("redwood.events.beforePostback"),
            afterPostback: new RedwoodEvent("redwood.events.afterPostback"),
            error: new RedwoodEvent("redwood.events.error")
        };
    }
    Redwood.prototype.init = function (viewModelName, culture) {
        this.culture = culture;
        this.viewModels[viewModelName].viewModel = ko.mapper.fromJS(this.viewModels[viewModelName].viewModel);
        var viewModel = this.viewModels[viewModelName].viewModel;
        ko.applyBindings(viewModel);
        this.events.init.trigger(new RedwoodEventArgs(viewModel));
    };
    Redwood.prototype.postBack = function (viewModelName, sender, path, command, controlUniqueId, validationTargetPath) {
        var _this = this;
        var viewModel = this.viewModels[viewModelName].viewModel;
        // prevent double postbacks
        this.postBackCounter++;
        var currentPostBackCounter = this.postBackCounter;
        // trigger beforePostback event
        var beforePostbackArgs = new RedwoodBeforePostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath);
        this.events.beforePostback.trigger(beforePostbackArgs);
        if (beforePostbackArgs.cancel) {
            return;
        }
        // perform the postback
        this.updateDynamicPathFragments(sender, path);
        var data = {
            viewModel: ko.mapper.toJS(viewModel),
            currentPath: path,
            command: command,
            controlUniqueId: controlUniqueId,
            validationTargetPath: validationTargetPath || null
        };
        this.postJSON(document.location.href, "POST", ko.toJSON(data), function (result) {
            // if another postback has already been passed, don't do anything
            if (_this.postBackCounter !== currentPostBackCounter)
                return;
            var resultObject = JSON.parse(result.responseText);
            if (!resultObject.viewModel && resultObject.viewModelDiff) {
                resultObject.viewModel = _this.patch(data.viewModel, resultObject.viewModelDiff);
            }
            var isSuccess = false;
            if (resultObject.action === "successfulCommand") {
                // remove updated controls
                var updatedControls = {};
                for (var id in resultObject.updatedControls) {
                    if (resultObject.updatedControls.hasOwnProperty(id)) {
                        var control = document.getElementById(id);
                        var nextSibling = control.nextSibling;
                        var parent = control.parentNode;
                        ko.removeNode(control);
                        updatedControls[id] = { control: control, nextSibling: nextSibling, parent: parent };
                    }
                }
                // update the viewmodel
                ko.mapper.fromJS(resultObject.viewModel, {}, _this.viewModels[viewModelName].viewModel);
                isSuccess = true;
                for (id in resultObject.updatedControls) {
                    if (resultObject.updatedControls.hasOwnProperty(id)) {
                        var updatedControl = updatedControls[id];
                        if (updatedControl.nextSibling) {
                            updatedControl.parent.insertBefore(updatedControl.control, updatedControl.nextSibling);
                        }
                        else {
                            updatedControl.parent.appendChild(updatedControl.control);
                        }
                        updatedControl.control.outerHTML = resultObject.updatedControls[id];
                        ko.applyBindings(ko.dataFor(updatedControl.parent), updatedControl.control);
                    }
                }
            }
            else if (resultObject.action === "redirect") {
                // redirect
                document.location.href = resultObject.url;
                return;
            }
            // trigger afterPostback event
            var afterPostBackArgs = new RedwoodAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, resultObject);
            _this.events.afterPostback.trigger(afterPostBackArgs);
            if (!isSuccess && !afterPostBackArgs.isHandled) {
                throw "Invalid response from server!";
            }
        }, function (xhr) {
            // if another postback has already been passed, don't do anything
            if (_this.postBackCounter !== currentPostBackCounter)
                return;
            // execute error handlers
            if (!_this.events.error.trigger(new RedwoodErrorEventArgs(viewModel, xhr))) {
                alert(xhr.responseText);
            }
        });
    };
    Redwood.prototype.patch = function (source, patch) {
        var _this = this;
        if (source instanceof Array && patch instanceof Array) {
            return patch.map(function (val, i) { return _this.patch(source[i], val); });
        }
        else if (source instanceof Array || patch instanceof Array)
            return patch;
        else if (typeof source == "object" && typeof patch == "object") {
            for (var p in patch) {
                if (patch[p] == null)
                    delete source[p];
                else
                    source[p] = this.patch(source[p], patch[p]);
            }
        }
        else
            return patch;
        return source;
    };
    Redwood.prototype.formatString = function (format, value) {
        if (format == "g") {
            return redwood.formatString("d", value) + " " + redwood.formatString("t", value);
        }
        else if (format == "G") {
            return redwood.formatString("d", value) + " " + redwood.formatString("T", value);
        }
        value = ko.unwrap(value);
        if (typeof value === "string" && value.match("^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(\\.[0-9]{1,3})?$")) {
            // JSON date in string
            value = new Date(value);
        }
        return Globalize.format(value, format, redwood.culture);
    };
    Redwood.prototype.getDataSourceItems = function (viewModel) {
        var value = ko.unwrap(viewModel);
        return value.Items || value;
    };
    Redwood.prototype.updateDynamicPathFragments = function (sender, path) {
        var context = ko.contextFor(sender);
        for (var i = path.length - 1; i >= 0; i--) {
            if (path[i].indexOf("[$index]") >= 0) {
                path[i] = path[i].replace("[$index]", "[" + context.$index() + "]");
            }
            context = context.$parentContext;
        }
    };
    Redwood.prototype.postJSON = function (url, method, postData, success, error) {
        var xhr = XMLHttpRequest ? new XMLHttpRequest() : new ActiveXObject("Microsoft.XMLHTTP");
        xhr.open(method, url, true);
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.onreadystatechange = function () {
            if (xhr.readyState != 4)
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
    Redwood.prototype.evaluateOnViewModel = function (context, expression) {
        return eval("(function (c) { return c." + expression + "; })")(context);
    };
    return Redwood;
})();
// RedwoodEvent is used because CustomEvent is not browser compatible and does not support 
// calling missed events for handler that subscribed too late.
var RedwoodEvent = (function () {
    function RedwoodEvent(name, triggerMissedEventsOnSubscribe) {
        if (triggerMissedEventsOnSubscribe === void 0) { triggerMissedEventsOnSubscribe = false; }
        this.name = name;
        this.triggerMissedEventsOnSubscribe = triggerMissedEventsOnSubscribe;
        this.handlers = [];
        this.history = [];
    }
    RedwoodEvent.prototype.subscribe = function (handler) {
        this.handlers.push(handler);
        if (this.triggerMissedEventsOnSubscribe) {
            for (var i = 0; i < this.history.length; i++) {
                handler(history[i]);
            }
        }
    };
    RedwoodEvent.prototype.unsubscribe = function (handler) {
        var index = this.handlers.indexOf(handler);
        if (index >= 0) {
            this.handlers = this.handlers.splice(index, 1);
        }
    };
    RedwoodEvent.prototype.trigger = function (data) {
        for (var i = 0; i < this.handlers.length; i++) {
            this.handlers[i](data);
        }
        if (this.triggerMissedEventsOnSubscribe) {
            this.history.push(data);
        }
    };
    return RedwoodEvent;
})();
var RedwoodEventArgs = (function () {
    function RedwoodEventArgs(viewModel) {
        this.viewModel = viewModel;
    }
    return RedwoodEventArgs;
})();
var RedwoodErrorEventArgs = (function (_super) {
    __extends(RedwoodErrorEventArgs, _super);
    function RedwoodErrorEventArgs(viewModel, xhr) {
        _super.call(this, viewModel);
        this.viewModel = viewModel;
        this.xhr = xhr;
    }
    return RedwoodErrorEventArgs;
})(RedwoodEventArgs);
var RedwoodBeforePostBackEventArgs = (function (_super) {
    __extends(RedwoodBeforePostBackEventArgs, _super);
    function RedwoodBeforePostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath) {
        _super.call(this, viewModel);
        this.sender = sender;
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.validationTargetPath = validationTargetPath;
        this.cancel = false;
    }
    return RedwoodBeforePostBackEventArgs;
})(RedwoodEventArgs);
var RedwoodAfterPostBackEventArgs = (function (_super) {
    __extends(RedwoodAfterPostBackEventArgs, _super);
    function RedwoodAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, serverResponseObject) {
        _super.call(this, viewModel);
        this.sender = sender;
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.validationTargetPath = validationTargetPath;
        this.serverResponseObject = serverResponseObject;
        this.isHandled = false;
    }
    return RedwoodAfterPostBackEventArgs;
})(RedwoodEventArgs);
var redwood = new Redwood();
// add knockout binding handler for update progress control
ko.bindingHandlers["redwoodUpdateProgressVisible"] = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        element.style.display = "none";
        redwood.events.beforePostback.subscribe(function (e) {
            element.style.display = "";
        });
        redwood.events.afterPostback.subscribe(function (e) {
            element.style.display = "none";
        });
        redwood.events.error.subscribe(function (e) {
            element.style.display = "none";
        });
    }
};
//# sourceMappingURL=Redwood.js.map