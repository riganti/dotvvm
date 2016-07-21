/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout.mapper/knockout.mapper.d.ts" />
/// <reference path="typings/globalize/globalize.d.ts" />

interface Document {
    getElementByDotvvmId(id: string): HTMLElement;
}

document.getElementByDotvvmId = function (id) {
    return <HTMLElement>document.querySelector(`[data-dotvvm-id='${id}'`);
}

interface IRenderedResourceList {
    [name: string]: string;
}

interface IDotvvmPostbackScriptFunction {
    (pageArea: string, sender: HTMLElement, pathFragments: string[], controlId: string, useWindowSetTimeout: boolean, validationTarget: string, context: any, handlers: IDotvvmPostBackHandlerConfiguration[]): void
}

interface IDotvvmExtensions {
}

interface IDotvvmViewModelInfo {
    viewModel?;
    renderedResources?: string[];
    url?: string;
    virtualDirectory?: string;
}

interface IDotvvmViewModels {
    [name: string]: IDotvvmViewModelInfo
}

class DotVVM {
    private postBackCounter = 0;
    private fakeRedirectAnchor: HTMLAnchorElement;
    private resourceSigns: { [name: string]: boolean } = {}
    private isViewModelUpdating: boolean = true;
    private viewModelObservables: {
        [name: string]: KnockoutObservable<IDotvvmViewModelInfo>;
    } = {};

    public isSpaReady = ko.observable(false);
    public viewModels: IDotvvmViewModels = {};
    public culture: string;
    public serialization = new DotvvmSerialization();
    public postBackHandlers = new DotvvmPostBackHandlers();
    public events = new DotvvmEvents();
    public globalize = new DotvvmGlobalize();
    public evaluator = new DotvvmEvaluator();
    public domUtils = new DotvvmDomUtils();
    public fileUpload = new DotvvmFileUpload();
    public validation: DotvvmValidation;
    public extensions: IDotvvmExtensions = {};

    public isPostbackRunning = ko.observable(false);

    public init(viewModelName: string, culture: string): void {
        this.addKnockoutBindingHandlers();

        // load the viewmodel
        var thisViewModel = this.viewModels[viewModelName] = JSON.parse((<HTMLInputElement>document.getElementById("__dot_viewmodel_" + viewModelName)).value);
        if (thisViewModel.renderedResources) {
            thisViewModel.renderedResources.forEach(r => this.resourceSigns[r] = true);
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
            this.domUtils.attachEvent(window, "hashchange", () => this.handleHashChange(viewModelName, spaPlaceHolder, false));
            this.handleHashChange(viewModelName, spaPlaceHolder, true);
        }
        this.isViewModelUpdating = false;

        if (idFragment) {
            if (spaPlaceHolder) {
                var element = document.getElementById(idFragment);
                if (element && "function" == typeof element.scrollIntoView) element.scrollIntoView(true);
            }
            else location.hash = idFragment;
        }

        // persist the viewmodel in the hidden field so the Back button will work correctly
        this.domUtils.attachEvent(window, "beforeunload", e => {
            this.persistViewModel(viewModelName);
        });
    }

    private handleHashChange(viewModelName: string, spaPlaceHolder: HTMLElement, isInitialPageLoad: boolean) {
        if (document.location.hash.indexOf("#!/") === 0) {
            this.navigateCore(viewModelName, document.location.hash.substring(2));
        } else {
            // redirect to the default URL
            var url = spaPlaceHolder.getAttribute("data-dotvvm-spacontentplaceholder-defaultroute");
            if (url) {
                url = "#!/" + url;
                url = this.fixSpaUrlPrefix(url);
                this.performRedirect(url, false);
            } else {
                this.isSpaReady(true);
                spaPlaceHolder.style.display = "";
            }
        }
    }

    // binding helpers
    private postbackScript(bindingId: string): IDotvvmPostbackScriptFunction {
        return (pageArea, sender, pathFragments, controlId, useWindowSetTimeout, validationTarget, context, handlers) => {
            this.postBack(pageArea, sender, pathFragments, bindingId, controlId, useWindowSetTimeout, validationTarget, context, handlers);
        }
    }

