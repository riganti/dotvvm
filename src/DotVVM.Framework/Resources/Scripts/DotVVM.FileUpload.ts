/// <reference path="dotvvm.ts" />
class DotvvmFileUpload {

    public showUploadDialog(sender: HTMLElement) {
        var uploadId = "DotVVM_upl" + new Date().getTime().toString();
        sender.parentElement.parentElement.dataset["dotvvmUploadId"] = uploadId;

        var iframe = <HTMLIFrameElement>sender.parentElement.previousSibling;
        iframe.dataset["dotvvmUploadId"] = uploadId;
        
        // trigger the file upload dialog
        var fileUpload = <HTMLInputElement>iframe.contentWindow.document.getElementById('upload');
        fileUpload.click();
    }

    public reportProgress(targetControlId: string, isBusy: boolean, progress: number, result: DotvvmFileUploadData[] | string) {
        // find target control viewmodel
        var targetControl = <HTMLDivElement>document.querySelector("div[data-dotvvm-upload-id='" + targetControlId + "']");
        var viewModel = <DotvvmFileUploadCollection>ko.dataFor(targetControl.firstChild);

        // determine the status
        if (typeof result === "string") {
            // error during upload
            viewModel.Error(result);
        } else {
            // files were uploaded successfully
            viewModel.Error("");
            for (var i = 0; i < result.length; i++) {
                viewModel.Files.push(dotvvm.serialization.wrapObservable(dotvvm.serialization.deserialize(result[i])));
            }

            // call the handler
            if (targetControl.dataset["uploadCompleted"]) {
                new Function(targetControl.dataset["uploadCompleted"]).call(targetControl);
            }
        }
        viewModel.Progress(progress);
        viewModel.IsBusy(isBusy);
    }
}

class DotvvmFileUploadCollection {
    public Files = ko.observableArray<KnockoutObservable<DotvvmFileUpload>>();
    public Progress = ko.observable<number>(0);
    public Error = ko.observable<string>();
    public IsBusy = ko.observable<boolean>();
}
class DotvvmFileUploadData {
    public FileId = ko.observable<string>();
    public FileName = ko.observable<string>();
}
