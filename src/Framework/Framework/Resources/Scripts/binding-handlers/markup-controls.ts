import { deserialize } from '../serialization/deserialize'
import { unmapKnockoutObservables } from '../state-manager';
import { logWarning } from '../utils/logging';
import { keys } from '../utils/objects';
import * as manager from '../viewModules/viewModuleManager';

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

function prepareViewModuleContexts(element: HTMLElement, value: any) {
    const contexts: any = {};
    for (const viewModuleName of value.modules) {
        contexts[viewModuleName] = manager.initViewModule(viewModuleName, value.viewId ?? element, element);
    }
    if (typeof value.viewId !== "string") {
        (element as any)[manager.viewModulesSymbol] = contexts;
    }
    return contexts;
}

ko.virtualElements.allowedBindings["dotvvm-with-control-properties"] = true;
ko.virtualElements.allowedBindings["dotvvm-with-view-modules"] = true;

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
            var newContext: any = { $control: value }

            // we have to merge these two bindings, since knockout does not support multiple binding to change the data context
            const viewModuleBinding = allBindings['dotvvm-with-view-modules']
            if (viewModuleBinding)
            {
                newContext.$viewModules = prepareViewModuleContexts(element, viewModuleBinding())
            }

            const innerBindingContext = bindingContext.extend(newContext);
            ko.applyBindingsToDescendants(innerBindingContext, element);
            return { controlsDescendantBindings: true }; // do not apply binding again
        }
    },
    'dotvvm-with-view-modules': {
        init: (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            if (!bindingContext) {
                throw new Error();
            }
            if (allBindings['dotvvm-with-control-properties'])
                return;

            const contexts = prepareViewModuleContexts(element, valueAccessor());

            const innerBindingContext = bindingContext.extend({ $viewModules: contexts });
            ko.applyBindingsToDescendants(innerBindingContext, element);
            return { controlsDescendantBindings: true }; // do not apply binding again
        }
    }
};
