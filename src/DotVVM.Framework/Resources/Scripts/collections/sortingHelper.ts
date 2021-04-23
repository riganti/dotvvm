import { getMetadataInfo } from "../metadata/metadataInfo";
type ElementType = string | number | boolean;

export const orderBy = <T>(array: T[], selector: (item: T) => ElementType) =>
    orderByImpl(array, selector, getComparer(array[0], selector, true))

export const orderByDesc = <T>(array: T[], selector: (item: T) => ElementType) =>
    orderByImpl(array, selector, getComparer(array[0], selector, false))

function orderByImpl<T>(array: T[], selector: (item: T) => ElementType, compare: (first: ElementType, second: ElementType) => number): T[] {

    if ((!array || array.length < 2))
        return array;

    return array
        .map((item, index) => ({ item, index }))
        .sort((first, second) => compare(ko.unwrap(selector(first.item)), ko.unwrap(selector(second.item))) || first.index - second.index)
        .map(({ item }) => item);
}

function getComparer<T>(element: T, selector: (item: T) => ElementType, ascending: boolean): (first: ElementType, second: ElementType) => number {
    let metadataInfo = getMetadataInfo(element, selector(element));

    if (metadataInfo !== null && metadataInfo.type === "object") {
        throw new Error("Can not compare objects!");
    }
    else if (metadataInfo !== null && metadataInfo.type === "enum") {
        // Enums should be compared based on their underlying primitive values
        // This is the same behaviour as used by .NET
        let enumMetadataInfo = metadataInfo as EnumTypeMetadata;
        return function (first: ElementType, second: ElementType) {
            let firstNumeric = (typeof(first) === "number") ? first : enumMetadataInfo.values[first as string];
            let secondNumeric = (typeof(second) === "number") ? second : enumMetadataInfo.values[second as string];
            return defaultPrimitivesComparer(firstNumeric, secondNumeric, ascending);
        }
    }
    else {
        // We are comparing primitive types
        return function (first: ElementType, second: ElementType) {
            return defaultPrimitivesComparer(first, second, ascending);
        }
    }
}

const defaultPrimitivesComparer = (first: ElementType, second: ElementType, ascending: boolean) => {
    let comparision = (first < second) ? -1 : (first == second) ? 0 : 1;
    return (ascending) ? comparision : comparision * -1;
}
