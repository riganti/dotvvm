export default (
    makeContextCallback: (bindingContext: KnockoutBindingContext, value: any) => any,
    shouldDisplay: (value: any) => boolean
) => (element: Node, valueAccessor: () => any, allBindings?: KnockoutAllBindingsAccessor, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
    if (!bindingContext) {
        throw new Error();
    }

    let savedNodes: Node[] | undefined;
    ko.computed(() => {
        const rawValue = valueAccessor();

        // save a copy of the inner nodes on the initial update, but only if we have dependencies.
        if (!savedNodes && ko.computedContext.getDependenciesCount()) {
            savedNodes = ko.utils.cloneNodes(ko.virtualElements.childNodes(element), true /* shouldCleanNodes */);
        }

        if (shouldDisplay(rawValue)) {
            if (savedNodes) {
                ko.virtualElements.setDomNodeChildren(element, ko.utils.cloneNodes(savedNodes));
            }
            ko.applyBindingsToDescendants(makeContextCallback(bindingContext, rawValue), element);
        } else {
            ko.virtualElements.emptyNode(element);
        }

    }, null, { disposeWhenNodeIsRemoved: element });
    return { controlsDescendantBindings: true } // do not apply binding again
}
