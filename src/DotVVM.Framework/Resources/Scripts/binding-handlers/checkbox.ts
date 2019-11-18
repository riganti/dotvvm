import * as events from '../DotVVM.Events';

export default {
    'dotvvm-checkbox-updateAfterPostback': {
        init(element: HTMLElement, valueAccessor: () => any, allBindingsAccessor?: KnockoutAllBindingsAccessor) {
            events.afterPostback.subscribe((e) => {
                var bindings = allBindingsAccessor!();
                if (bindings["dotvvm-checked-pointer"]) {
                    var checked = bindings[bindings["dotvvm-checked-pointer"]];
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
        init() {}
    },

    "dotvvm-CheckState": {
        init(element: HTMLElement, valueAccessor: () => any, allBindingsAccessor?: KnockoutAllBindingsAccessor, viewModel?: any, bindingContext?: KnockoutBindingContext) {
            ko.getBindingHandler("checked").init!(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
        },
        update(element: any, valueAccessor: () => any) {
            let value = ko.unwrap(valueAccessor());
            element.indeterminate = value == null;
        }
    }
}
