/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout.mapper/knockout.mapper.d.ts" />
/// <reference path="typings/globalize/globalize.d.ts" />
interface RenderedResourceList {
    [name: string]: string;
}

class Redwood { 

    private postBackCounter = 0;
    private resourceSigns: { [name: string]: boolean } = {}

    private resourceLoadCallback: () => void;

    public extensions: any = {}; 
    public viewModels: any = {};
    public culture: string;
    public events = {
        init: new RedwoodEvent<RedwoodEventArgs>("redwood.events.init", true),
        beforePostback: new RedwoodEvent<RedwoodBeforePostBackEventArgs>("redwood.events.beforePostback"),
        afterPostback: new RedwoodEvent<RedwoodAfterPostBackEventArgs>("redwood.events.afterPostback"),
        error: new RedwoodEvent<RedwoodErrorEventArgs>("redwood.events.error"),
        spaNavigating: new RedwoodEvent<RedwoodSpaNavigatingEventArgs>("redwood.events.spaNavigating"),
        spaNavigated: new RedwoodEvent<RedwoodSpaNavigatedEventArgs>("redwood.events.spaNavigated")
    };

    public init(viewModelName: string, culture: string): void {
        this.culture = culture;
        var thisVm = this.viewModels[viewModelName] = JSON.parse((<HTMLInputElement>document.getElementById("__rw_viewmodel_" + viewModelName)).value);
        if (thisVm.renderedResources) {
            thisVm.renderedResources.forEach(r => this.resourceSigns[r] = true);
        }
        var viewModel = thisVm.viewModel = ko.mapper.fromJS(this.viewModels[viewModelName].viewModel);

        ko.applyBindings(viewModel, document.documentElement);
        this.events.init.trigger(new RedwoodEventArgs(viewModel));

        // handle SPA
        if (document.location.hash.indexOf("#!/") === 0) {
            this.navigateCore(viewModelName, document.location.hash.substring(2));
        }
        if (this.getSpaPlaceHolder()) {
            this.attachEvent(window, "hashchange",() => {
                if (document.location.hash.indexOf("#!/") === 0) {
                    this.navigateCore(viewModelName, document.location.hash.substring(2));
                }
            });
        }

        // persist the viewmodel in the hidden field so the Back button will work correctly
        this.attachEvent(window, "beforeunload", e => {
            this.persistViewModel(viewModelName);
        });
    }

    public onDocumentReady(callback: () => void) {
        // many thanks to http://dustindiaz.com/smallest-domready-ever
        /in/.test(document.readyState) ? setTimeout('redwood.onDocumentReady(' + callback + ')', 9) : callback();
    }

    private persistViewModel(viewModelName: string) {
        var viewModel = this.viewModels[viewModelName];
        var persistedViewModel = {};
        for (var p in viewModel) {
            if (viewModel.hasOwnProperty(p)) {
                persistedViewModel[p] = viewModel[p];
            }
        }
        persistedViewModel["viewModel"] = ko.mapper.toJS(persistedViewModel["viewModel"]);
        (<HTMLInputElement>document.getElementById("__rw_viewmodel_" + viewModelName)).value = JSON.stringify(persistedViewModel);
    }
    
    private backUpPostBackConter(): number {
        this.postBackCounter++;
        return this.postBackCounter;
    }

    private isPostBackStillActive(currentPostBackCounter: number): boolean {
        return this.postBackCounter === currentPostBackCounter;
    }

