import * as manager from '../viewModules/viewModuleManager';

const nameSymbol = Symbol("name");
const viewIdSymbol = Symbol("viewId");

ko.virtualElements.allowedBindings["dotvvm-named-command"] = true;
export default {
    'dotvvm-named-command': {
        init: (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            const value = valueAccessor();
            return { controlsDescendantBindings: false }; // do not apply binding again
        },
        update: (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            const value = valueAccessor();

            const newName = ko.unwrap(value.name);
            const oldName = (element as any)[nameSymbol];
            const newViewId = ko.unwrap(value.viewId);
            const oldViewId = (element as any)[viewIdSymbol];

            if (oldViewId) {
                manager.unregisterNamedCommand(oldViewId, oldName);
            }
            manager.registerNamedCommand(newViewId, newName, value.command, element);
        }
    }
};
