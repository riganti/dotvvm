/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout.mapper/knockout.mapper.d.ts" />

interface RedwoodExtensions {
}
interface RedwoodViewModel {
    viewModel: any;
}

class Redwood {

    public extensions: RedwoodExtensions = {};
    public viewModels: { [name: string]: RedwoodViewModel } = {};
    public culture: string;
    public events = {
        init: new RedwoodEvent<RedwoodEventArgs>("redwood.events.init", true),
        beforePostback: new RedwoodEvent<RedwoodBeforePostBackEventArgs>("redwood.events.beforePostback"),
        afterPostback: new RedwoodEvent<RedwoodAfterPostBackEventArgs>("redwood.events.afterPostback"),
        error: new RedwoodEvent<RedwoodErrorEventArgs>("redwood.events.error")
    };

    public includeParentNameProps(viewModel: any, parentProp: string = null) {
        viewModel.$parentProp = parentProp;
        for (var p in viewModel) {
            if (typeof viewModel[p] === "object" && viewModel[p] != null && p.charAt(0) != "$") {
                if (viewModel[p] instanceof Array) viewModel[p].forEach(v => this.includeParentNameProps(v, p))
                else this.includeParentNameProps(viewModel[p], p);
            }
        }
    }

    public init(viewModelName: string, culture: string): void {
        this.culture = culture;
        this.includeParentNameProps(this.viewModels[viewModelName].viewModel);
        this.viewModels[viewModelName].viewModel = ko.mapper.fromJS(this.viewModels[viewModelName].viewModel);

        var viewModel = this.viewModels[viewModelName].viewModel;
        ko.applyBindings(viewModel);
        this.events.init.trigger(new RedwoodEventArgs(viewModel));
    }

    public postBack(viewModelName: string, sender: HTMLElement, path: string[], command: string, controlUniqueId: string, validationTargetPath?: string): void {
        var viewModel = this.viewModels[viewModelName].viewModel;

        // trigger beforePostback event
        var beforePostbackArgs = new RedwoodBeforePostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, path, command);
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
        this.postJSON(document.location.href, "POST", ko.toJSON(data), result => {
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
                this.includeParentNameProps(resultObject.viewModel);
                // update the viewmodel
                ko.mapper.fromJS(resultObject.viewModel, {}, this.viewModels[viewModelName].viewModel);
                isSuccess = true;

                // add updated controls
                for (id in resultObject.updatedControls) {
                    if (resultObject.updatedControls.hasOwnProperty(id)) {
                        var updatedControl = updatedControls[id];
                        if (updatedControl.nextSibling) {
                            updatedControl.parent.insertBefore(updatedControl.control, updatedControl.nextSibling);
                        } else {
                            updatedControl.parent.appendChild(updatedControl.control);
                        }
                        updatedControl.control.outerHTML = resultObject.updatedControls[id];
                        ko.applyBindings(ko.dataFor(updatedControl.parent), updatedControl.control);
                    }
                }

            } else if (resultObject.action === "redirect") {
                // redirect
                document.location.href = resultObject.url;
                return;
            } 
            
            // trigger afterPostback event
            var isHandled = this.events.afterPostback.trigger(new RedwoodAfterPostBackEventArgs(sender, viewModel, viewModelName, validationTargetPath, resultObject));
            if (!isSuccess && !isHandled) {
                throw "Invalid response from server!";
            }
        }, xhr => {
                if (!this.events.error.trigger(new RedwoodErrorEventArgs(viewModel, xhr))) {
                    alert(xhr.responseText);
                }
            });
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

    public getPath(sender: HTMLElement): string[] {
        var context = ko.contextFor(sender);
        var arr = new Array<string>(context.$parents.length);
        while(context.$parent) {
            if (!context.$data.$parentProp) throw "invalid viewModel for path creating";
            if (context.$index && typeof context.$index === "function")
                arr.push("[" + context.$index() + "]");
            arr.push(ko.utils.unwrapObservable<string>(context.$data.$parentProp));
            context = context.$parentContext;
        }
        return arr.reverse();
    }
    public spitPath(path: string): string[] {
        var indexPos = path.indexOf('[');
        var dotPos = path.indexOf('.');
        var res: string[] = [];
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
    }

    /**
    * Combines two simple javascript paths
    * Supports properties and indexers, includes functions calls
    */
    public combinePaths(a: string[], b: string[]): string[] {
        return this.simplifyPath(a.concat(b));
    }

    /**
    * removes `$parent` and `$root` where possible
    */
    public simplifyPath(path: string[]): string[] {
        var ri = path.lastIndexOf("$root");
        if (ri > 0) path = path.slice(ri);
        path = path.filter(v => v != "$data");
        var parIndex = 0;
        while ((parIndex = path.indexOf("$parent")) >= 0) {
            path.splice(parIndex - 1, 2);
        }
        return path;
    }

    private postJSON(url: string, method: string, postData: any, success: (request: XMLHttpRequest) => void, error: (response: XMLHttpRequest) => void) {
        var xhr = XMLHttpRequest ? new XMLHttpRequest() : <XMLHttpRequest>new ActiveXObject("Microsoft.XMLHTTP");
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

    public evaluateOnViewModel(context, expression: string) {
        return eval("(function (c) { return c." + expression + "; })")(context);
    }
}

// RedwoodEvent is used because CustomEvent is not browser compatible and does not support 
// calling missed events for handler that subscribed too late.
class RedwoodEvent<T extends RedwoodEventArgs> {
    private handlers = [];
    private history = [];

    constructor(public name: string, private triggerMissedEventsOnSubscribe: boolean = false) {
    }

    public subscribe(handler: (data: T) => boolean) {
        this.handlers.push(handler);

        if (this.triggerMissedEventsOnSubscribe) {
            for (var i = 0; i < this.history.length; i++) {
                if (handler(history[i])) {
                    this.history = this.history.splice(i, 1);
                }
            }
        }
    }

    public unsubscribe(handler: (data: T) => boolean) {
        var index = this.handlers.indexOf(handler);
        if (index >= 0) {
            this.handlers = this.handlers.splice(index, 1);
        }
    }

    public trigger(data: T): boolean {
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
    }
}

class RedwoodEventArgs {
    constructor(public viewModel: any) {
    }
}
class RedwoodErrorEventArgs extends RedwoodEventArgs {
    constructor(public viewModel: any, public xhr: XMLHttpRequest) {
        super(viewModel);
    }
}
class RedwoodBeforePostBackEventArgs extends RedwoodEventArgs {
    public cancel: boolean = false;
    constructor(public sender: HTMLElement, public viewModel: any, public viewModelName: string, public validationTargetPath: string, public viewModelPath: string[], public command: string) {
        super(viewModel);
    }
}
class RedwoodAfterPostBackEventArgs extends RedwoodEventArgs {
    constructor(public sender: HTMLElement, public viewModel: any, public viewModelName: string, public validationTargetPath: string, public serverResponseObject: any) {
        super(viewModel);
    }
}

var redwood = new Redwood();
