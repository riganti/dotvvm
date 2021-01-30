import { keys } from '../utils/objects';
import * as manager from '../viewModules/viewModuleManager';

ko.virtualElements.allowedBindings["dotvvm-named-command"] = true;
export default {
    'dotvvm-named-command': {
        init: (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            if (!bindingContext) {
                throw new Error();
            }

            const value = valueAccessor();
            
            if (!value.viewId) {
                throw new Error('Cannot initialize view modules. Property viewId not defined.');
            }

            if (!value.name) {
                throw new Error('Cannot initialize view modules. Property name not defined.');
            }

            if (!value.command) {
                throw new Error('Cannot initialize view modules. Property command not defined.');
            }

            manager.registerNamedCommand(value.viewId, value.name, value.command);

            return { controlsDescendantBindings: false }; // do not apply binding again
        }
    }
};
