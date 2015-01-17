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
            var resultObject = JSON.parse(result.responseText);
            var isSuccess = false;
            if (resultObject.action === "successfulCommand") {
                // update the viewmodel
                ko.mapper.fromJS(resultObject.viewModel, {}, _this.viewModels[viewModelName].viewModel);
                isSuccess = true;
            }
            else if (resultObject.action === "redirect") {
                // redirect
                document.location.href = resultObject.url;
                return;
            }
            // trigger afterPostback event
            var isHandled = _this.events.afterPostback.trigger(new RedwoodAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, resultObject));
            if (!isSuccess && !isHandled) {
                throw "Invalid response from server!";
            }
        }, function (xhr) {
            if (!_this.events.error.trigger(new RedwoodErrorEventArgs(viewModel, xhr))) {
                alert(xhr.responseText);
            }
        });
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
                if (handler(history[i])) {
                    this.history = this.history.splice(i, 1);
                }
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
            var result = this.handlers[i](data);
            if (result) {
                return true;
            }
        }
        if (this.triggerMissedEventsOnSubscribe) {
            this.history.push(data);
        }
        return false;
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
    }
    return RedwoodAfterPostBackEventArgs;
})(RedwoodEventArgs);
var redwood = new Redwood();
//# sourceMappingURL=Redwood.js.map