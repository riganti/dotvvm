function makeUpdatableChildrenContextHandler(
    makeContextCallback: (bindingContext: KnockoutBindingContext, value: any, allBindings: KnockoutAllBindingsAccessor | undefined) => any,
    shouldDisplay: (value: any) => boolean
) {
    return (element: Node, valueAccessor: () => any, allBindings: KnockoutAllBindingsAccessor | undefined, _viewModel: any, bindingContext: KnockoutBindingContext | undefined) => {
        if (!bindingContext) throw new Error()

        var savedNodes: Node[] | undefined;
        var isInitial = true;
        ko.computed(function () {
            var rawValue = valueAccessor();
            ko.unwrap(rawValue); // we have to touch the observable in the binding so that the `getDependenciesCount` call knows about this dependency. If would be unwrapped only later (in the makeContextCallback) we would not have the savedNodes.

            // Save a copy of the inner nodes on the initial update, but only if we have dependencies.
            if (isInitial && ko.computedContext.getDependenciesCount()) {
                savedNodes = ko.utils.cloneNodes(ko.virtualElements.childNodes(element), true /* shouldCleanNodes */);
            }

            if (shouldDisplay(rawValue)) {
                if (!isInitial) {
                    ko.virtualElements.setDomNodeChildren(element, ko.utils.cloneNodes(savedNodes!));
                }
                ko.applyBindingsToDescendants(makeContextCallback(bindingContext, rawValue, allBindings), element);
            } else {
                ko.virtualElements.emptyNode(element);
            }

            isInitial = false;

        }, null, { disposeWhenNodeIsRemoved: element });
        return { controlsDescendantBindings: true } // do not apply binding again
    };
}

ko.virtualElements.allowedBindings["dotvvm-gridviewdataset"] = true;
export default {
    "dotvvm-gridviewdataset": {
        init: makeUpdatableChildrenContextHandler(
            (bindingContext: KnockoutBindingContext, value, allBindings) => bindingContext.extend({
                $gridViewDataSetHelper: {
                    ...value,
                    isInEditMode: function ($context: any) {
                        let columnName = this.dataSet.RowEditOptions().PrimaryKeyPropertyName();
                        columnName = this.mapping[columnName] || columnName;
                        return this.dataSet.RowEditOptions().EditRowId() === $context.$data[columnName]();
                    }
                }
            }),
            _ => true
        )
    }
}