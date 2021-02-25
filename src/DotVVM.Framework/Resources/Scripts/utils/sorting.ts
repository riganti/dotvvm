import { getTypeInfo } from "../metadata/typeMap";
type ElementType = string | number | boolean | Date | object;

export const orderBy = <T>(array: T[], selector: (item: T) => ElementType) =>
    orderByImpl(array, selector, getComparer(array[0], selector))

export const orderByDesc = <T>(array: T[], selector: (item: T) => ElementType) =>
    orderByImpl(array, selector, function (first: ElementType, second: ElementType) { return getComparer(array[0], selector)(first, second) * -1; })

const orderByImpl = <T>(array: T[], selector: (item: T) => ElementType, compare: (first: ElementType, second: ElementType) => number) => array
    .map((item, index) => ({ item, index }))
    .sort((first, second) => compare(ko.unwrap(selector(first.item)), ko.unwrap(selector(second.item))) || first.index - second.index)
    .map(({ item }) => item)


function getComparer<T>(element: T, selector: (item: T) => ElementType): (first: ElementType, second: ElementType) => number {
    let item = selector(element);
    const type = typeof item;

    if (type === "object") {
        const typeInfo = getTypeInfo(type);
        if (typeInfo.type === "enum") {
            // .NET enums are compared based on their underlying primitive value (i.e. we should not compare their string identifiers)
            typeInfo as EnumTypeMetadata;
            return function (first: ElementType, second: ElementType) {
                var firstNumeric = typeInfo.values[first as string];
                var secondNumeric = typeInfo.values[second as string];
                return defaultPrimitivesComparer(firstNumeric, secondNumeric);
            }
        }
        else {
            // TODO: maybe throw an error
            return defaultObjectsComparer;
        }
    }
    else {
        return defaultPrimitivesComparer;
    }
}

const defaultPrimitivesComparer = (first: ElementType, second: ElementType) => {
    return (first < second) ? -1 : (first == second) ? 0 : 1;
}

const defaultObjectsComparer = (first: ElementType, second: ElementType) => {
    return defaultPrimitivesComparer(JSON.stringify(first), JSON.stringify(second));
}