    public postBack(viewModelName: string, sender: HTMLElement, path: string[], command: string, controlUniqueId: string, validationTargetPath?: any): void {
        var viewModel = this.viewModels[viewModelName].viewModel;

        // prevent double postbacks
        var currentPostBackCounter = this.backUpPostBackConter();

        // trigger beforePostback event
        var beforePostbackArgs = new RedwoodBeforePostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath);
        this.events.beforePostback.trigger(beforePostbackArgs);
        if (beforePostbackArgs.cancel) {
            // trigger afterPostback event
            var afterPostBackArgsCanceled = new RedwoodAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, null);
            afterPostBackArgsCanceled.wasInterrupted = true;
            this.events.afterPostback.trigger(afterPostBackArgsCanceled);
            return;
        }

        // perform the postback
        this.updateDynamicPathFragments(sender, path);
        var data = {
            viewModel: ko.mapper.toJS(viewModel),
            currentPath: path,
            command: command,
            controlUniqueId: controlUniqueId,
            validationTargetPath: validationTargetPath || null,
            renderedResources: this.viewModels[viewModelName].renderedResources
        };
        this.postJSON(this.viewModels[viewModelName].url, "POST", ko.toJSON(data), result => {
            // if another postback has already been passed, don't do anything
            if (!this.isPostBackStillActive(currentPostBackCounter)) {
                var afterPostBackArgsCanceled = new RedwoodAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, null);
                afterPostBackArgsCanceled.wasInterrupted = true;
                this.events.afterPostback.trigger(afterPostBackArgsCanceled);
                return;
            }

            var resultObject = JSON.parse(result.responseText);
            if (!resultObject.viewModel && resultObject.viewModelDiff) {
                // TODO: patch (~deserialize) it to ko.observable viewModel
                resultObject.viewModel = this.patch(data.viewModel, resultObject.viewModelDiff);
            }

            this.loadResourceList(resultObject.resources, () => {
            var isSuccess = false;
            if (resultObject.action === "successfulCommand") {
                // remove updated controls
                var updatedControls = this.cleanUpdatedControls(resultObject);

                // update the viewmodel
                if (resultObject.viewModel)
                    ko.mapper.fromJS(resultObject.viewModel, {}, this.viewModels[viewModelName].viewModel);
                isSuccess = true;

                // add updated controls
                this.restoreUpdatedControls(resultObject, updatedControls, true);

            } else if (resultObject.action === "redirect") {
                // redirect
                this.navigateCore(viewModelName, resultObject.url);
                return;
            } 
            
            // trigger afterPostback event
            var afterPostBackArgs = new RedwoodAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, resultObject);
            this.events.afterPostback.trigger(afterPostBackArgs);
            if (!isSuccess && !afterPostBackArgs.isHandled) {
                throw "Invalid response from server!";
            }
            });
        }, xhr => {
            // if another postback has already been passed, don't do anything
            if (!this.isPostBackStillActive(currentPostBackCounter)) return;

            // execute error handlers
            var errArgs = new RedwoodErrorEventArgs(viewModel, xhr);
            this.events.error.trigger(errArgs);
            if (!errArgs.handled) {
                alert(xhr.responseText);
            }
        });
    }

    private loadResourceList(resources: RenderedResourceList, callback: () => void) {
        if (this.resourceLoadCallback) throw "Resource loading conflict";
        var html = "";
        for (var name in resources) {
            if (this.resourceSigns[name]) continue;

            this.resourceSigns[name] = true;

            html += resources[name] + " ";
        }
        if (html.trim() == "") {
            setTimeout(callback, 4);
            return;
        }
        else {
            this.resourceLoadCallback = callback;

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
        var el = elements[offset];
        if (el.tagName.toLowerCase() == "script") {
            // do some hacks to load script
            var script = document.createElement("script");
            script.src = (<HTMLScriptElement> el).src;
            script.type = (<HTMLScriptElement> el).type;
            script.text = (<HTMLScriptElement> el).text;
            el = script;
        }
        else if (el.tagName.toLowerCase() == "link") {
            var link = document.createElement("link");
            link.href = (<HTMLLinkElement>el).href;
            link.rel = (<HTMLLinkElement>el).rel;
            link.type = (<HTMLLinkElement>el).type;
            el = link;
        }
        el.onload = () => this.loadResourceElements(elements, offset + 1, callback);
        document.head.appendChild(el);
    }

    public evaluateOnViewModel(context, expression) {
        var result = eval("(function (c) { return c." + expression + "; })")(context);
        if (result && result.$data) {
            result = result.$data;
        }
        return result;
    }

    private getSpaPlaceHolder() {
        var elements = document.getElementsByName("__rw_SpaContentPlaceHolder");
        if (elements.length == 1) {
            return elements[0];
        }
        return null;
    }

    private navigateCore(viewModelName: string, url: string) {
        var viewModel = this.viewModels[viewModelName].viewModel;

        // prevent double postbacks
        var currentPostBackCounter = this.backUpPostBackConter();

        // trigger spaNavigating event
        var spaNavigatingArgs = new RedwoodSpaNavigatingEventArgs(viewModel, viewModelName, url);
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
        var spaPlaceHolderUniqueId = spaPlaceHolder.attributes["data-rw-spacontentplaceholder"].value;
        this.getJSON(fullUrl, "GET", spaPlaceHolderUniqueId, result => {
            // if another postback has already been passed, don't do anything
            if (!this.isPostBackStillActive(currentPostBackCounter)) return;

            var resultObject = JSON.parse(result.responseText);
            this.loadResourceList(resultObject.resources, () => {
            var isSuccess = false;
                if (resultObject.action === "successfulCommand" || !resultObject.action) {
                // remove updated controls
                var updatedControls = this.cleanUpdatedControls(resultObject);

                // update the viewmodel
                ko.cleanNode(document.documentElement);
                this.viewModels[viewModelName] = {};
                for (var p in resultObject) {
                    if (resultObject.hasOwnProperty(p)) {
                        this.viewModels[viewModelName][p] = resultObject[p];
                    }
                }
                ko.mapper.fromJS(resultObject.viewModel, {}, this.viewModels[viewModelName].viewModel);
                isSuccess = true;

                // add updated controls
                this.restoreUpdatedControls(resultObject, updatedControls, false);
                ko.applyBindings(this.viewModels[viewModelName].viewModel, document.documentElement);

            } else if (resultObject.action === "redirect") {
                // redirect
                document.location.href = resultObject.url;
                return;
            } 
            
            // trigger spaNavigated event
            var spaNavigatedArgs = new RedwoodSpaNavigatedEventArgs(viewModel, viewModelName, resultObject);
            this.events.spaNavigated.trigger(spaNavigatedArgs);
            if (!isSuccess && !spaNavigatedArgs.isHandled) {
                throw "Invalid response from server!";
            }
            });
        }, xhr => {
            // if another postback has already been passed, don't do anything
            if (!this.isPostBackStillActive(currentPostBackCounter)) return;

            // execute error handlers
            var errArgs = new RedwoodErrorEventArgs(viewModel, xhr, true);
            this.events.error.trigger(errArgs);
            if (!errArgs.handled) {
                alert(xhr.responseText);
            }
        });
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

    public formatString(format: string, value: any) {
        if (format == "g") {
            return redwood.formatString("d", value) + " " + redwood.formatString("t", value);
        } else if (format == "G") {
            return redwood.formatString("d", value) + " " + redwood.formatString("T", value);
        }

        value = ko.unwrap(value);
        if (typeof value === "string" && value.match("^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(\\.[0-9]{1,3})?$")) {
            // JSON date in string
            value = new Date(value);
        }
        return Globalize.format(value, format, redwood.culture);
    }

    public getDataSourceItems(viewModel: any) {
        var value = ko.unwrap(viewModel);
        return value.Items || value;
    }

    private updateDynamicPathFragments(sender: HTMLElement, path: string[]): void {
        var context = ko.contextFor(sender);

        for (var i = path.length - 1; i >= 0; i--) {
            if (path[i].indexOf("[$index]") >= 0) {
                path[i] = path[i].replace("[$index]", "[" + context.$index() + "]");
            }
            context = context.$parentContext;
        }
    }

    private postJSON(url: string, method: string, postData: any, success: (request: XMLHttpRequest) => void, error: (response: XMLHttpRequest) => void) {
        var xhr = this.getXHR();
        xhr.open(method, url, true);
        xhr.setRequestHeader("Content-Type", "application/json");
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
        xhr.open("GET", url, true);
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.setRequestHeader("X-Redwood-SpaContentPlaceHolder", spaPlaceHolderUniqueId);
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

    private getXHR(): XMLHttpRequest {
        return XMLHttpRequest ? new XMLHttpRequest() : <XMLHttpRequest>new ActiveXObject("Microsoft.XMLHTTP");
    }
    
    private cleanUpdatedControls(resultObject: any) {
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
        return updatedControls;
    }

    private restoreUpdatedControls(resultObject: any, updatedControls: any, applyBindingsOnEachControl: boolean) {
        for (var id in resultObject.updatedControls) {
            if (resultObject.updatedControls.hasOwnProperty(id)) {
                var updatedControl = updatedControls[id];
                if (updatedControl.nextSibling) {
                    updatedControl.parent.insertBefore(updatedControl.control, updatedControl.nextSibling);
                } else {
                    updatedControl.parent.appendChild(updatedControl.control);
                }
                updatedControl.control.outerHTML = resultObject.updatedControls[id];

                if (applyBindingsOnEachControl) {
                    ko.applyBindings(ko.dataFor(updatedControl.parent), updatedControl.control);
                }
            }
        }
    }

    private attachEvent(target: any, name: string, callback: (ev: PointerEvent) => any, useCapture: boolean = false) {
        if (target.addEventListener) {
            target.addEventListener(name, callback, useCapture);
        }
        else {
            target.attachEvent("on" + name, callback);
        }
    }

    public buildRouteUrl(routePath: string, params: any): string {
        return routePath.replace(/\{[^\}]+\}/g, s => params[s.substring(1, s.length - 1)] || "");
    }
}

// RedwoodEvent is used because CustomEvent is not browser compatible and does not support 
// calling missed events for handler that subscribed too late.
class RedwoodEvent<T extends RedwoodEventArgs> {
    private handlers = [];
    private history = [];

    constructor(public name: string, private triggerMissedEventsOnSubscribe: boolean = false) {
    }

    public subscribe(handler: (data: T) => void) {
        this.handlers.push(handler);

        if (this.triggerMissedEventsOnSubscribe) {
            for (var i = 0; i < this.history.length; i++) {
                handler(history[i]);
            }
        }
    }

    public unsubscribe(handler: (data: T) => void) {
        var index = this.handlers.indexOf(handler);
        if (index >= 0) {
            this.handlers = this.handlers.splice(index, 1);
        }
    }

    public trigger(data: T): void {
        for (var i = 0; i < this.handlers.length; i++) {
            this.handlers[i](data);
        }

        if (this.triggerMissedEventsOnSubscribe) {
            this.history.push(data);
        }
    }
}

class RedwoodEventArgs {
    constructor(public viewModel: any) {
    }
}
class RedwoodErrorEventArgs extends RedwoodEventArgs {
    public handled = false;
    constructor(public viewModel: any, public xhr: XMLHttpRequest, public isSpaNavigationError: boolean = false) {
        super(viewModel);
    }
}
class RedwoodBeforePostBackEventArgs extends RedwoodEventArgs {
    public cancel: boolean = false;
    public clientValidationFailed: boolean = false;
    constructor(public sender: HTMLElement, public viewModel: any, public viewModelName: string, public validationTargetPath: any) {
        super(viewModel);
    }
}
class RedwoodAfterPostBackEventArgs extends RedwoodEventArgs {
    public isHandled: boolean = false;
    public wasInterrupted: boolean = false;
    constructor(public sender: HTMLElement, public viewModel: any, public viewModelName: string, public validationTargetPath: any, public serverResponseObject: any) {
        super(viewModel);
    }
}
class RedwoodSpaNavigatingEventArgs extends RedwoodEventArgs {
    public cancel: boolean = false;
    constructor(public viewModel: any, public viewModelName: string, public newUrl: string) {
        super(viewModel);
    }
}
class RedwoodSpaNavigatedEventArgs extends RedwoodEventArgs {
    public isHandled: boolean = false;
    constructor(public viewModel: any, public viewModelName: string, public serverResponseObject: any) {
        super(viewModel);
    }
}

var redwood = new Redwood();


// add knockout binding handler for update progress control
ko.bindingHandlers["redwoodUpdateProgressVisible"] = {
    init(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
        element.style.display = "none";
        redwood.events.beforePostback.subscribe(e => {
            element.style.display = "";
        });
        redwood.events.spaNavigating.subscribe(e => {
            element.style.display = "";
        });
        redwood.events.afterPostback.subscribe(e => {
            element.style.display = "none";
        });
        redwood.events.spaNavigated.subscribe(e => {
            element.style.display = "none";
        });
        redwood.events.error.subscribe(e => {
            element.style.display = "none";
        });
    }
};