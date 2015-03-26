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
        this.extensions = {};
        this.viewModels = {};
        this.postBackCounter = 0;
        this.events = {
            preinit: new RedwoodEvent("redwood.events.preinit"),
            init: new RedwoodEvent("redwood.events.init", true),
            beforePostback: new RedwoodEvent("redwood.events.beforePostback"),
            afterPostback: new RedwoodEvent("redwood.events.afterPostback"),
            error: new RedwoodEvent("redwood.events.error")
        };
    }
    Redwood.prototype.includePathProps = function (viewModel, path) {
        var _this = this;
        if (path === void 0) { path = []; }
        viewModel.$path = path;
        for (var p in viewModel) {
            if (typeof viewModel[p] === "object" && viewModel[p] != null && p.charAt(0) != "$") {
                if (viewModel[p] instanceof Array)
                    viewModel[p].forEach(function (v, i) { return _this.includePathProps(v, path.concat([p, "[" + i + "]"])); });
                else
                    this.includePathProps(viewModel[p], path.concat(p));
            }
        }
    };
    Redwood.prototype.init = function (viewModelName, culture) {
        this.culture = culture;
        this.includePathProps(this.viewModels[viewModelName].viewModel);
        var viewModel = this.viewModels[viewModelName].viewModel = ko.mapper.fromJS(this.viewModels[viewModelName].viewModel);
        this.events.preinit.trigger(new RedwoodEventArgs(viewModel));
        ko.applyBindings(viewModel);
        this.events.init.trigger(new RedwoodEventArgs(viewModel));
    };
    Redwood.prototype.postBack = function (viewModelName, sender, path, command, controlUniqueId, validationTargetPath) {
        var _this = this;
        var viewModel = this.viewModels[viewModelName].viewModel;
        this.updateDynamicPathFragments(sender, path);
        // prevent double postbacks
        this.postBackCounter++;
        var currentPostBackCounter = this.postBackCounter;
        // trigger beforePostback event
        var beforePostbackArgs = new RedwoodBeforePostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath || ["$this"], path, command);
        this.events.beforePostback.trigger(beforePostbackArgs);
        if (beforePostbackArgs.cancel) {
            return;
        }
        // perform the postback
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
                _this.includePathProps(resultObject.viewModel);
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
    Redwood.prototype.spitPath = function (path) {
        var indexPos = path.indexOf('[');
        var dotPos = path.indexOf('.');
        var res = [];
        while (dotPos >= 0 || indexPos >= 0) {
            if (dotPos >= 0 && dotPos < indexPos) {
                res.push(path.substr(0, dotPos));
                path = path.substr(dotPos + 1);
                dotPos = path.indexOf('.');
            }
            if (indexPos >= 0 && indexPos < dotPos) {
                res.push(path.substr(0, dotPos));
                path = path.substr(dotPos);
                indexPos = path.indexOf('[');
                dotPos = path.indexOf('.');
            }
        }
        res.push(path);
        return res;
    };
    Redwood.prototype.combinePaths = function (a, b) {
        return this.simplifyPath(a.concat(b));
    };
    /**
    * removes `$parent` and `$root` where possible
    */
    Redwood.prototype.simplifyPath = function (path) {
        var ri = path.lastIndexOf("$root");
        if (ri > 0)
            path = path.slice(ri);
        path = path.filter(function (v) { return v != "$data"; });
        var parIndex = 0;
        while ((parIndex = path.indexOf("$parent")) >= 0) {
            path.splice(parIndex - 1, 2);
        }
        return path;
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
        expression.forEach(function (e) {
            if (e.length == 0 || context == null)
                return;
            if (ko.isObservable(context))
                context = context();
            if (e[0] == "[")
                context = context[eval(e.substring(1, e.length - 1))];
            else if (e[0] == "`")
                context = eval("(function (c) { return c." + e.substring(1, e.length - 1) + "; })")(context);
            else
                context = context[e];
        });
        return context;
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
    function RedwoodBeforePostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, viewModelPath, command) {
        _super.call(this, viewModel);
        this.sender = sender;
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.validationTargetPath = validationTargetPath;
        this.viewModelPath = viewModelPath;
        this.command = command;
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