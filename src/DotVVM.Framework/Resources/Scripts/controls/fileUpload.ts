import { dotvvm } from '../dotvvm-root';
import { wrapObservable } from '../utils/knockout';
import { deserialize } from '../serialization/deserialize';

export function showUploadDialog(sender: HTMLElement) {
    // trigger the file upload dialog
    var iframe = getIframe(sender);
    createUploadId(sender, iframe);
    openUploadDialog(iframe);
}

export function createUploadId(sender: HTMLElement, iframe: HTMLElement): void {
    iframe = iframe || getIframe(sender);
    var uploadId = "DotVVM_upl" + new Date().getTime().toString();
    sender.parentElement!.parentElement!.setAttribute("data-dotvvm-upload-id", uploadId);

    iframe.setAttribute("data-dotvvm-upload-id", uploadId);
}

export function reportProgress(targetControlId: any, isBusy: boolean, progress: number, result: DotvvmFileUploadData[] | string): void {
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
            viewModel.Files.push(wrapObservable(deserialize(result[i])));
        }

        // call the handler
        if ((targetControl.attributes["data-dotvvm-upload-completed"] || { value: null }).value) {
            new Function(targetControl.attributes["data-dotvvm-upload-completed"].value).call(targetControl);
        }
    }
    viewModel.Progress(progress);
    viewModel.IsBusy(isBusy);
}

function getIframe(sender: HTMLElement): HTMLIFrameElement {
    return <HTMLIFrameElement>sender.parentElement!.previousSibling;
}

function openUploadDialog(iframe: HTMLIFrameElement): void {
    var window = iframe.contentWindow;
    if (window) {
        var fileUpload = <HTMLInputElement>window.document.getElementById('upload');
        fileUpload.click();
    }
}

type DotvvmFileUploadCollection = {
    Files: KnockoutObservableArray<KnockoutObservable<DotvvmFileUploadData>>;
    Progress: KnockoutObservable<number>;
    Error: KnockoutObservable<string>;
    IsBusy: KnockoutObservable<boolean>;
}
type DotvvmFileUploadData = {
    FileId: KnockoutObservable<string>;
    FileName: KnockoutObservable<string>;
    FileSize: KnockoutObservable<DotvvmFileSize>;
    IsFileTypeAllowed: KnockoutObservable<boolean>;
    IsMaxSizeExceeded: KnockoutObservable<boolean>;
    IsAllowed: KnockoutObservable<boolean>;
}
type DotvvmFileSize = {
    Bytes : KnockoutObservable<number>;
    FormattedText : KnockoutObservable<string>;
}

dotvvm.fileUpload = export;
