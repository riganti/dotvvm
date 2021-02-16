import * as manager from '../viewModules/viewModuleManager';

const viewIdSymbol = Symbol("viewId");

ko.virtualElements.allowedBindings["dotvvm-with-view-modules"] = true;
export default {
    'dotvvm-with-view-modules': {
        init: (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            const value = valueAccessor();
            return { controlsDescendantBindings: false }; // do not apply binding again
        },
        update: (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            const value = valueAccessor();
            const newViewId = ko.unwrap(value.viewId);

            const oldViewId = (element as any)[viewIdSymbol];
            if (!oldViewId) {
                for (const viewModuleName of value.modules) {
                    manager.initViewModule(viewModuleName, newViewId, element);
                }
            } else {
                for (const viewModuleName of value.modules) {
                    manager.renameViewModule(viewModuleName, oldViewId, newViewId, element);                
                }
            }

            (element as any)[viewIdSymbol] = newViewId;
        }
    }
};
