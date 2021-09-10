import { deserialize } from '../serialization/deserialize'
import { unmapKnockoutObservables } from '../state-manager';
import { logWarning } from '../utils/logging';
import { keys } from '../utils/objects';

function isCommand(value: any, prop: string) {
    return !ko.isObservable(value[prop]) && typeof value[prop] === 'function';
}

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
                logWarning("binding-handler", `Attempted to write to readonly property` + (!propertyDebugInfo ? `` : ` ` + propertyDebugInfo) + `.`);
            }
        }
    });
    computed["wrappedProperty"] = accessor;
    Object.defineProperty(computed, "state", {
        get: () => {
            const x = (accessor() as any || {})
            return x.state ?? unmapKnockoutObservables(x)
        },
        configurable: false,
        enumerable: false
    })
    return computed;
}

ko.virtualElements.allowedBindings["dotvvm-with-control-properties"] = true;

export default {
    'dotvvm-with-control-properties': {
        init: (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            if (!bindingContext) {
                throw new Error();
            }

            const value = valueAccessor();
            for (const prop of keys(value)) {
                if (isCommand(value, prop)) {
                    const commandFunction = value[prop];
                    value[prop] = createWrapperComputed(() => commandFunction);
                } else {
                    value[prop] = createWrapperComputed(
                        () => {
                            const property = valueAccessor()[prop];
                            return !ko.isObservable(property) ? deserialize(property) : property
                        },
                        `'${prop}' at '${valueAccessor.toString()}'`);
                }
            }
            const innerBindingContext = bindingContext.extend({ $control: value });
            ko.applyBindingsToDescendants(innerBindingContext, element);
            return { controlsDescendantBindings: true }; // do not apply binding again
        }
    }
};


