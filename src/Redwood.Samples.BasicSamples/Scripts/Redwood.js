var Redwood = (function () {
    function Redwood() {
        this.viewModels = {};
    }
    Redwood.prototype.init = function (viewModelName) {
        var viewModel = ko.mapping.fromJS(this.viewModels[viewModelName]);
        this.viewModels[viewModelName] = viewModel;
        ko.applyBindings(viewModel);
    };

    Redwood.prototype.postBack = function (viewModelName, sender, path, command, controlUniqueId) {
        var _this = this;
        var viewModel = this.viewModels[viewModelName];
        this.updateDynamicPathFragments(sender, path);
        var data = {
            viewModel: ko.mapping.toJS(viewModel),
            currentPath: path,
            command: command,
            controlUniqueId: controlUniqueId
        };
        this.postJSON(document.location.href, "POST", ko.toJSON(data), function (result) {
            ko.mapping.fromJSON(result.responseText, {}, _this.viewModels[viewModelName]);
        }, function (error) {
            alert(error.responseText);
        });
    };

    Redwood.prototype.updateDynamicPathFragments = function (sender, path) {
        var context = ko.contextFor(sender);

        for (var i = path.length - 1; i >= 0; i--) {
            if (path[i] == "[$index]") {
                path[i] = "[" + context.$index() + "]";
            }
            context = context.$parent;
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
            } else {
                error(xhr);
            }
        };
        xhr.send(postData);
    };
    return Redwood;
})();

var redwood = new Redwood();
//# sourceMappingURL=Redwood.js.map
