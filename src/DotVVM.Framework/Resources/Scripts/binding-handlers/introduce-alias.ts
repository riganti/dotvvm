import { deserialize } from '../serialization/deserialize'
import makeUpdatableChildrenContextHandler from './makeUpdatableChildrenContext'
import foreachCollectionSymbol from './foreachCollectionSymbol'
import { getDataSourceItems } from '../utils/evaluator'

function createWrapperComputed<T>(accessor: () => KnockoutObservable<T> | T, propertyDebugInfo: string | null = null) {
    const computed = ko.pureComputed({
        read() {
            return ko.unwrap(accessor());
        },
        write(value: T) {
            const val = accessor();
            if (ko.isObservable(val)) {
                val(value);
            } else {
                console.warn(`Attempted to write to readonly property` + (!propertyDebugInfo ? `` : ` ` + propertyDebugInfo) + `.`);
            }
        }
    });
    computed["wrappedProperty"] = accessor;
    return computed;
}

ko.virtualElements.allowedBindings["dotvvm_withControlProperties"] = true;
ko.virtualElements.allowedBindings["withGridViewDataSet"] = true;
ko.virtualElements.allowedBindings["dotvvm-introduceAlias"] = true;
export default {
    "dotvvm_withControlProperties": {
        init: (element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) => {
            if (!bindingContext) {
                throw new Error();
            }

            const value = valueAccessor();
            for (const prop of Object.keys(value)) {

                value[prop] = createWrapperComputed(
                    () => {
                        const property = valueAccessor()[prop];
                        return !ko.isObservable(property) ? deserialize(property) : property
                    },
                    `'${prop}' at '${valueAccessor.toString()}'`);
            }
            const innerBindingContext = bindingContext.extend({ $control: value });
            ko.applyBindingsToDescendants(innerBindingContext, element);
            return { controlsDescendantBindings: true }; // do not apply binding again
        }
    },
    "dotvvm-introduceAlias": {
        init(element: HTMLElement, valueAccessor: () => any, allBindings?: any, viewModel?: any, bindingContext?: KnockoutBindingContext) {
            if (!bindingContext) {
                throw new Error();
            }

            const value = valueAccessor();
            const extendBy: any = {};
            for (const prop of Object.keys(value)) {
                const propPath = prop.split('.');
                let obj = extendBy;
                for (const p in propPath.slice(0, -1)) {
                    if (extendBy.hasOwnProperty(p)) {
                        obj = extendBy[p] || (extendBy[p] = {});
                    }
                }
                obj[propPath[propPath.length - 1]] = createWrapperComputed(() => valueAccessor()[prop], `'${prop}' at '${valueAccessor.toString()}'`);
            }
            const innerBindingContext = bindingContext.extend(extendBy);
            ko.applyBindingsToDescendants(innerBindingContext, element);
            return { controlsDescendantBindings: true }; // do not apply binding again
        }
    },
    "withGridViewDataSet": {
        init: makeUpdatableChildrenContextHandler(
            (bindingContext, value) => bindingContext.extend({ $gridViewDataSet: value, [foreachCollectionSymbol]: getDataSourceItems(value) }),
            _ => true
        )
    }
};
