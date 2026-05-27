import { uploadFiles } from "../controls/fileUpload";

export default {
    "dotvvm-FileUpload-UploadOnPasteOrDrop": {
        init: function (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) {
            const options = ko.unwrap(valueAccessor());
            const uploadUrl = ko.unwrap(options.url);
            const collectionObservable = options.collection;

            function callUploadFiles(files: FileList) {
                uploadFiles(collectionObservable, options.multiple, uploadUrl, files, () => {
                    if (element.hasAttribute("data-dotvvm-upload-completed")) {
                        new Function(element.getAttribute("data-dotvvm-upload-completed")!).call(element);
                    }
                });
            }

            // Handle paste events
            element.addEventListener("paste", function(e: ClipboardEvent) {
                e.preventDefault();
                
                if (e.clipboardData && e.clipboardData.files && e.clipboardData.files.length > 0) {
                    callUploadFiles(e.clipboardData.files);
                }
            });

            // Handle drop events
            element.addEventListener("dragover", function(e: DragEvent) {
                e.preventDefault();
                e.stopPropagation();
                element.classList.add("dotvvm-upload-dragover");
            });

            element.addEventListener("dragleave", function(e: DragEvent) {
                e.preventDefault();
                e.stopPropagation();
                element.classList.remove("dotvvm-upload-dragover");
            });

            element.addEventListener("drop", function(e: DragEvent) {
                e.preventDefault();
                e.stopPropagation();
                element.classList.remove("dotvvm-upload-dragover");
                
                if (e.dataTransfer && e.dataTransfer.files && e.dataTransfer.files.length > 0) {
                    callUploadFiles(e.dataTransfer.files);
                }
            });
        }
    }
}
