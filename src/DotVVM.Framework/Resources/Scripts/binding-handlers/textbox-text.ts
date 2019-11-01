import { parseDate as parseDotvvmDate, serializeDate } from '../serialization/date'
import * as globalize from '../DotVVM.Globalize'

export type DotvvmValidationObservableMetadata = DotvvmValidationElementMetadata[];
export interface DotvvmValidationElementMetadata {
    element: HTMLElement;
    dataType: string;
    format: string;
    domNodeDisposal: boolean;
    elementValidationState: boolean;
}

// handler dotvvm-textbox-text
export default {
    "dotvvm-textbox-text": {
        init(element: HTMLInputElement, valueAccessor: () => any, allBindingsAccessor?: KnockoutAllBindingsAccessor) {
            var obs = valueAccessor(),
                valueUpdate = allBindingsAccessor!.get("valueUpdate");

            //generate metadata func
            var elmMetadata : DotvvmValidationElementMetadata = {
                element,
                dataType: element.getAttribute("data-dotvvm-value-type") || "",
                format: element.getAttribute("data-dotvvm-format") || "",
                domNodeDisposal: false,
                elementValidationState: true
            }

            //add metadata for validation
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
                if (!ko.isObservable(obs)) return;
                // parse the value
                var result, isEmpty, newValue;
                if (elmMetadata.dataType === "datetime") {
                    // parse date
                    var currentValue = obs();
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
                    obs(newValue);
                }
            });
        },
        update(element: HTMLInputElement, valueAccessor: () => any) {
            var obs = valueAccessor(),
                format = element.getAttribute("data-dotvvm-format"),
                value = ko.unwrap(obs);

            if (format) {
                const formatted = globalize.formatString(format, value),
                    invalidValue = element.getAttribute("data-invalid-value");

                if (invalidValue == null) {
                    element.value = formatted || "";

                    if (obs.dotvvmMetadata) {
                        var elemsMetadata: DotvvmValidationElementMetadata[] = obs.dotvvmMetadata;

                        for (const elemMetadata of elemsMetadata) {
                            if (elemMetadata.element == element) {
                                element.setAttribute("data-dotvvm-value-type-valid", "true");
                                elemMetadata.elementValidationState = true;
                            }
                        }
                    }
                }
                else {
                    element.removeAttribute("data-invalid-value");
                    element.value = invalidValue;
                }
            } else {
                element.value = value;
            }
        }
    }
}
