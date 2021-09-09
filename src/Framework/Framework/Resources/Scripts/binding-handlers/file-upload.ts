export default {
    "dotvvm-FileUpload": {
        init: function (element: HTMLInputElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) {

            var args = ko.unwrap(valueAccessor());

            function reportProgress(isBusy: boolean, percent: number, resultOrError: string | DotvvmStaticCommandResponse<DotvvmFileUploadData[]>) {
                dotvvm.fileUpload.reportProgress(<HTMLInputElement> element, isBusy, percent, resultOrError);
            }

            element.addEventListener("change", function() {
                if (!element.files || !element.files.length) return;

                var xhr = XMLHttpRequest ? new XMLHttpRequest() : new ((window as any)["ActiveXObject"])("Microsoft.XMLHTTP");
                xhr.open("POST", args.url, true);
                xhr.setRequestHeader("X-DotVVM-AsyncUpload", "true");
                xhr.upload.onprogress = function (e: ProgressEvent) {
                    if (e.lengthComputable) {
                        reportProgress(true, Math.round(e.loaded * 100 / e.total), '');
                    }
                };
                xhr.onload = function () {
                    if (xhr.status == 200) {
                        reportProgress(false, 100, JSON.parse(xhr.responseText));
                        element.value = "";
                    } else {
                        reportProgress(false, 0, "Upload failed.");
                    }
                };

                var formData = new FormData();
                if (element.files.length > 1) {
                    for (var i = 0; i < element.files.length; i++) {
                        formData.append("upload[]", element.files[i]);
                    }
                } else if (element.files.length > 0) {
                    formData.append("upload", element.files[0]);
                }
                xhr.send(formData);
            });

        }
    }
}
