import { wrapObservable } from '../utils/knockout';
import { updateTypeInfo } from '../metadata/typeMap';

export function showUploadDialog(sender: HTMLElement) {
    // trigger the file upload dialog
    let fileUpload = <HTMLInputElement>sender.parentElement!.parentElement!.querySelector("input[type=file]");
    fileUpload!.click();
}

export function uploadFiles(viewModel: DotvvmObservable<DotvvmFileUploadCollection>, allowMultiple: boolean, url: string, files: FileList, onCompleted: () => void) {
    var xhr = XMLHttpRequest ? new XMLHttpRequest() : new ((window as any)["ActiveXObject"])("Microsoft.XMLHTTP");
    xhr.open("POST", url, true);
    xhr.setRequestHeader("X-DotVVM-AsyncUpload", "true");
    xhr.upload.onprogress = function (e: ProgressEvent) {
        if (e.lengthComputable) {
            (viewModel as any).patchState({ Error: null, IsBusy: true, Progress: Math.round(e.loaded * 100 / e.total) });
        }
    };
    xhr.onerror = function () {
        (viewModel as any).patchState({ Error: "Upload failed.", IsBusy: false, Progress: 0 });
    };
    xhr.onload = function () {
        if (xhr.status == 200) {
            (viewModel as any).patchState({ Error: null, IsBusy: true, Progress: 100 });

            const result = JSON.parse(xhr.responseText) as DotvvmStaticCommandResponse<DotvvmFileUploadData[]>;
            if ("typeMetadata" in result) {
                updateTypeInfo(result.typeMetadata);
            }
            if (!("result" in result)) {
                throw new Error("FileUpload result is empty!");
            }

            // if multiple files are allowed, we append to the collection
            // if it's not, we replace the collection with the one new file
            const newFiles = allowMultiple ? [...viewModel.state.Files as any, ...result.result] : result.result;
            (viewModel as any).patchState({ Files: newFiles });

            // call the handler
            onCompleted();

            (viewModel as any).patchState({ IsBusy: false });

        } else {
            (viewModel as any).patchState({ Error: "Upload failed.", IsBusy: false, Progress: 0 });
        }
    };

    var formData = new FormData();
    if (files.length > 1) {
        for (var i = 0; i < files.length; i++) {
            formData.append("upload[]", files[i]);
        }
    } else if (files.length > 0) {
        formData.append("upload", files[0]);
    }
    xhr.send(formData);
}
