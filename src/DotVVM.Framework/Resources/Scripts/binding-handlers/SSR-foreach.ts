import makeUpdatableChildrenContextHandler from './makeUpdatableChildrenContext'
import foreachCollectionSymbol from './foreachCollectionSymbol'

ko.virtualElements.allowedBindings["dotvvm-SSR-foreach"] = true;
ko.virtualElements.allowedBindings["dotvvm-SSR-item"] = true;

type SeenUpdateElement = HTMLElement & { seenUpdate?: number };

export default {
    "dotvvm-SSR-foreach": {
        init: makeUpdatableChildrenContextHandler(
            (bindingContext, rawValue) => bindingContext.extend({ [foreachCollectionSymbol]: rawValue.data }),
            v => v.data != null)
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
