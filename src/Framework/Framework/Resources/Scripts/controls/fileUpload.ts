import { wrapObservable } from '../utils/knockout';
import { updateTypeInfo } from '../metadata/typeMap';
import { getCsrfToken } from "../postback/http";

export function showUploadDialog(sender: HTMLElement) {
    // trigger the file upload dialog
    let fileUpload = <HTMLInputElement>sender.parentElement!.parentElement!.querySelector("input[type=file]");
    fileUpload!.click();
}

export async function uploadFiles(viewModel: DotvvmObservable<DotvvmFileUploadCollection>, uploadToken: string, allowMultiple: boolean, url: string, files: FileList, onCompleted: () => void) {
    try {
        var xhr = XMLHttpRequest ? new XMLHttpRequest() : new ((window as any)["ActiveXObject"])("Microsoft.XMLHTTP");
        xhr.open("POST", url, true);
        xhr.setRequestHeader("X-DotVVM-AsyncUpload", "true");
        xhr.setRequestHeader("X-DotVVM-UploadToken", uploadToken);
        xhr.setRequestHeader("X-DotVVM-CsrfToken", await getCsrfToken(undefined));
        xhr.upload.onprogress = function (e: ProgressEvent) {
            if (e.lengthComputable) {
                reportProgress(true, Math.round(e.loaded * 100 / e.total));
            }
        };
        xhr.onerror = function () {
            reportProgress(false, 0, "Upload failed.");
        };
        xhr.onload = function () {
            if (xhr.status == 200) {
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
                reportProgress(true, 100, newFiles);

                // call the handler
                onCompleted();

                reportProgress(false, 100);
            }
            else if (xhr.status == 413) {
                reportProgress(false, 0, "Uploaded file is too large.")
            } else {
                reportProgress(false, 100, "Upload failed.");
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
    } catch (e) {
        console.log("File upload error", e);
        reportProgress(false, 0, "Upload failed.");
    }

    function reportProgress(isBusy: boolean, progress: number, filesOrError?: DotvvmFileUploadData[] | string): void {
        (viewModel as any).patchState({
            IsBusy: isBusy,
            Progress: progress,
            Error: typeof (filesOrError) === "string" ? filesOrError : null,
            Files: typeof (filesOrError) === "object" ? filesOrError : viewModel.state.Files
        });
    }
}
