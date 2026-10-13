import { updateTypeInfo } from '../metadata/typeMap';
import { getCsrfToken } from "../postback/http";

export function showUploadDialog(sender: HTMLElement) {
    // trigger the file upload dialog
    let fileUpload = <HTMLInputElement>sender.parentElement!.parentElement!.querySelector("input[type=file]");
    fileUpload!.click();
}

export async function uploadFiles(viewModel: DotvvmObservable<DotvvmFileUploadCollection>, uploadToken: string, allowMultiple: boolean, url: string, files: FileList, onCompleted: () => void, chunkSize = 0) {
    if (!files.length || viewModel.state.IsBusy) return;
    reportProgress(true, 0);
    try {
        if (!Number.isSafeInteger(chunkSize) || chunkSize < 0) {
            throw new Error("Invalid upload chunk size.");
        }
        const csrfToken = await getCsrfToken(undefined);
        const totalSize = Array.from(files).reduce((sum, file) => sum + file.size, 0);
        const uploadedFiles: DotvvmFileUploadData[] = [];
        let completedSize = 0;
        if (chunkSize > 0) {
            for (const file of Array.from(files)) {
                let token = uploadToken;
                let position = 0;
                do {
                    const end = Math.min(position + chunkSize, file.size);
                    const final = end === file.size;
                    const formData = new FormData();
                    formData.append("upload", file.slice(position, end, file.type), file.name);
                    const response = await send(formData, token, end - position, final, position === 0 ? file.size : undefined);
                    if (final) {
                        if (response.files.length !== 1) throw new Error("FileUpload result is empty!");
                        uploadedFiles.push(...response.files);
                    } else {
                        if (!response.token || response.files.length) throw new Error("Invalid upload continuation.");
                        token = response.token;
                    }
                    completedSize += end - position;
                    position = end;
                    if (final) break;
                } while (position < file.size);
            }
        } else {
            const formData = new FormData();
            for (const file of Array.from(files)) {
                formData.append(files.length > 1 ? "upload[]" : "upload", file);
            }
            uploadedFiles.push(...(await send(formData, uploadToken, totalSize)).files);
        }

        // if multiple files are allowed, we append to the collection
        // if it's not, we replace the collection with the one new file
        const newFiles = allowMultiple ? [...viewModel.state.Files as any, ...uploadedFiles] : uploadedFiles;
        reportProgress(true, 100, newFiles);
        onCompleted();
        reportProgress(false, 100);

        function send(formData: FormData, token: string, size: number, final?: boolean, fileSize?: number): Promise<{ files: DotvvmFileUploadData[], token: string | null }> {
            return new Promise((resolve, reject) => {
                const xhr = new XMLHttpRequest();
                xhr.open("POST", url, true);
                xhr.setRequestHeader("X-DotVVM-AsyncUpload", "true");
                xhr.setRequestHeader("X-DotVVM-UploadToken", token);
                xhr.setRequestHeader("X-DotVVM-CsrfToken", csrfToken);
                if (final !== undefined) {
                    xhr.setRequestHeader("X-DotVVM-UploadChunk", "true");
                    xhr.setRequestHeader("X-DotVVM-UploadFinalChunk", String(final));
                }
                if (fileSize !== undefined) {
                    xhr.setRequestHeader("X-DotVVM-UploadFileSize", String(fileSize));
                }
                xhr.upload.onprogress = (e: ProgressEvent) => {
                    if (e.lengthComputable && totalSize > 0 && e.total > 0) {
                        reportProgress(true, Math.min(99, Math.round((completedSize + size * e.loaded / e.total) * 100 / totalSize)));
                    }
                };
                xhr.onerror = xhr.onabort = () => reject(new Error("Upload failed."));
                xhr.onload = () => {
                    if (xhr.status !== 200) {
                        reject(new Error(xhr.status === 413 ? "Uploaded file is too large." : "Upload failed."));
                        return;
                    }
                    try {
                        const result = JSON.parse(xhr.responseText) as DotvvmStaticCommandResponse<DotvvmFileUploadData[]>;
                        if ("typeMetadata" in result) {
                            updateTypeInfo(result.typeMetadata);
                        }
                        if (!("result" in result) || !Array.isArray(result.result)) {
                            throw new Error("FileUpload result is empty!");
                        }
                        resolve({ files: result.result, token: xhr.getResponseHeader("X-DotVVM-UploadToken") });
                    } catch (e) {
                        reject(e);
                    }
                };
                xhr.send(formData);
            });
        };
    } catch (e) {
        console.log("File upload error", e);
        reportProgress(false, 0, e instanceof Error && e.message === "Uploaded file is too large." ? e.message : "Upload failed.");
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
