const foreachCollectionSymbol = "$foreachCollectionSymbol" // knockout does not support symbols ;(

ko.virtualElements.allowedBindings["dotvvm-SSR-foreach"] = true;
ko.virtualElements.allowedBindings["dotvvm-SSR-item"] = true;

type SeenUpdateElement = HTMLElement & { seenUpdate?: number };

export default {
    "dotvvm-SSR-foreach": {
        init(element: Node, valueAccessor: () => any, allBindings?: KnockoutAllBindingsAccessor, viewModel?: any, bindingContext?: KnockoutBindingContext) {
            if (!bindingContext) {
                throw new Error();
            }

            let savedNodes: Node[] | undefined;
            let isInitial = true;
            ko.computed(() => {
                const rawValue = valueAccessor().data;
                ko.unwrap(rawValue); // we have to touch the observable in the binding so that the `getDependenciesCount` call knows about this dependency. If would be unwrapped only later (in the makeContextCallback) we would not have the savedNodes.

                // save a copy of the inner nodes on the initial update, but only if we have dependencies.
                if (isInitial && ko.computedContext.getDependenciesCount()) {
                    savedNodes = ko.utils.cloneNodes(ko.virtualElements.childNodes(element), true /* shouldCleanNodes */);
                }

                if (rawValue != null) {
                    if (!isInitial) {
                        ko.virtualElements.setDomNodeChildren(element, ko.utils.cloneNodes(savedNodes!));
                    }
                    ko.applyBindingsToDescendants(bindingContext.extend({ [foreachCollectionSymbol]: rawValue }), element);
                } else {
                    ko.virtualElements.emptyNode(element);
                }

                isInitial = false;
            }, null, { disposeWhenNodeIsRemoved: element });
            return { controlsDescendantBindings: true } // do not apply binding again
        }
    },
    "dotvvm-SSR-item": {
        init<T>(element: SeenUpdateElement, valueAccessor: () => T, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) {
            if (!bindingContext) {
                throw new Error();
            }

            const collection = (bindingContext as any)[foreachCollectionSymbol]
            if (!collection) {
                throw new Error();
            }

            const innerBindingContext = bindingContext.createChildContext(() => {
                    return ko.unwrap((ko.unwrap(collection) || [])[valueAccessor()]);
                }).extend({ $index: ko.pureComputed(valueAccessor) });
            ko.applyBindingsToDescendants(innerBindingContext, element);
            return { controlsDescendantBindings: true }; // do not apply binding again
        },
        update(element: SeenUpdateElement) {
            if (element.seenUpdate) {
                console.error(`dotvvm-SSR-item binding did not expect to see an update`);
            }
            element.seenUpdate = 1;
        }
    }
}
