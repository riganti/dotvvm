import { parseDate as parseDotvvmDate, serializeDate } from '../serialization/date'
import * as globalize from '../DotVVM.Globalize'
import { DotvvmValidationElementMetadata, DotvvmValidationObservableMetadata } from '../validation/common';

// handler dotvvm-textbox-text
export default {
    "dotvvm-textbox-text": {
        init(element: HTMLInputElement, valueAccessor: () => any, allBindingsAccessor?: KnockoutAllBindingsAccessor) {
            const obs = valueAccessor();
            const valueUpdate = allBindingsAccessor!.get("valueUpdate");

            // generate metadata func
            const elmMetadata: DotvvmValidationElementMetadata = {
                element,
                dataType: element.getAttribute("data-dotvvm-value-type") || "",
                format: element.getAttribute("data-dotvvm-format") || "",
                domNodeDisposal: false,
                elementValidationState: true
            }

            // add metadata for validation
            let metadata = [] as DotvvmValidationObservableMetadata
            if (ko.isObservable(obs)) {
                if (!obs.dotvvmMetadata) {
                    obs.dotvvmMetadata = [elmMetadata];
                } else {
                    obs.dotvvmMetadata.push(elmMetadata);
                }
                metadata = obs.dotvvmMetadata;
            }
            setTimeout(() => {
                // remove element from collection when its removed from dom
                ko.utils.domNodeDisposal.addDisposeCallback(element, () => {
                    for (const meta of metadata) {
                        if (meta.element === element) {
                            metadata.splice(metadata.indexOf(meta), 1);
                            break;
                        }
                    }
                });
            }, 0);

            element.addEventListener("change", () => {
                if (!ko.isObservable(obs)) {
                    return;
                }

                // parse the value
                let result;
                let isEmpty;
                let newValue;
                if (elmMetadata.dataType === "datetime") {
                    // parse date
                    let currentValue = obs();
                    if (currentValue != null) {
                        currentValue = parseDotvvmDate(currentValue);
                    }
                    result = globalize.parseDate(element.value, elmMetadata.format, currentValue);
                    isEmpty = result == null;
                    newValue = isEmpty ? null : serializeDate(result, false);
                } else {
                    // parse number
                    result = globalize.parseNumber(element.value);
                    isEmpty = result === null || isNaN(result);
                    newValue = isEmpty ? null : result;
                }

                // update element validation metadata
                if (newValue == null && element.value !== null && element.value !== "") {
                    element.setAttribute("data-invalid-value", element.value);
                    element.setAttribute("data-dotvvm-value-type-valid", "false"); // TODO: is this actually needed?
                    elmMetadata.elementValidationState = false;
                } else {
                    element.removeAttribute("data-invalid-value");
                    element.setAttribute("data-dotvvm-value-type-valid", "true");
                    elmMetadata.elementValidationState = true;
                }

                if (obs() === newValue) {
                    if (obs.valueHasMutated) {
                        obs.valueHasMutated();
                    } else {
                        obs.notifySubscribers();
                    }
                } else {
                    try {
                        obs(newValue);
                    } catch { 
                        // observable may throw an exception if there is a validation error
                        // but subscribers will be notified anyway so it's not a problem
                    }
                }
            });
        },
        update(element: HTMLInputElement, valueAccessor: () => any) {
            const obs = valueAccessor();
            const format = element.getAttribute("data-dotvvm-format");
            const value = ko.unwrap(obs);

            if (format) {
                const formatted = globalize.formatString(format, value);
                const invalidValue = element.getAttribute("data-invalid-value");

                if (invalidValue == null) {
                    element.value = formatted || "";

                    if (obs.dotvvmMetadata) {
                        const elementMetadata: DotvvmValidationElementMetadata[] = obs.dotvvmMetadata;

                        for (const elemMetadata of elementMetadata) {
                            if (elemMetadata.element == element) {
                                element.setAttribute("data-dotvvm-value-type-valid", "true");
                                elemMetadata.elementValidationState = true;
                            }
                        }
                    }
                } else {
                    element.removeAttribute("data-invalid-value");
                    element.value = invalidValue;
                }
            } else {
                element.value = value;
            }
        }
    }
}
