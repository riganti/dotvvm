import * as manager from '../viewModules/viewModuleManager';

ko.virtualElements.allowedBindings["dotvvm-with-view-modules"] = true;
export default {
    'dotvvm-with-view-modules': {
        init: (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            if (!bindingContext) {
                throw new Error();
            }

            const value = valueAccessor();
            const contexts: any = {};
            for (const viewModuleName of value.modules) {
                contexts[viewModuleName] = manager.initViewModule(viewModuleName, value.viewIdOrElement, element);
            }
            if (typeof value.viewIdOrElement !== "string") {
                (element as any)[manager.viewModulesSymbol] = contexts;
            }

            const innerBindingContext = bindingContext.extend({ $viewModules: contexts });
            ko.applyBindingsToDescendants(innerBindingContext, element);
            return { controlsDescendantBindings: true }; // do not apply binding again
        }
    }
};
