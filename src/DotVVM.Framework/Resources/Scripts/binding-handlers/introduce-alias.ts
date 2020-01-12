import { deserialize } from '../serialization/deserialize'

function createWrapperComputed<T>(accessor: () => KnockoutObservable<T> | T, propertyDebugInfo: string | null = null) {
    const computed = ko.pureComputed({
        read() {
            return ko.unwrap(accessor());
        },
        write(value: T) {
            const val = accessor();
            if (ko.isObservable(val)) {
                val(value);
            } else {
                console.warn(`Attempted to write to readonly property` + (!propertyDebugInfo ? `` : ` ` + propertyDebugInfo) + `.`);
            }
        }
    });
    computed["wrappedProperty"] = accessor;
    return computed;
}

ko.virtualElements.allowedBindings["dotvvm_withControlProperties"] = true;
export default {
    "dotvvm_withControlProperties": {
        init: (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            if (!bindingContext) {
                throw new Error();
            }

            const value = valueAccessor();
            for (const prop of Object.keys(value)) {

                value[prop] = createWrapperComputed(
                    () => {
                        const property = valueAccessor()[prop];
                        return !ko.isObservable(property) ? deserialize(property) : property
                    },
                    `'${prop}' at '${valueAccessor.toString()}'`);
            }
            const innerBindingContext = bindingContext.extend({ $control: value });
            ko.applyBindingsToDescendants(innerBindingContext, element);
            return { controlsDescendantBindings: true }; // do not apply binding again
        }
    }
};
