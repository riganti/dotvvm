import { keys } from '../utils/objects';
import * as manger from '../viewModules/viewModuleManager';

ko.virtualElements.allowedBindings["dotvvm-with-view-modules"] = true;
export default {
    'dotvvm-with-view-modules': {
        init: (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            if (!bindingContext) {
                throw new Error();
            }

            const value = valueAccessor();

            console.info(value);

            if (!value.viewId) {
                throw new Error('Cannot initialize view modules. Property viewId not defined.');
            }

            if (!value.modules) {
                throw new Error('Cannot initialize view modules. Property modules not defined.');
            }

            for (const viewModuleName of value.modules) {
                manger.initViewModule(viewModuleName, value.viewId, element)
            }
            return { controlsDescendantBindings: false }; // do not apply binding again
        }
    }
};
