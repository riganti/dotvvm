class Redwood {

    public viewModels: any = {};
    public culture: string;
    public events = {
        init: new RedwoodEvent<RedwoodEventArgs>("redwood.events.init", true),
        beforePostback: new RedwoodEvent<RedwoodEventArgs>("redwood.events.beforePostback"),
        afterPostback: new RedwoodEvent<RedwoodEventArgs>("redwood.events.afterPostback"),
        error: new RedwoodEvent<RedwoodErrorEventArgs>("redwood.events.error")
    };

    public init(viewModelName: string, culture: string): void {
        this.culture = culture;
        var viewModel = ko.mapper.fromJS(this.viewModels[viewModelName].viewModel);
        this.viewModels[viewModelName] = viewModel;
        ko.applyBindings(viewModel);

        this.events.init.trigger(new RedwoodEventArgs(viewModel));
    }
    
    public postBack(viewModelName: string, sender: HTMLElement, path: string[], command: string, controlUniqueId: string): void {
        var viewModel = this.viewModels[viewModelName];
        this.events.beforePostback.trigger(new RedwoodEventArgs(viewModel));
        this.updateDynamicPathFragments(sender, path);
        var data = {
            viewModel: ko.mapper.toJS(viewModel),
            currentPath: path,
            command: command,
            controlUniqueId: controlUniqueId
        };
        this.postJSON(document.location.href, "POST", ko.toJSON(data), result => {
            var resultObject = JSON.parse(result.responseText);
            if (resultObject.action === "successfulCommand") {
                ko.mapper.fromJS(resultObject.viewModel, {}, this.viewModels[viewModelName]);
                this.events.afterPostback.trigger(new RedwoodEventArgs(viewModel));
            } else if (resultObject.action === "redirect") {
                document.location.href = resultObject.url;
            } else {
                throw "Invalid response from the server!";
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
            if (path[i].indexOf("[$index]")) {
                path[i] = path[i].replace("[$index]", "[" + context.$index() + "]");
            }
            context = context.$parentContext;
        }
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

var redwood = new Redwood();
