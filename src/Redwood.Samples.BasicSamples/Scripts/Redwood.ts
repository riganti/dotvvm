class Redwood {
    
    public viewModels: any = {};


    public init(viewModelName: string): void {
        var viewModel = ko.mapping.fromJS(this.viewModels[viewModelName]);
        this.viewModels[viewModelName] = viewModel;
        ko.applyBindings(viewModel);
    }

    public postBack(viewModelName: string, sender: HTMLElement, path: string[], command: string, controlUniqueId: string): void {
        var viewModel = this.viewModels[viewModelName];
        this.updateDynamicPathFragments(sender, path);
        var data = {
            viewModel: ko.mapping.toJS(viewModel),
            currentPath: path,
            command: command,
            controlUniqueId: controlUniqueId
        };
        this.postJSON(document.location.href, "POST", ko.toJSON(data), result => {
            ko.mapping.fromJSON(result.responseText, {}, this.viewModels[viewModelName]);
        }, error => {
            alert(error.responseText);
        });
    }

    private updateDynamicPathFragments(sender: HTMLElement, path: string[]): void {
        var context = ko.contextFor(sender);
        
        for (var i = path.length - 1; i >= 0; i--) {
            if (path[i] == "[$index]") {
                path[i] = "[" + context.$index() + "]";
            }
            context = context.$parent;
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

var redwood = new Redwood();
