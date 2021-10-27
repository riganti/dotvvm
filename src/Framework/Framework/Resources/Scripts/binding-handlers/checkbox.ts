import * as events from '../events';

export default {
    'dotvvm-checkbox-updateAfterPostback': {
        init(element: HTMLElement, valueAccessor: () => any, allBindingsAccessor?: KnockoutAllBindingsAccessor) {
            events.afterPostback.subscribe((e) => {
                const bindings = allBindingsAccessor!();
                if (bindings["dotvvm-checked-pointer"]) {
                    const checked = bindings[bindings["dotvvm-checked-pointer"]];
                    if (ko.isObservable(checked)) {
                        if (checked.valueHasMutated) {
                            checked.valueHasMutated();
                        } else {
                            checked.notifySubscribers();
                        }
                    }
                }
            });
        }
    },
    'dotvvm-checked-pointer': {
    },

    "dotvvm-CheckState": {
        init: ko.getBindingHandler("checked").init,
        update(element: any, valueAccessor: () => any) {
            const value = ko.unwrap(valueAccessor());
            element.indeterminate = value == null;
        }
    },

    "dotvvm-checkedItems": {
        after: ko.bindingHandlers.checked.after,
        init: ko.bindingHandlers.checked.init,
        options: ko.bindingHandlers.checked.options,
        update(element: any, valueAccessor: () => any) {
            const value = valueAccessor();
            if (!Array.isArray(ko.unwrap(value))) {
                throw Error("The value of a `checkedItems` binding must be an array (i.e. not null nor undefined).");
            }
            // Note: As of now, the `checked` binding doesn't have an `update`. If that changes, invoke it here.
        }
    }
}
