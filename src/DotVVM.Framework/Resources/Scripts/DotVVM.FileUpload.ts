/// <reference path="dotvvm.ts" />
class DotvvmFileUpload {
    public showUploadDialog(sender: HTMLElement) {
        // trigger the file upload dialog
        var iframe = this.getIframe(sender);
        this.createUploadId(sender, iframe);
        this.openUploadDialog(iframe);
    }
    private getIframe(sender:HTMLElement) {
        return <HTMLIFrameElement>sender.parentElement.previousSibling;
    }
    private openUploadDialog(iframe: HTMLIFrameElement) {
        var fileUpload = <HTMLInputElement>iframe.contentWindow.document.getElementById('upload');
        fileUpload.click();
    }

    public createUploadId(sender: HTMLElement, iframe: HTMLElement) {
        iframe = iframe || this.getIframe(sender);
        var uploadId = "DotVVM_upl" + new Date().getTime().toString();
        sender.parentElement.parentElement.setAttribute("data-dotvvm-upload-id", uploadId);

        iframe.setAttribute("data-dotvvm-upload-id", uploadId);
    }

    public reportProgress(targetControlId: any, isBusy: boolean, progress: number, result: DotvvmFileUploadData[] | string) {
        // find target control viewmodel
        var targetControl = <HTMLDivElement>document.querySelector("div[data-dotvvm-upload-id='" + targetControlId.value + "']");
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
            if ((targetControl.attributes["data-dotvvm-upload-completed"] || { value: null }).value) {
                new Function(targetControl.attributes["data-dotvvm-upload-completed"].value).call(targetControl);
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
    public FileTypeAllowed = ko.observable<boolean>();
    public MaxSizeExceeded = ko.observable<boolean>();
    public Allowed = ko.observable<boolean>();
}