    private persistViewModel(viewModelName: string) {
        var viewModel = this.viewModels[viewModelName];
        var persistedViewModel = {};
        for (var p in viewModel) {
            if (viewModel.hasOwnProperty(p)) {
                persistedViewModel[p] = viewModel[p];
            }
        }
        persistedViewModel["viewModel"] = this.serialization.serialize(persistedViewModel["viewModel"], { serializeAll: true });
        (<HTMLInputElement>document.getElementById("__dot_viewmodel_" + viewModelName)).value = JSON.stringify(persistedViewModel);
    }

    private backUpPostBackConter(): number {
        this.postBackCounter++;
        return this.postBackCounter;
    }

    private isPostBackStillActive(currentPostBackCounter: number): boolean {
        return this.postBackCounter === currentPostBackCounter;
    }

    public staticCommandPostback(viewModelName: string, sender: HTMLElement, command: string, args: any[], callback = _ => { }, errorCallback = (xhr: XMLHttpRequest, error?) => { }) {
        if (this.isPostBackProhibited(sender)) return;

        // TODO: events for static command postback

        // prevent double postbacks
        var currentPostBackCounter = this.backUpPostBackConter();

        var data = this.serialization.serialize({
            "args": args,
            "command": command,
            "$csrfToken": this.viewModels[viewModelName].viewModel.$csrfToken
        });

        this.postJSON(this.viewModels[viewModelName].url, "POST", ko.toJSON(data), response => {
            if (!this.isPostBackStillActive(currentPostBackCounter)) return;
            try {
                callback(JSON.parse(response.responseText));
            }
            catch (error) {
                errorCallback(response, error);
            }
        }, errorCallback,
            xhr => {
                xhr.setRequestHeader("X-PostbackType", "StaticCommand");
            });
    }

