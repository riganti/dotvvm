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
})();
var DotvvmEvaluator = (function () {
    function DotvvmEvaluator() {
    }
    DotvvmEvaluator.prototype.evaluateOnViewModel = function (context, expression) {
        var result;
        if (context && context.$data) {
            result = eval("(function ($context) { with($context) { with ($data) { return " + expression + "; } } })")(context);
        }
        else {
            result = eval("(function ($context) { with($context) { return " + expression + "; } })")(context);
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
    DotvvmEvaluator.prototype.buildClientId = function (element, fragments) {
        var id = "";
        for (var i = 0; i < fragments.length; i++) {
            if (id.length > 0) {
                id += "_";
            }
            id += ko.unwrap(fragments[i]);
        }
        return id;
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
})();
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var DotvvmEvents = (function () {
    function DotvvmEvents() {
        this.init = new DotvvmEvent("dotvvm.events.init", true);
        this.beforePostback = new DotvvmEvent("dotvvm.events.beforePostback");
        this.afterPostback = new DotvvmEvent("dotvvm.events.afterPostback");
        this.error = new DotvvmEvent("dotvvm.events.error");
        this.spaNavigating = new DotvvmEvent("dotvvm.events.spaNavigating");
        this.spaNavigated = new DotvvmEvent("dotvvm.events.spaNavigated");
    }
    return DotvvmEvents;
})();
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
})();
var DotvvmEventArgs = (function () {
    function DotvvmEventArgs(viewModel) {
        this.viewModel = viewModel;
    }
    return DotvvmEventArgs;
})();
var DotvvmErrorEventArgs = (function (_super) {
    __extends(DotvvmErrorEventArgs, _super);
    function DotvvmErrorEventArgs(viewModel, xhr, isSpaNavigationError) {
        if (isSpaNavigationError === void 0) { isSpaNavigationError = false; }
        _super.call(this, viewModel);
        this.viewModel = viewModel;
        this.xhr = xhr;
        this.isSpaNavigationError = isSpaNavigationError;
        this.handled = false;
    }
    return DotvvmErrorEventArgs;
})(DotvvmEventArgs);
var DotvvmBeforePostBackEventArgs = (function (_super) {
    __extends(DotvvmBeforePostBackEventArgs, _super);
    function DotvvmBeforePostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath) {
        _super.call(this, viewModel);
        this.sender = sender;
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.validationTargetPath = validationTargetPath;
        this.cancel = false;
        this.clientValidationFailed = false;
    }
    return DotvvmBeforePostBackEventArgs;
})(DotvvmEventArgs);
var DotvvmAfterPostBackEventArgs = (function (_super) {
    __extends(DotvvmAfterPostBackEventArgs, _super);
    function DotvvmAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, serverResponseObject) {
        _super.call(this, viewModel);
        this.sender = sender;
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.validationTargetPath = validationTargetPath;
        this.serverResponseObject = serverResponseObject;
        this.isHandled = false;
        this.wasInterrupted = false;
    }
    return DotvvmAfterPostBackEventArgs;
})(DotvvmEventArgs);
var DotvvmSpaNavigatingEventArgs = (function (_super) {
    __extends(DotvvmSpaNavigatingEventArgs, _super);
    function DotvvmSpaNavigatingEventArgs(viewModel, viewModelName, newUrl) {
        _super.call(this, viewModel);
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.newUrl = newUrl;
        this.cancel = false;
    }
    return DotvvmSpaNavigatingEventArgs;
})(DotvvmEventArgs);
var DotvvmSpaNavigatedEventArgs = (function (_super) {
    __extends(DotvvmSpaNavigatedEventArgs, _super);
    function DotvvmSpaNavigatedEventArgs(viewModel, viewModelName, serverResponseObject) {
        _super.call(this, viewModel);
        this.viewModel = viewModel;
        this.viewModelName = viewModelName;
        this.serverResponseObject = serverResponseObject;
        this.isHandled = false;
    }
    return DotvvmSpaNavigatedEventArgs;
})(DotvvmEventArgs);
/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout.mapper/knockout.mapper.d.ts" />
/// <reference path="typings/globalize/globalize.d.ts" />
var DotVVM = (function () {
    function DotVVM() {
        this.postBackCounter = 0;
        this.resourceSigns = {};
        this.isViewModelUpdating = true;
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
    }
    DotVVM.prototype.init = function (viewModelName, culture) {
        var _this = this;
        this.addKnockoutBindingHandlers();
        // load the viewmodel
        var thisViewModel = this.viewModels[viewModelName] = JSON.parse(document.getElementById("__dot_viewmodel_" + viewModelName).value);
        if (thisViewModel.renderedResources) {
            thisViewModel.renderedResources.forEach(function (r) { return _this.resourceSigns[r] = true; });
        }
        var viewModel = thisViewModel.viewModel = this.serialization.deserialize(this.viewModels[viewModelName].viewModel, {}, true);
        // initialize services
        this.culture = culture;
        this.validation = new DotvvmValidation(this);
        // wrap it in the observable
        this.viewModelObservables[viewModelName] = ko.observable(viewModel);
        ko.applyBindings(this.viewModelObservables[viewModelName], document.documentElement);
        // trigger the init event
        this.events.init.trigger(new DotvvmEventArgs(viewModel));
        this.isViewModelUpdating = false;
        // handle SPA requests
        var spaPlaceHolder = this.getSpaPlaceHolder();
        if (spaPlaceHolder) {
            this.domUtils.attachEvent(window, "hashchange", function () { return _this.handleHashChange(viewModelName, spaPlaceHolder); });
            this.handleHashChange(viewModelName, spaPlaceHolder);
        }
        // persist the viewmodel in the hidden field so the Back button will work correctly
        this.domUtils.attachEvent(window, "beforeunload", function (e) {
            _this.persistViewModel(viewModelName);
        });
    };
    DotVVM.prototype.handleHashChange = function (viewModelName, spaPlaceHolder) {
        if (document.location.hash.indexOf("#!/") === 0) {
            this.navigateCore(viewModelName, document.location.hash.substring(2));
        }
        else {
            // redirect to the default URL
            var url = spaPlaceHolder.getAttribute("data-dot-spacontentplaceholder-defaultroute");
            if (url) {
                document.location.hash = "#!/" + url;
            }
            else {
                this.isSpaReady(true);
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
        if (errorCallback === void 0) { errorCallback = function (xhr) { }; }
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
            callback(JSON.parse(response.responseText));
        }, errorCallback, function (xhr) {
            xhr.setRequestHeader("X-PostbackType", "StaticCommand");
        });
    };
    DotVVM.prototype.postBack = function (viewModelName, sender, path, command, controlUniqueId, useWindowSetTimeout, validationTargetPath, context, handlers) {
        var _this = this;
        if (this.isPostBackProhibited(sender))
            return;
        var promise = new DotvvmPromise();
        if (useWindowSetTimeout) {
            window.setTimeout(function () { return promise.chainFrom(_this.postBack(viewModelName, sender, path, command, controlUniqueId, false, validationTargetPath, context, handlers)); }, 0);
            return promise;
        }
        // apply postback handlers
        if (handlers && handlers.length > 0) {
            var handler = this.postBackHandlers[handlers[0].name];
            var options = this.evaluator.evaluateOnViewModel(ko.contextFor(sender), "(" + handlers[0].options.toString() + ")()");
            var handlerInstance = handler(options);
            handlerInstance.execute(function () { return promise.chainFrom(_this.postBack(viewModelName, sender, path, command, controlUniqueId, false, validationTargetPath, context, handlers.slice(1))); }, sender);
            return promise;
        }
        var viewModel = this.viewModels[viewModelName].viewModel;
        // prevent double postbacks
        var currentPostBackCounter = this.backUpPostBackConter();
        // trigger beforePostback event
        var beforePostbackArgs = new DotvvmBeforePostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath);
        this.events.beforePostback.trigger(beforePostbackArgs);
        if (beforePostbackArgs.cancel) {
            // trigger afterPostback event
            var afterPostBackArgsCanceled = new DotvvmAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, null);
            afterPostBackArgsCanceled.wasInterrupted = true;
            this.events.afterPostback.trigger(afterPostBackArgsCanceled);
            return;
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
            controlUniqueId: controlUniqueId,
            validationTargetPath: validationTargetPath || null,
            renderedResources: this.viewModels[viewModelName].renderedResources
        };
        this.postJSON(this.viewModels[viewModelName].url, "POST", ko.toJSON(data), function (result) {
            // if another postback has already been passed, don't do anything
            if (!_this.isPostBackStillActive(currentPostBackCounter)) {
                var afterPostBackArgsCanceled = new DotvvmAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, null);
                afterPostBackArgsCanceled.wasInterrupted = true;
                _this.events.afterPostback.trigger(afterPostBackArgsCanceled);
                promise.reject("postback collision");
                return;
            }
            var resultObject = JSON.parse(result.responseText);
            if (!resultObject.viewModel && resultObject.viewModelDiff) {
                // TODO: patch (~deserialize) it to ko.observable viewModel
                _this.isViewModelUpdating = true;
                resultObject.viewModel = _this.patch(data.viewModel, resultObject.viewModelDiff);
            }
            _this.loadResourceList(resultObject.resources, function () {
                var isSuccess = false;
                if (resultObject.action === "successfulCommand") {
                    _this.isViewModelUpdating = true;
                    // remove updated controls
                    var updatedControls = _this.cleanUpdatedControls(resultObject);
                    // update the viewmodel
                    if (resultObject.viewModel) {
                        _this.serialization.deserialize(resultObject.viewModel, _this.viewModels[viewModelName].viewModel);
                    }
                    isSuccess = true;
                    // remove updated controls which were previously hidden
                    _this.cleanUpdatedControls(resultObject, updatedControls);
                    // add updated controls
                    _this.restoreUpdatedControls(resultObject, updatedControls, true);
                    _this.isViewModelUpdating = false;
                }
                else if (resultObject.action === "redirect") {
                    // redirect
                    _this.handleRedirect(resultObject, viewModelName);
                    return;
                }
                // trigger afterPostback event
                var afterPostBackArgs = new DotvvmAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, resultObject);
                promise.resolve(afterPostBackArgs);
                _this.events.afterPostback.trigger(afterPostBackArgs);
                if (!isSuccess && !afterPostBackArgs.isHandled) {
                    throw "Invalid response from server!";
                }
            });
        }, function (xhr) {
            // if another postback has already been passed, don't do anything
            if (!_this.isPostBackStillActive(currentPostBackCounter))
                return;
            // execute error handlers
            var errArgs = new DotvvmErrorEventArgs(viewModel, xhr);
            promise.reject(errArgs);
            _this.events.error.trigger(errArgs);
            if (!errArgs.handled) {
                alert(xhr.responseText);
            }
        });
        return promise;
    };
    DotVVM.prototype.loadResourceList = function (resources, callback) {
        var html = "";
        for (var name in resources) {
            if (this.resourceSigns[name])
                continue;
            this.resourceSigns[name] = true;
            html += resources[name] + " ";
        }
        if (html.trim() == "") {
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
        var fullUrl = this.addLeadingSlash(this.concatUrl(this.viewModels[viewModelName].virtualDirectory || "", url));
        // find SPA placeholder
        var spaPlaceHolder = this.getSpaPlaceHolder();
        if (!spaPlaceHolder) {
            document.location.href = fullUrl;
            return;
        }
        // send the request
        var spaPlaceHolderUniqueId = spaPlaceHolder.attributes["data-dot-spacontentplaceholder"].value;
        this.getJSON(fullUrl, "GET", spaPlaceHolderUniqueId, function (result) {
            // if another postback has already been passed, don't do anything
            if (!_this.isPostBackStillActive(currentPostBackCounter))
                return;
            var resultObject = JSON.parse(result.responseText);
            _this.loadResourceList(resultObject.resources, function () {
                var isSuccess = false;
                if (resultObject.action === "successfulCommand" || !resultObject.action) {
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
                    _this.serialization.deserialize(resultObject.viewModel, _this.viewModels[viewModelName].viewModel);
                    isSuccess = true;
                    // add updated controls
                    _this.viewModelObservables[viewModelName](_this.viewModels[viewModelName].viewModel);
                    _this.restoreUpdatedControls(resultObject, updatedControls, true);
                    _this.isSpaReady(true);
                    _this.isViewModelUpdating = false;
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
        if (this.getSpaPlaceHolder() && resultObject.url.indexOf("//") < 0) {
            // relative URL - keep in SPA mode, but remove the virtual directory
            url = "#!" + this.removeVirtualDirectoryFromUrl(resultObject.url, viewModelName);
        }
        else {
            // absolute URL - load the URL
            url = resultObject.url;
        }
        if (replace)
            location.replace(url);
        else
            location.href = url;
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
        preprocessRequest(xhr);
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
    DotVVM.prototype.getJSON = function (url, method, spaPlaceHolderUniqueId, success, error) {
        var xhr = this.getXHR();
        xhr.open(method, url, true);
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.setRequestHeader("X-DotVVM-SpaContentPlaceHolder", spaPlaceHolderUniqueId);
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
        xhr.send();
    };
    DotVVM.prototype.getXHR = function () {
        return XMLHttpRequest ? new XMLHttpRequest() : new ActiveXObject("Microsoft.XMLHTTP");
    };
    DotVVM.prototype.cleanUpdatedControls = function (resultObject, updatedControls) {
        if (updatedControls === void 0) { updatedControls = {}; }
        for (var id in resultObject.updatedControls) {
            if (resultObject.updatedControls.hasOwnProperty(id)) {
                var control = document.getElementById(id);
                if (control) {
                    var nextSibling = control.nextSibling;
                    var parent = control.parentNode;
                    ko.removeNode(control);
                    updatedControls[id] = { control: control, nextSibling: nextSibling, parent: parent };
                }
            }
        }
        return updatedControls;
    };
    DotVVM.prototype.restoreUpdatedControls = function (resultObject, updatedControls, applyBindingsOnEachControl) {
        for (var id in resultObject.updatedControls) {
            if (resultObject.updatedControls.hasOwnProperty(id)) {
                var updatedControl = updatedControls[id];
                if (updatedControl) {
                    if (updatedControl.nextSibling) {
                        updatedControl.parent.insertBefore(updatedControl.control, updatedControl.nextSibling);
                    }
                    else {
                        updatedControl.parent.appendChild(updatedControl.control);
                    }
                    updatedControl.control.outerHTML = resultObject.updatedControls[id];
                }
            }
        }
        if (applyBindingsOnEachControl) {
            window.setTimeout(function () {
                for (var id in resultObject.updatedControls) {
                    var updatedControl = document.getElementById(id);
                    if (updatedControl) {
                        ko.applyBindings(ko.dataFor(updatedControl.parentNode), updatedControl);
                    }
                }
            }, 0);
        }
    };
    DotVVM.prototype.unwrapArrayExtension = function (array) {
        return ko.unwrap(ko.unwrap(array));
    };
    DotVVM.prototype.buildRouteUrl = function (routePath, params) {
        return routePath.replace(/\{[^\}]+\??\}/g, function (s) {
            var paramName = s.substring(1, s.length - 1).toLowerCase();
            if (paramName && paramName.length > 0 && paramName.substring(paramName.length - 1) === "?") {
                paramName = paramName.substring(0, paramName.length - 1);
            }
            return ko.unwrap(params[paramName]) || "";
        });
    };
    DotVVM.prototype.isPostBackProhibited = function (element) {
        if (element.tagName.toLowerCase() === "a" && element.getAttribute("disabled")) {
            return true;
        }
        return false;
    };
    DotVVM.prototype.addKnockoutBindingHandlers = function () {
        ko.virtualElements.allowedBindings["withControlProperties"] = true;
        ko.bindingHandlers["withControlProperties"] = {
            init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                var value = valueAccessor();
                for (var prop in value) {
                    if (!ko.isObservable(value[prop])) {
                        value[prop] = ko.observable(value[prop]);
                    }
                }
                var innerBindingContext = bindingContext.extend({ $control: value });
                ko.applyBindingsToDescendants(innerBindingContext, element);
                return { controlsDescendantBindings: true }; // do not apply binding again
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
        ko.bindingHandlers["dotvvmUpdateProgressVisible"] = {
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
                dotvvm.events.spaNavigated.subscribe(function (e) {
                    element.style.display = "none";
                });
                dotvvm.events.error.subscribe(function (e) {
                    element.style.display = "none";
                });
            }
        };
    };
    return DotVVM;
})();
/// <reference path="dotvvm.ts" />
var DotvvmFileUpload = (function () {
    function DotvvmFileUpload() {
    }
    DotvvmFileUpload.prototype.showUploadDialog = function (sender) {
        var uploadId = "upl" + new Date().getTime().toString();
        sender.parentElement.parentElement.dataset["dotvvmUploadId"] = uploadId;
        var iframe = sender.parentElement.previousSibling;
        iframe.dataset["dotvvmUploadId"] = uploadId;
        // trigger the file upload dialog
        var fileUpload = iframe.contentWindow.document.getElementById('upload');
        fileUpload.click();
    };
    DotvvmFileUpload.prototype.reportProgress = function (targetControlId, isBusy, progress, result) {
        // find target control viewmodel
        var targetControl = document.querySelector("div[data-dotvvm-upload-id='" + targetControlId + "']");
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
                viewModel.Files.push(dotvvm.serialization.deserialize(result[i]));
            }
            // call the handler
            if (targetControl.dataset["uploadCompleted"]) {
                new Function(targetControl.dataset["uploadCompleted"]).call(targetControl);
            }
        }
        viewModel.Progress(progress);
        viewModel.IsBusy(isBusy);
    };
    return DotvvmFileUpload;
})();
var DotvvmFileUploadCollection = (function () {
    function DotvvmFileUploadCollection() {
        this.Files = ko.observableArray();
        this.Progress = ko.observable(0);
        this.Error = ko.observable();
        this.IsBusy = ko.observable();
    }
    return DotvvmFileUploadCollection;
})();
var DotvvmFileUploadData = (function () {
    function DotvvmFileUploadData() {
        this.FileId = ko.observable();
        this.FileName = ko.observable();
    }
    return DotvvmFileUploadData;
})();
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
        if (format === "g") {
            return this.formatString("d", value) + " " + this.formatString("t", value);
        }
        else if (format === "G") {
            return this.formatString("d", value) + " " + this.formatString("T", value);
        }
        if (typeof value === "string" && value.match("^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(\\.[0-9]{1,3})?$")) {
            // JSON date in string
            value = new Date(value);
        }
        return Globalize.format(value, format, dotvvm.culture);
    };
    return DotvvmGlobalize;
})();
var DotvvmPostBackHandler = (function () {
    function DotvvmPostBackHandler() {
    }
    DotvvmPostBackHandler.prototype.execute = function (callback, sender) {
    };
    return DotvvmPostBackHandler;
})();
var ConfirmPostBackHandler = (function (_super) {
    __extends(ConfirmPostBackHandler, _super);
    function ConfirmPostBackHandler(message) {
        _super.call(this);
        this.message = message;
    }
    ConfirmPostBackHandler.prototype.execute = function (callback, sender) {
        if (confirm(this.message)) {
            callback();
        }
    };
    return ConfirmPostBackHandler;
})(DotvvmPostBackHandler);
var DotvvmPostBackHandlers = (function () {
    function DotvvmPostBackHandlers() {
        this.confirm = function (options) { return new ConfirmPostBackHandler(options.message); };
    }
    return DotvvmPostBackHandlers;
})();
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
    };
    DotvvmPromise.prototype.chainFrom = function (promise) {
        var _this = this;
        promise.done(function (a) { return _this.resolve(a); });
        promise.fail(function (e) { return _this.fail(e); });
    };
    return DotvvmPromise;
})();
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
                        if (deserialized !== result[prop])
                            result[prop](deserialized);
                    }
                    else {
                        result[prop] = ko.observable(deserialized);
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
            return viewModel;
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
                if (!opt.serializeAll && /\$options$/.test(prop)) {
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
            }
        }
        if (pathProp)
            opt.path.push(pathProp);
        return result;
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
    DotvvmSerialization.prototype.serializeDate = function (date) {
        var date2 = new Date(date.getTime());
        date2.setMinutes(date.getMinutes() + date.getTimezoneOffset());
        var y = this.pad(date2.getFullYear().toString(), 4);
        var m = this.pad((date2.getMonth() + 1).toString(), 2);
        var d = this.pad(date2.getDate().toString(), 2);
        var h = this.pad(date2.getHours().toString(), 2);
        var mi = this.pad(date2.getMinutes().toString(), 2);
        var s = this.pad(date2.getSeconds().toString(), 2);
        var ms = this.pad(date2.getMilliseconds().toString(), 3);
        return y + "-" + m + "-" + d + "T" + h + ":" + mi + ":" + s + "." + ms + "000";
    };
    return DotvvmSerialization;
})();
/// <reference path="typings/knockout/knockout.d.ts" />
var DotvvmValidationContext = (function () {
    function DotvvmValidationContext(valueToValidate, parentViewModel, parameters) {
        this.valueToValidate = valueToValidate;
        this.parentViewModel = parentViewModel;
        this.parameters = parameters;
    }
    return DotvvmValidationContext;
})();
var DotvvmValidatorBase = (function () {
    function DotvvmValidatorBase() {
    }
    DotvvmValidatorBase.prototype.isValid = function (context) {
        return false;
    };
    DotvvmValidatorBase.prototype.isEmpty = function (value) {
        return value == null || (typeof value == "string" && value.trim() == "");
    };
    return DotvvmValidatorBase;
})();
var DotvvmRequiredValidator = (function (_super) {
    __extends(DotvvmRequiredValidator, _super);
    function DotvvmRequiredValidator() {
        _super.apply(this, arguments);
    }
    DotvvmRequiredValidator.prototype.isValid = function (context) {
        var value = context.valueToValidate;
        return !this.isEmpty(value);
    };
    return DotvvmRequiredValidator;
})(DotvvmValidatorBase);
var DotvvmRegularExpressionValidator = (function (_super) {
    __extends(DotvvmRegularExpressionValidator, _super);
    function DotvvmRegularExpressionValidator() {
        _super.apply(this, arguments);
    }
    DotvvmRegularExpressionValidator.prototype.isValid = function (context) {
        var value = context.valueToValidate;
        var expr = context.parameters[0];
        return this.isEmpty(value) || new RegExp(expr).test(value);
    };
    return DotvvmRegularExpressionValidator;
})(DotvvmValidatorBase);
var DotvvmIntRangeValidator = (function (_super) {
    __extends(DotvvmIntRangeValidator, _super);
    function DotvvmIntRangeValidator() {
        _super.apply(this, arguments);
    }
    DotvvmIntRangeValidator.prototype.isValid = function (context) {
        var val = context.valueToValidate;
        var from = context.parameters[0];
        var to = context.parameters[1];
        return val % 1 === 0 && val >= from && val <= to;
    };
    return DotvvmIntRangeValidator;
})(DotvvmValidatorBase);
var DotvvmRangeValidator = (function (_super) {
    __extends(DotvvmRangeValidator, _super);
    function DotvvmRangeValidator() {
        _super.apply(this, arguments);
    }
    DotvvmRangeValidator.prototype.isValid = function (context) {
        var val = context.valueToValidate;
        var from = context.parameters[0];
        var to = context.parameters[1];
        return val >= from && val <= to;
    };
    return DotvvmRangeValidator;
})(DotvvmValidatorBase);
var ValidationError = (function () {
    function ValidationError(targetObservable) {
        var _this = this;
        this.targetObservable = targetObservable;
        this.errorMessage = ko.observable("");
        this.isValid = ko.computed(function () { return _this.errorMessage(); });
    }
    ValidationError.getOrCreate = function (targetObservable) {
        if (!targetObservable["validationError"]) {
            targetObservable["validationError"] = new ValidationError(targetObservable);
        }
        return targetObservable["validationError"];
    };
    return ValidationError;
})();
var DotvvmValidation = (function () {
    function DotvvmValidation(dotvvm) {
        var _this = this;
        this.rules = {
            "required": new DotvvmRequiredValidator(),
            "regularExpression": new DotvvmRegularExpressionValidator(),
            "intrange": new DotvvmIntRangeValidator(),
            "range": new DotvvmRangeValidator(),
        };
        this.errors = ko.observableArray([]);
        this.events = {
            validationErrorsChanged: new DotvvmEvent("dotvvm.validation.events.validationErrorsChanged")
        };
        this.elementUpdateFunctions = {
            // shows the element when it is valid
            hideWhenValid: function (element, errorMessage, param) {
                if (errorMessage) {
                    element.style.display = "";
                }
                else {
                    element.style.display = "none";
                }
            },
            // adds a CSS class when the element is not valid
            invalidCssClass: function (element, errorMessage, param) {
                if (errorMessage) {
                    element.className += " " + param;
                }
                else {
                    element.className = element.className.split(' ').filter(function (c) { return c != param; }).join(' ');
                }
            },
            // sets the error message as the title attribute
            setToolTipText: function (element, errorMessage, param) {
                if (errorMessage) {
                    element.title = errorMessage;
                }
                else {
                    element.title = "";
                }
            },
            // displays the error message
            showErrorMessageText: function (element, errorMessage, param) {
                element[element.innerText ? "innerText" : "textContent"] = errorMessage;
            }
        };
        // perform the validation before postback
        dotvvm.events.beforePostback.subscribe(function (args) {
            if (args.validationTargetPath) {
                // resolve target
                var context = ko.contextFor(args.sender);
                var validationTarget = dotvvm.evaluator.evaluateOnViewModel(context, args.validationTargetPath);
                // validate the object
                _this.clearValidationErrors(args.viewModel);
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
                    var updateFunction = function (element, errorMessage) {
                        for (var option in options) {
                            if (options.hasOwnProperty(option)) {
                                _this.elementUpdateFunctions[option](element, errorMessage, options[option]);
                            }
                        }
                    };
                    // subscribe to the observable property changes
                    var validationError = ValidationError.getOrCreate(observableProperty);
                    validationError.errorMessage.subscribe(function (newValue) { return updateFunction(element, newValue); });
                    updateFunction(element, validationError.errorMessage());
                }
            }
        };
    }
    /// Validates the specified view model
    DotvvmValidation.prototype.validateViewModel = function (viewModel) {
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
            if (value) {
                if (Array.isArray(value)) {
                    // handle collections
                    for (var i = 0; i < value.length; i++) {
                        this.validateViewModel(value[i]);
                    }
                }
                else if (value.$type) {
                    // handle nested objects
                    this.validateViewModel(value);
                }
            }
        }
    };
    /// Validates the specified property in the viewModel
    DotvvmValidation.prototype.validateProperty = function (viewModel, property, value, rulesForProperty) {
        for (var i = 0; i < rulesForProperty.length; i++) {
            // validate the rules
            var rule = rulesForProperty[i];
            var ruleTemplate = this.rules[rule.ruleName];
            var context = new DotvvmValidationContext(value, viewModel, rule.parameters);
            var validationError = ValidationError.getOrCreate(property);
            if (!ruleTemplate.isValid(context)) {
                // add error message
                validationError.errorMessage(rule.errorMessage);
                this.addValidationError(viewModel, validationError);
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
    DotvvmValidation.prototype.clearValidationErrors = function (viewModel) {
        this.clearValidationErrorsCore(viewModel);
        var errors = this.errors();
        for (var i = 0; i < errors.length; i++) {
            errors[i].errorMessage("");
        }
        this.errors.removeAll();
    };
    DotvvmValidation.prototype.clearValidationErrorsCore = function (viewModel) {
        viewModel = ko.unwrap(viewModel);
        if (!viewModel || !viewModel.$type)
            return;
        // clear validation errors
        if (viewModel.$validationErrors) {
            viewModel.$validationErrors.removeAll();
        }
        else {
            viewModel.$validationErrors = ko.observableArray([]);
        }
        // validate all properties
        for (var property in viewModel) {
            if (!viewModel.hasOwnProperty(property) || property.indexOf("$") === 0)
                continue;
            var viewModelProperty = viewModel[property];
            if (!viewModelProperty || !ko.isObservable(viewModelProperty))
                continue;
            var value = viewModel[property]();
            if (value) {
                if (Array.isArray(value)) {
                    // handle collections
                    for (var i = 0; i < value.length; i++) {
                        this.clearValidationErrorsCore(value[i]);
                    }
                }
                else if (value.$type) {
                    // handle nested objects
                    this.clearValidationErrorsCore(value);
                }
            }
        }
    };
    // get validation errors
    DotvvmValidation.prototype.getValidationErrors = function (viewModel, recursive) {
        viewModel = ko.unwrap(viewModel);
        if (!viewModel || !viewModel.$type || !viewModel.$validationErrors)
            return [];
        var errors = viewModel.$validationErrors();
        if (recursive) {
            // get child validation errors
            for (var property in viewModel) {
                if (!viewModel.hasOwnProperty(property) || property.indexOf("$") === 0)
                    continue;
                var viewModelProperty = viewModel[property];
                if (!viewModelProperty || !ko.isObservable(viewModelProperty))
                    continue;
                var value = viewModel[property]();
                if (value) {
                    if (Array.isArray(value)) {
                        // handle collections
                        for (var i = 0; i < value.length; i++) {
                            errors = errors.concat(this.getValidationErrors(value[i], recursive));
                        }
                    }
                    else if (value.$type) {
                        // handle nested objects
                        errors = errors.concat(this.getValidationErrors(value, recursive));
                    }
                }
            }
        }
        return errors;
    };
    // shows the validation errors from server
    DotvvmValidation.prototype.showValidationErrorsFromServer = function (args) {
        // resolve validation target
        var context = ko.contextFor(args.sender);
        var validationTarget = dotvvm.evaluator.evaluateOnViewModel(context, args.validationTargetPath);
        validationTarget = ko.unwrap(validationTarget);
        // add validation errors
        this.clearValidationErrors(args.viewModel);
        var modelState = args.serverResponseObject.modelState;
        for (var i = 0; i < modelState.length; i++) {
            // find the observable property
            var propertyPath = modelState[i].propertyPath;
            var observable = dotvvm.evaluator.evaluateOnViewModel(validationTarget, propertyPath);
            var parentPath = propertyPath.substring(0, propertyPath.lastIndexOf("."));
            var parent = parentPath ? dotvvm.evaluator.evaluateOnViewModel(validationTarget, parentPath) : validationTarget;
            parent = ko.unwrap(parent);
            if (!ko.isObservable(observable) || !parent || !parent.$validationErrors) {
                throw "Invalid validation path!";
            }
            // add the error to appropriate collections
            var error = ValidationError.getOrCreate(observable);
            error.errorMessage(modelState[i].errorMessage);
            this.addValidationError(parent, error);
        }
    };
    DotvvmValidation.prototype.addValidationError = function (viewModel, error) {
        if (viewModel.$validationErrors.indexOf(error) < 0) {
            viewModel.$validationErrors.push(error);
            this.errors.push(error);
        }
    };
    return DotvvmValidation;
})();
;
//# sourceMappingURL=DotVVM.js.map