/// <reference path="dotvvm.ts" />
if (!dotvvm) {
    throw "DotVVM.js is not loaded!";
}
var DotvvmFileUpload = (function () {
    function DotvvmFileUpload() {
    }
    DotvvmFileUpload.prototype.showUploadDialog = function (iframeId) {
        var iframe = document.getElementById(iframeId);
        // trigger the file upload dialog
        var fileUpload = iframe.contentWindow.document.getElementById('upload');
        fileUpload.click();
    };
    DotvvmFileUpload.prototype.reportProgress = function (targetControlId, isBusy, progress, result) {
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
dotvvm.fileUpload = dotvvm.fileUpload || new DotvvmFileUpload();
//# sourceMappingURL=DotVVM.FileUpload.js.map