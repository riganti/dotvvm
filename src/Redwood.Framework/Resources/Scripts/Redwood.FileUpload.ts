/// <reference path="redwood.ts" />

if (!redwood) {
    throw "Redwood.js is not loaded!";
}

class RedwoodFileUpload {

    public showUploadDialog(iframeId: string) {
        var iframe = <HTMLIFrameElement>document.getElementById(iframeId);
        
        // trigger the file upload dialog
        var fileUpload = <HTMLInputElement>iframe.contentWindow.document.getElementById('upload');
        fileUpload.click();
    }

    public reportProgress(targetControlId: string, isBusy: boolean, progress: number, result: RedwoodFileUploadData[] | string) {
        // find target control viewmodel
        var targetControl = document.getElementById(targetControlId);
        var viewModel = <RedwoodFileUploadCollection>ko.dataFor(targetControl.firstChild);

        // determine the status
        if (typeof result === "string") {
            // error during upload
            viewModel.Error(result);
        } else {
            // files were uploaded successfully
            viewModel.Error("");
            for (var i = 0; i < result.length; i++) {
                viewModel.Files.push(ko.mapper.fromJS(result[i]));
            }
        }
        viewModel.Progress(progress);
        viewModel.IsBusy(isBusy);
    }
}

class RedwoodFileUploadCollection {
    public Files = ko.observableArray<RedwoodFileUpload>();
    public Progress = ko.observable<number>(0);
    public Error = ko.observable<string>();
    public IsBusy = ko.observable<boolean>();
}
class RedwoodFileUploadData {
    public FileId = ko.observable<string>();
    public FileName = ko.observable<string>();
}

(<any>redwood).fileUpload = (<any>redwood).fileUpload || new RedwoodFileUpload();