    public postBack(viewModelName: string, sender: HTMLElement, path: string[], command: string, controlUniqueId: string, useWindowSetTimeout: boolean, validationTargetPath?: any, context?: any, handlers?: IDotvvmPostBackHandlerConfiguration[]): IDotvvmPromise<DotvvmAfterPostBackEventArgs> {
        if (this.isPostBackProhibited(sender)) return new DotvvmPromise<DotvvmAfterPostBackEventArgs>().reject("rejected");

        var promise = new DotvvmPromise<DotvvmAfterPostBackEventArgs>();
        this.isPostbackRunning(true);
        promise.done(() => this.isPostbackRunning(false));
        promise.fail(() => this.isPostbackRunning(false));
        if (useWindowSetTimeout) {
            window.setTimeout(() => promise.chainFrom(this.postBack(viewModelName, sender, path, command, controlUniqueId, false, validationTargetPath, context, handlers)), 0);
            return promise;
        }

        // apply postback handlers
        if (handlers && handlers.length > 0) {
            var handler = this.postBackHandlers[handlers[0].name];
            var options = this.evaluator.evaluateOnViewModel(ko.contextFor(sender), "(" + handlers[0].options.toString() + ")()");
            var handlerInstance = handler(options);
            var nextHandler = () => promise.chainFrom(this.postBack(viewModelName, sender, path, command, controlUniqueId, false, validationTargetPath, context, handlers.slice(1)));
            if (options.enabled) {
                handlerInstance.execute(nextHandler, sender);
            } else {
                nextHandler();
            }
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
            return promise.reject("canceled");
        }

        // perform the postback
        if (!context) {
            context = ko.contextFor(sender);
        }
        this.updateDynamicPathFragments(context, path);
        var data = {
            viewModel: this.serialization.serialize(viewModel, { pathMatcher(val) { return context && val == context.$data } }),
            currentPath: path,
            command: command,
            controlUniqueId: controlUniqueId,
            validationTargetPath: validationTargetPath || null,
            renderedResources: this.viewModels[viewModelName].renderedResources
        };
        this.postJSON(this.viewModels[viewModelName].url, "POST", ko.toJSON(data), result => {
            // if another postback has already been passed, don't do anything
            if (!this.isPostBackStillActive(currentPostBackCounter)) {
                var afterPostBackArgsCanceled = new DotvvmAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, null);
                afterPostBackArgsCanceled.wasInterrupted = true;
                this.events.afterPostback.trigger(afterPostBackArgsCanceled);
                promise.reject("postback collision");
                return;
            }
            try {
                var resultObject = JSON.parse(result.responseText);
                if (!resultObject.viewModel && resultObject.viewModelDiff) {
                    // TODO: patch (~deserialize) it to ko.observable viewModel
                    this.isViewModelUpdating = true;
                    resultObject.viewModel = this.patch(data.viewModel, resultObject.viewModelDiff);
                }

                this.loadResourceList(resultObject.resources, () => {
                    var isSuccess = false;
                    if (resultObject.action === "successfulCommand") {
                        this.isViewModelUpdating = true;

                        // remove updated controls
                        var updatedControls = this.cleanUpdatedControls(resultObject);

                        // update the viewmodel
                        if (resultObject.viewModel) {
                            this.serialization.deserialize(resultObject.viewModel, this.viewModels[viewModelName].viewModel);
                        }
                        isSuccess = true;

                        // remove updated controls which were previously hidden
                        this.cleanUpdatedControls(resultObject, updatedControls);

                        // add updated controls
                        this.restoreUpdatedControls(resultObject, updatedControls, true);
                        this.isViewModelUpdating = false;
                    } else if (resultObject.action === "redirect") {
                        // redirect
                        this.handleRedirect(resultObject, viewModelName);
                        return;
                    }

                    var idFragment = resultObject.resultIdFragment;
                    if (idFragment) {
                        if (this.getSpaPlaceHolder() || location.hash == "#" + idFragment) {
                            var element = document.getElementById(idFragment);
                            if (element && "function" == typeof element.scrollIntoView) element.scrollIntoView(true);
                        }
                        else location.hash = idFragment;
                    }

                    // trigger afterPostback event
                    var afterPostBackArgs = new DotvvmAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, resultObject);
                    promise.resolve(afterPostBackArgs);
                    this.events.afterPostback.trigger(afterPostBackArgs);
                    if (!isSuccess && !afterPostBackArgs.isHandled) {
                        this.error(viewModel, result, promise);
                    }
                });
            }
            catch (error) {
                this.error(viewModel, result, promise);
            }
        }, xhr => {
            // if another postback has already been passed, don't do anything
            if (!this.isPostBackStillActive(currentPostBackCounter)) return;
            this.error(viewModel, xhr, promise);
        });
        return promise;
    }

    private error(viewModel, xhr: XMLHttpRequest, promise: DotvvmPromise<any> = null) {
        // execute error handlers
        var errArgs = new DotvvmErrorEventArgs(viewModel, xhr);
        if (promise != null) promise.reject(errArgs);
        this.events.error.trigger(errArgs);
        if (!errArgs.handled) {
            alert("unhandled error during postback");
        }
    }

    private loadResourceList(resources: IRenderedResourceList, callback: () => void) {
        var html = "";
        for (var name in resources) {
            if (!/^__noname_\d+$/.test(name)) {
                if (this.resourceSigns[name]) continue;
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
            var elements: HTMLElement[] = [];
            for (var i = 0; i < tmp.children.length; i++) {
                elements.push(<HTMLElement>tmp.children.item(i));
            }
            this.loadResourceElements(elements, 0, callback);
        }
    }

    private loadResourceElements(elements: HTMLElement[], offset: number, callback: () => void) {
        if (offset >= elements.length) {
            callback();
            return;
        }
        var el = <any>elements[offset];
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
            el.onload = () => this.loadResourceElements(elements, offset + 1, callback);
        }
        document.head.appendChild(el);
        if (!waitForScriptLoaded) {
            this.loadResourceElements(elements, offset + 1, callback);
        }
    }

    private getSpaPlaceHolder(): HTMLElement {
        var elements = document.getElementsByName("__dot_SpaContentPlaceHolder");
        if (elements.length == 1) {
            return <HTMLElement>elements[0];
        }
        return null;
    }

    private navigateCore(viewModelName: string, url: string) {
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
        var spaPlaceHolderUniqueId = spaPlaceHolder.attributes["data-dotvvm-spacontentplaceholder"].value;
        this.getJSON(fullUrl, "GET", spaPlaceHolderUniqueId, result => {
            // if another postback has already been passed, don't do anything
            if (!this.isPostBackStillActive(currentPostBackCounter)) return;

            var resultObject = JSON.parse(result.responseText);
            this.loadResourceList(resultObject.resources, () => {
                var isSuccess = false;
                if (resultObject.action === "successfulCommand" || !resultObject.action) {
                    this.isViewModelUpdating = true;

                    // remove updated controls
                    var updatedControls = this.cleanUpdatedControls(resultObject);

                    // update the viewmodel
                    this.viewModels[viewModelName] = {};
                    for (var p in resultObject) {
                        if (resultObject.hasOwnProperty(p)) {
                            this.viewModels[viewModelName][p] = resultObject[p];
                        }
                    }

                    this.serialization.deserialize(resultObject.viewModel, this.viewModels[viewModelName].viewModel);
                    isSuccess = true;

                    // add updated controls
                    this.viewModelObservables[viewModelName](this.viewModels[viewModelName].viewModel);
                    this.restoreUpdatedControls(resultObject, updatedControls, true);

                    this.isSpaReady(true);
                    this.isViewModelUpdating = false;
                } else if (resultObject.action === "redirect") {
                    this.handleRedirect(resultObject, viewModelName, true);
                    return;
                }

                // trigger spaNavigated event
                var spaNavigatedArgs = new DotvvmSpaNavigatedEventArgs(viewModel, viewModelName, resultObject);
                this.events.spaNavigated.trigger(spaNavigatedArgs);
                if (!isSuccess && !spaNavigatedArgs.isHandled) {
                    throw "Invalid response from server!";
                }
            });
        }, xhr => {
            // if another postback has already been passed, don't do anything
            if (!this.isPostBackStillActive(currentPostBackCounter)) return;

            // execute error handlers
            var errArgs = new DotvvmErrorEventArgs(viewModel, xhr, true);
            this.events.error.trigger(errArgs);
            if (!errArgs.handled) {
                alert(xhr.responseText);
            }
        });
    }

    private handleRedirect(resultObject: any, viewModelName: string, replace?: boolean) {
        if (resultObject.replace != null) replace = resultObject.replace;
        var url;
        // redirect
        if (this.getSpaPlaceHolder() && resultObject.url.indexOf("//") < 0 && !replace) {
            // relative URL - keep in SPA mode, but remove the virtual directory
            url = "#!" + this.removeVirtualDirectoryFromUrl(resultObject.url, viewModelName);
            if (url === "#!") {
                url = "#!/";
            }

            // verify that the URL prefix is correct, if not - add it before the fragment
            url = this.fixSpaUrlPrefix(url);

        } else {
            // absolute URL - load the URL
            url = resultObject.url;
        }

        // trigger redirect event
        var redirectArgs = new DotvvmRedirectEventArgs(dotvvm.viewModels[viewModelName], viewModelName, url, replace);
        this.events.redirect.trigger(redirectArgs);

        this.performRedirect(url, replace);
    }

    private performRedirect(url: string, replace: boolean) {
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
    }

    private fixSpaUrlPrefix(url: string): string {
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
    }

    private removeVirtualDirectoryFromUrl(url: string, viewModelName: string) {
        var virtualDirectory = "/" + this.viewModels[viewModelName].virtualDirectory;
        if (url.indexOf(virtualDirectory) == 0) {
            return this.addLeadingSlash(url.substring(virtualDirectory.length));
        } else {
            return url;
        }
    }

    private addLeadingSlash(url: string) {
        if (url.length > 0 && url.substring(0, 1) != "/") {
            return "/" + url;
        }
        return url;
    }

    private concatUrl(url1: string, url2: string) {
        if (url1.length > 0 && url1.substring(url1.length - 1) == "/") {
            url1 = url1.substring(0, url1.length - 1);
        }
        return url1 + this.addLeadingSlash(url2);
    }

    public patch(source: any, patch: any): any {
        if (source instanceof Array && patch instanceof Array) {
            return patch.map((val, i) =>
                this.patch(source[i], val));
        }
        else if (source instanceof Array || patch instanceof Array)
            return patch;
        else if (typeof source == "object" && typeof patch == "object") {
            for (var p in patch) {
                if (patch[p] == null) source[p] = null;
                else if (source[p] == null) source[p] = patch[p];
                else source[p] = this.patch(source[p], patch[p]);
            }
        }
        else return patch;

        return source;
    }

    private updateDynamicPathFragments(context: any, path: string[]): void {
        for (var i = path.length - 1; i >= 0; i--) {
            if (path[i].indexOf("[$index]") >= 0) {
                path[i] = path[i].replace("[$index]", "[" + context.$index() + "]");
            }
            context = context.$parentContext;
        }
    }

    private postJSON(url: string, method: string, postData: any, success: (request: XMLHttpRequest) => void, error: (response: XMLHttpRequest) => void, preprocessRequest = (xhr: XMLHttpRequest) => { }) {
        var xhr = this.getXHR();
        xhr.open(method, url, true);
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.setRequestHeader("X-DotVVM-PostBack", "true");
        preprocessRequest(xhr);
        xhr.onreadystatechange = () => {
            if (xhr.readyState != 4) return;
            if (xhr.status < 400) {
                success(xhr);
            } else {
                error(xhr);
            }
        };
        xhr.send(postData);
    }

    private getJSON(url: string, method: string, spaPlaceHolderUniqueId: string, success: (request: XMLHttpRequest) => void, error: (response: XMLHttpRequest) => void) {
        var xhr = this.getXHR();
        xhr.open(method, url, true);
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.setRequestHeader("X-DotVVM-SpaContentPlaceHolder", spaPlaceHolderUniqueId);
        xhr.onreadystatechange = () => {
            if (xhr.readyState != 4) return;
            if (xhr.status < 400) {
                success(xhr);
            } else {
                error(xhr);
            }
        };
        xhr.send();
    }

    public getXHR(): XMLHttpRequest {
        return XMLHttpRequest ? new XMLHttpRequest() : <XMLHttpRequest>new ActiveXObject("Microsoft.XMLHTTP");
    }

    private cleanUpdatedControls(resultObject: any, updatedControls: any = {}) {
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
    }

    private restoreUpdatedControls(resultObject: any, updatedControls: any, applyBindingsOnEachControl: boolean) {
        for (var id in resultObject.updatedControls) {
            if (resultObject.updatedControls.hasOwnProperty(id)) {
                var updatedControl = updatedControls[id];
                if (updatedControl) {
                    var wrapper = document.createElement(updatedControls[id].parent.tagName || "div");
                    wrapper.innerHTML = resultObject.updatedControls[id];
                    if (wrapper.childElementCount > 1) throw new Error("Postback.Update control can not render more than one element");
                    var element = wrapper.firstElementChild;
                    if (element.id == null) throw new Error("Postback.Update control always has to render id attribute.");
                    if (element.id !== updatedControls[id].control.id) console.log(`Postback.Update control changed id from '${updatedControls[id].control.id}' to '${element.id}'`);
                    wrapper.removeChild(element);
                    if (updatedControl.nextSibling) {
                        updatedControl.parent.insertBefore(element, updatedControl.nextSibling);
                    } else {
                        updatedControl.parent.appendChild(element);
                    }
                }
            }
        }

        if (applyBindingsOnEachControl) {
            window.setTimeout(() => {
                for (var id in resultObject.updatedControls) {
                    var updatedControl = document.getElementByDotvvmId(id);
                    if (updatedControl) {
                        ko.applyBindings(updatedControls[id].dataContext, updatedControl);
                    }
                }
            }, 0);
        }
    }

    public unwrapArrayExtension(array: any): any {
        return ko.unwrap(ko.unwrap(array));
    }
    public buildRouteUrl(routePath: string, params: any): string {
        return routePath.replace(/\{([^\}]+?)\??(:(.+?))?\}/g, (s, paramName, hsjdhsj, type) => {
            if (!paramName) return "";
            return ko.unwrap(params[paramName.toLowerCase()]) || "";
        });
    }

    private isPostBackProhibited(element: HTMLElement) {
        if (element.tagName.toLowerCase() === "a" && element.getAttribute("disabled")) {
            return true;
        }
        return false;
    }

    private addKnockoutBindingHandlers() {
        function createWrapperComputed(accessor: () => any, propertyDebugInfo: string = null) {
            var computed = ko.pureComputed({
                read() {
                    var property = accessor();
                    var propertyValue = ko.unwrap(property); // has to call that always as it is a dependency
                    return propertyValue;
                },
                write(value) {
                    var val = accessor();
                    if (ko.isObservable(val)) {
                        val(value);
                    }
                    else {
                        console.warn(`Attempted to write to readonly property` + (propertyDebugInfo == null ? `` : ` ` + propertyDebugInfo) + `.`);
                    }
                }
            });
            computed["wrappedProperty"] = accessor;
            return computed;
        }

        ko.virtualElements.allowedBindings["dotvvm_withControlProperties"] = true;
        ko.bindingHandlers["dotvvm_withControlProperties"] = {
            init: (element, valueAccessor, allBindings, viewModel, bindingContext) => {
                var value = valueAccessor();
                for (var prop in value) {
                    value[prop] = createWrapperComputed(function () { return valueAccessor()[this.prop]; }.bind({ prop: prop }), `'${prop}' at '${valueAccessor.toString()}'`);
                }
                var innerBindingContext = bindingContext.extend({ $control: value });
                element.innerBindingContext = innerBindingContext;
                ko.applyBindingsToDescendants(innerBindingContext, element);
                return { controlsDescendantBindings: true }; // do not apply binding again
            },
            update(element, valueAccessor, allBindings, viewModel, bindingContext) {
            }
        };

        ko.virtualElements.allowedBindings["dotvvm_introduceAlias"] = true;
        ko.bindingHandlers["dotvvm_introduceAlias"] = {
            init(element, valueAccessor, allBindings, viewModel, bindingContext) {
                var value = valueAccessor();
                var extendBy = {};
                for (var prop in value) {
                    var propPath = prop.split('.');
                    var obj = extendBy;
                    for (var i = 0; i < propPath.length - 1; i) {
                        obj = extendBy[propPath[i]] || (extendBy[propPath[i]] = {});
                    }
                    obj[propPath[propPath.length - 1]] = createWrapperComputed(function () { return valueAccessor()[this.prop] }.bind({ prop: prop }), `'${prop}' at '${valueAccessor.toString()}'`);
                }
                var innerBindingContext = bindingContext.extend(extendBy);
                element.innerBindingContext = innerBindingContext;
                ko.applyBindingsToDescendants(innerBindingContext, element);
                return { controlsDescendantBindings: true }; // do not apply binding again
            }
        }

        ko.virtualElements.allowedBindings["withGridViewDataSet"] = true;
        ko.bindingHandlers["withGridViewDataSet"] = {
            init: (element, valueAccessor, allBindings, viewModel, bindingContext) => {
                var value = valueAccessor();
                var innerBindingContext = bindingContext.extend({ $gridViewDataSet: value });
                element.innerBindingContext = innerBindingContext;
                ko.applyBindingsToDescendants(innerBindingContext, element);
                return { controlsDescendantBindings: true }; // do not apply binding again
            },
            update(element, valueAccessor, allBindings, viewModel, bindingContext) {
            }
        };

        ko.bindingHandlers['dotvvmEnable'] = {
            'update': (element, valueAccessor) => {
                var value = ko.utils.unwrapObservable(valueAccessor());
                if (value && element.disabled) {
                    element.disabled = false;
                    element.removeAttribute("disabled");
                } else if ((!value) && (!element.disabled)) {
                    element.disabled = true;
                    element.setAttribute("disabled", "disabled");
                }
            }
        };
        ko.bindingHandlers['dotvvm-checkbox-updateAfterPostback'] = {
            init(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
             dotvvm.events.afterPostback.subscribe((e) => {
                 var bindings = allBindingsAccessor();
                 if (bindings["dotvvm-checked-pointer"]) {
                     var checked = bindings[bindings["dotvvm-checked-pointer"]];
                     if (ko.isObservable(checked)) {
                         if ((<KnockoutObservable<any>>checked).valueHasMutated) {
                             (<KnockoutObservable<any>>checked).valueHasMutated();
                         } else {
                             (<KnockoutObservable<any>>checked).notifySubscribers();
                         }
                     }
                 }
             });
            }
        };
        ko.bindingHandlers['dotvvm-checked-pointer'] = {
            init(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
            }
        };

        ko.bindingHandlers["dotvvm-UpdateProgress-Visible"] = {
            init(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
                element.style.display = "none";
                dotvvm.events.beforePostback.subscribe(e => {
                    element.style.display = "";
                });
                dotvvm.events.spaNavigating.subscribe(e => {
                    element.style.display = "";
                });
                dotvvm.events.afterPostback.subscribe(e => {
                    element.style.display = "none";
                });
                dotvvm.events.redirect.subscribe(e => {
                    element.style.display = "none";
                });
                dotvvm.events.spaNavigated.subscribe(e => {
                    element.style.display = "none";
                });
                dotvvm.events.error.subscribe(e => {
                    element.style.display = "none";
                });
            }
        };
        ko.bindingHandlers['dotvvm-textbox-text'] = {
            init(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
                var obs = valueAccessor();

                //generate metadata func 
                var elmMetadata = new DotvvmValidationElementMetadata();
                elmMetadata.dataType = (element.attributes["data-dotvvm-value-type"] || { value: "" }).value;
                elmMetadata.format = (element.attributes["data-dotvvm-format"] || { value: "" }).value;

                //add metadata for validation
                if (!obs.dotvvmMetadata) {
                    obs.dotvvmMetadata = new DotvvmValidationObservableMetadata();
                    obs.dotvvmMetadata.elementsMetadata = [elmMetadata];
                } else {
                    if (!obs.dotvvmMetadata.elementsMetadata) {
                        obs.dotvvmMetadata.elementsMetadata = [];
                    }
                    obs.dotvvmMetadata.elementsMetadata.push(elmMetadata);
                }
                setTimeout((metaArray: DotvvmValidationElementMetadata[], element:HTMLElement) => {
                    // remove element from collection when its removed from dom
                    ko.utils.domNodeDisposal.addDisposeCallback(element, () => {
                        for (var meta of metaArray) {
                            if (meta.element === element) {
                                metaArray.splice(metaArray.indexOf(meta), 1);
                                break;
                            }
                        }
                    });
                }, 0, obs.dotvvmMetadata.elementsMetadata, element);


                dotvvm.domUtils.attachEvent(element, "blur", () => {

                    // parse the value
                    var result, isEmpty, newValue;
                    if (elmMetadata.dataType === "datetime") {
                        // parse date
                        result = dotvvm.globalize.parseDate(element.value, elmMetadata.format);
                        isEmpty = result === null;
                        newValue = isEmpty ? null : dotvvm.serialization.serializeDate(result, false);
                    } else {
                        // parse number
                        result = dotvvm.globalize.parseNumber(element.value);
                        isEmpty = result === null || isNaN(result);
                        newValue = isEmpty ? null : result;
                    }

                    // update element validation metadata
                    if (newValue == null && element.value !== null && element.value !== "") {
                        element.attributes["data-dotvvm-value-type-valid"] = false;
                        elmMetadata.elementValidationState = false;
                    } else {
                        element.attributes["data-dotvvm-value-type-valid"] = true;
                        elmMetadata.elementValidationState = true;
                    }

                    if (obs() === newValue) {
                        if ((<KnockoutObservable<number>>obs).valueHasMutated) {
                            (<KnockoutObservable<number>>obs).valueHasMutated();
                        } else {
                            (<KnockoutObservable<number>>obs).notifySubscribers();
                        }
                    } else {
                        obs(newValue);
                    }
                });
            },
            update(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
                var value = ko.unwrap(valueAccessor());
                if (element.attributes["data-dotvvm-value-type-valid"] != false) {
                    var format = (element.attributes["data-dotvvm-format"] || { value: "" }).value;
                    if (format) {
                        element.value = dotvvm.globalize.formatString(format, value);
                    } else {
                        element.value = value;
                    }
                }
            }
        };
    }
}