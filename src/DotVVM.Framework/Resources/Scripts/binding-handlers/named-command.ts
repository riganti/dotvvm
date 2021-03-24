import * as manager from '../viewModules/viewModuleManager';

ko.virtualElements.allowedBindings["dotvvm-named-command"] = true;
export default {
    'dotvvm-named-command': {
        init: (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            const value = valueAccessor();
            manager.registerNamedCommand(value.viewIdOrElement, value.name, value.command, element);
            return { controlsDescendantBindings: false }; // do not apply binding again
        }
    }
};
