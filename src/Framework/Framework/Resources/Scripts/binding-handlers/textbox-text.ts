import { parseDate, parseDateOnly, parseTimeOnly, serializeDate, serializeDateOnly, serializeTimeOnly } from '../serialization/date'
import * as globalize from '../DotVVM.Globalize'
import { DotvvmValidationElementMetadata, DotvvmValidationObservableMetadata, getValidationMetadata } from '../validation/common';
import { lastSetErrorSymbol } from '../state-manager';

// handler dotvvm-textbox-text
export default {
    "dotvvm-textbox-text": {
        init(element: HTMLInputElement, valueAccessor: () => any, allBindingsAccessor?: KnockoutAllBindingsAccessor) {
            const obs = valueAccessor();
            const valueUpdate = allBindingsAccessor?.get("valueUpdate") || "change";

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
                if (!(obs as any).dotvvmMetadata) {
                    (obs as any).dotvvmMetadata = [elmMetadata];
                } else {
                    (obs as any).dotvvmMetadata.push(elmMetadata);
                }
                metadata = (obs as any).dotvvmMetadata;
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

            const valueUpdateHandler = () => {
                const obs = valueAccessor();
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
                        currentValue = parseDate(currentValue);
                    }
                    result = globalize.parseDate(element.value, elmMetadata.format, currentValue) || globalize.parseDate(element.value, "", currentValue);
                    isEmpty = result == null;
                    newValue = isEmpty ? null : serializeDate(result, false);
                } else if (elmMetadata.dataType === "number") {
                    // parse number
                    result = globalize.parseNumber(element.value);
                    isEmpty = result === null || isNaN(result);
                    newValue = isEmpty ? null : result;
                } else if (elmMetadata.dataType === "dateonly") {
                    // parse dateonly
                    let currentValue = obs();
                    if (currentValue != null) {
                        currentValue = parseDateOnly(currentValue);
                    }
                    result = globalize.parseDate(element.value, elmMetadata.format, currentValue) || globalize.parseDate(element.value, "", currentValue);
                    isEmpty = result == null;
                    newValue = isEmpty ? null : serializeDateOnly(result);
                } else if (elmMetadata.dataType === "timeonly") {
                    // parse timeonly
                    let currentValue = obs();
                    if (currentValue != null) {
                        currentValue = parseTimeOnly(currentValue);
                    }
                    result = globalize.parseDate(element.value, elmMetadata.format, currentValue) || globalize.parseDate(element.value, "", currentValue);
                    isEmpty = result == null;
                    newValue = isEmpty ? null : serializeTimeOnly(result);
                } else {
                    // string
                    newValue = element.value;
                }

                // update element validation metadata (this is used when FormatString is set)
                if (newValue == null && element.value !== null && element.value !== "") {
                    element.setAttribute("data-invalid-value", element.value);
                    element.setAttribute("data-dotvvm-value-type-valid", "false");
                    elmMetadata.elementValidationState = false;
                } else {
                    element.removeAttribute("data-invalid-value");
                    element.setAttribute("data-dotvvm-value-type-valid", "true");
                    elmMetadata.elementValidationState = true;
                }

                const originalElementValue = element.value;
                try {
                    if (obs.peek() === newValue) {
                        // first null can be legit (allowed empty value), second can be a validation error (invalid format etc.)
                        // we have to trigger the change anyway
                        obs.valueHasMutated ? obs.valueHasMutated() : obs.notifySubscribers();
                        if (elmMetadata.elementValidationState) {
                            (obs as any)[lastSetErrorSymbol] = void 0;
                        }
                    } else {
                        if (element.validity.valid || element.value !== '') {
                            obs(newValue);
                        }
                    }
                } catch (err) {
                    // observable may throw an exception if there is a validation error
                    // but subscribers will be notified anyway so it's not a problem
                    elmMetadata.elementValidationState = false;
                    element.setAttribute("data-invalid-value", element.value);
                    element.setAttribute("data-dotvvm-value-type-valid", "false");

                    // update has already been called - we need to restore the original value in the element
                    element.value = originalElementValue;
                }
            };

            element.addEventListener(valueUpdate, valueUpdateHandler);
        },
        update(element: HTMLInputElement, valueAccessor: () => any) {
            const obs = valueAccessor();

            // get value
            let value = ko.unwrap(obs);

            // apply formatting
            const format = element.getAttribute("data-dotvvm-format");
            if (format) {
                value = globalize.formatString(format, value, element.getAttribute("data-dotvvm-value-type"));
            }

            const invalidValue = element.getAttribute("data-invalid-value");
            if (invalidValue != null) {
                // if there is an invalid value from previous change, use it and reset the flag
                element.removeAttribute("data-invalid-value");
                value = invalidValue;
            } else {
                // value has changed, reset validation state
                const elementMetadata = obs && getValidationMetadata(obs);
                if (elementMetadata) {
                    for (const elemMetadata of elementMetadata) {
                        if (elemMetadata.element == element) {
                            elemMetadata.elementValidationState = true;
                            element.setAttribute("data-dotvvm-value-type-valid", "true");
                            element.removeAttribute("data-invalid-value");
                        }
                    }
                }
            }

            element.value = value == null ? "" : value;
        }
    }
}
