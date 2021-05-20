import { wrapObservable } from '../utils/knockout';
import { deserialize } from '../serialization/deserialize';
import { updateTypeInfo } from '../metadata/typeMap';

export function showUploadDialog(sender: HTMLElement) {
    // trigger the file upload dialog
    let fileUpload = <HTMLInputElement>sender.parentElement!.parentElement!.querySelector("input[type=file]");
    fileUpload!.click();
}

export function reportProgress(inputControl: HTMLInputElement, isBusy: boolean, progress: number, result: DotvvmStaticCommandResponse<DotvvmFileUploadData[]> | string): void {
    // find target control viewmodel
    const targetControl = <HTMLDivElement> inputControl.parentElement!;
    const viewModel = <DotvvmFileUploadCollection> ko.dataFor(targetControl.firstChild);

    // determine the status
    if (typeof result === "string") {
        // error during upload
        viewModel.Error(result);
    } else if ("result" in result) {
        // files were uploaded successfully
        viewModel.Error("");
        updateTypeInfo(result.typeMetadata);
        for (let i = 0; i < result.result.length; i++) {
            viewModel.Files.push(wrapObservable(deserialize(result.result[i])));
        }

        // call the handler
        if (((<any> targetControl.attributes)["data-dotvvm-upload-completed"] || { value: null }).value) {
            new Function((<any> targetControl.attributes)["data-dotvvm-upload-completed"].value).call(targetControl);
        }
    }
    viewModel.Progress(progress);
    viewModel.IsBusy(isBusy);
}

