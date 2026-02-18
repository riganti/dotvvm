import { uploadFiles } from '../controls/fileUpload';

export default {
    "dotvvm-FileUpload": {
        init: function (element: HTMLInputElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) {
            var args = ko.unwrap(valueAccessor());

            element.addEventListener("change", function() {
                if (!element.files || !element.files.length) return;

                uploadFiles(ko.contextFor(element).$rawData, element.multiple, args.url, element.files, () => {
                    if (element.parentElement!.hasAttribute("data-dotvvm-upload-completed")) {
                        new Function(element.parentElement!.getAttribute("data-dotvvm-upload-completed")!).call(element);
                    }
                    element.value = "";
                });
            });
        }
    }
}
