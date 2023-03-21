import { deserialize } from '../serialization/deserialize'
import { currentStateSymbol, unmapKnockoutObservables } from '../state-manager';
import { logWarning } from '../utils/logging';
import { defineConstantProperty, keys } from '../utils/objects';
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
            if (ko.isWriteableObservable(val)) {
                val(value);
            } else {
                logWarning("binding-handler", `Attempted to write to readonly property` + (!propertyDebugInfo ? `` : ` ` + propertyDebugInfo) + `.`);
            }
        }
    });
    (computed as any)["wrappedProperty"] = accessor;
    Object.defineProperty(computed, "state", {
        get: () => {
            const x = accessor() as any
            return (x && x.state) ?? unmapKnockoutObservables(x, true)
        }
    })
    defineConstantProperty(computed, "setState", (state: any) => {
        const x = accessor() as any
        if (compileConstants.debug && typeof x.setState != "function") {
            throw new Error(`Cannot set state of property ${propertyDebugInfo}.`)
        }
        x.setState(state)
    })
    defineConstantProperty(computed, "patchState", (state: any) => {
        const x = accessor() as any
        if (compileConstants.debug && typeof x.patchState != "function") {
            throw new Error(`Cannot patch state of property ${propertyDebugInfo}.`)
        }
        x.patchState(state)
    })
    defineConstantProperty(computed, "updateState", (updateFunction: (state: any) => any) => {
        const x = accessor() as any
        if (compileConstants.debug && typeof x.updateState != "function") {
            throw new Error(`Cannot patch state of property ${propertyDebugInfo}.`)
        }
        x.updateState(updateFunction)
    })
    defineConstantProperty(computed, "updateState", (updateFunction: (state: any) => any) => {
        const x = accessor() as any
        if (compileConstants.debug && typeof x.updateState != "function") {
            throw new Error(`Cannot update state of property ${propertyDebugInfo}.`)
        }
        x.updateState(updateFunction)
    })
    return computed;
}

function prepareViewModuleContexts(element: HTMLElement, value: any, properties: object) {
    if (compileConstants.debug && value.modules.length == 0) {
        throw new Error(`dotvvm-with-view-modules binding was used without any modules.`)
    }
    const contexts: any = {};
    for (const viewModuleName of value.modules) {
        contexts[viewModuleName] = manager.initViewModule(viewModuleName, value.viewId ?? element, element, properties);
    }
    if (typeof value.viewId !== "string") {
        if (compileConstants.debug && value.viewId != null) {
            throw new Error(`View id ${value.viewId} is invalid.`)
        }
        (element as any)[manager.viewModulesSymbol] = contexts;
    }
    return contexts;
}

ko.virtualElements.allowedBindings["dotvvm-with-control-properties"] = true;
ko.virtualElements.allowedBindings["dotvvm-with-view-modules"] = true;

export function wrapControlProperties(valueAccessor: () => any) {
    const value = valueAccessor();
    for (const prop of keys(value)) {
        if (isCommand(value, prop)) {
            const commandFunction = value[prop];
            value[prop] = createWrapperComputed(() => commandFunction);
        } else {
            value[prop] = createWrapperComputed(
                () => {
                    const value = valueAccessor()[prop];
                    // if it's observable or FakeObservableObject, we assume that we don't need to wrap it in observables.
                    const isWrapped = ko.isObservable(value) || (value && typeof value == 'object' && currentStateSymbol in value)
                    return isWrapped ? value : deserialize(value)
                },
                `'${prop}' at '${valueAccessor.toString()}'`);
        }
    }
    return value
}

export default {
    'dotvvm-with-control-properties': {
        init: (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            var newContext: any = { $control: wrapControlProperties(valueAccessor) }

            // we have to merge these two bindings, since knockout does not support multiple binding to change the data context
            const viewModuleBinding = allBindings.get('dotvvm-with-view-modules')
            if (viewModuleBinding)
            {
                newContext.$viewModules = prepareViewModuleContexts(element, viewModuleBinding, newContext.$control)
            }

            const innerBindingContext = bindingContext!.extend(newContext);
            ko.applyBindingsToDescendants(innerBindingContext, element);
            return { controlsDescendantBindings: true }; // do not apply binding again
        }
    },
    'dotvvm-with-view-modules': {
        init: (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            if (allBindings.has('dotvvm-with-control-properties'))
                return;

            const contexts = prepareViewModuleContexts(element, valueAccessor(), {});

            const innerBindingContext = bindingContext!.extend({ $viewModules: contexts });
            ko.applyBindingsToDescendants(innerBindingContext, element);
            return { controlsDescendantBindings: true }; // do not apply binding again
        }
    },
};
