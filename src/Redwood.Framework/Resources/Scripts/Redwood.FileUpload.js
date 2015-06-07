/// <reference path="redwood.ts" />
if (!redwood) {
    throw "Redwood.js is not loaded!";
}
var RedwoodFileUpload = (function () {
    function RedwoodFileUpload() {
    }
    RedwoodFileUpload.prototype.showUploadDialog = function (iframeId) {
        var iframe = document.getElementById(iframeId);
        // trigger the file upload dialog
        var fileUpload = iframe.contentWindow.document.getElementById('upload');
        fileUpload.click();
    };
    RedwoodFileUpload.prototype.reportProgress = function (targetControlId, isBusy, progress, result) {
        // find target control viewmodel
        var targetControl = document.getElementById(targetControlId);
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
                viewModel.Files.push(ko.mapper.fromJS(result[i]));
            }
        }
        viewModel.Progress(progress);
        viewModel.IsBusy(isBusy);
    };
    return RedwoodFileUpload;
})();
var RedwoodFileUploadCollection = (function () {
    function RedwoodFileUploadCollection() {
        this.Files = ko.observableArray();
        this.Progress = ko.observable(0);
        this.Error = ko.observable();
        this.IsBusy = ko.observable();
    }
    return RedwoodFileUploadCollection;
})();
var RedwoodFileUploadData = (function () {
    function RedwoodFileUploadData() {
        this.FileId = ko.observable();
        this.FileName = ko.observable();
    }
    return RedwoodFileUploadData;
})();
redwood.fileUpload = redwood.fileUpload || new RedwoodFileUpload();
//# sourceMappingURL=Redwood.FileUpload.js.map