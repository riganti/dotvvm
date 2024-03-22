import { enumStringToInt } from "../metadata/enums";
import { getTypeInfo } from "../metadata/typeMap";
type ElementType = string | number | boolean;

export const orderBy = <T>(array: T[], selector: (item: T) => ElementType, typeId: string | null) =>
    orderByImpl(array, selector, getComparer(typeId, true))

export const orderByDesc = <T>(array: T[], selector: (item: T) => ElementType, typeId: string | null) =>
    orderByImpl(array, selector, getComparer(typeId, false))

function orderByImpl<T>(array: T[], selector: (item: T) => ElementType, compare: (first: ElementType, second: ElementType) => number): T[] {
    if ((!array || array.length < 2))
        return array;

    // JS sort is stable: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/sort#sort_stability
    array = Array.from(array)
    array.sort((first, second) => compare(ko.unwrap(selector(first)), ko.unwrap(selector(second))))
    return array;
}

function getComparer(typeId: string | null, ascending: boolean): (first: ElementType, second: ElementType) => number {
    const metadataInfo = (typeId != null) ? getTypeInfo(typeId) : null;
    if (metadataInfo?.type === "enum") {
        // Enums should be compared based on their underlying primitive values
        // This is the same behaviour as used by .NET
        const enumMetadataInfo = metadataInfo as EnumTypeMetadata;
        return function (first: ElementType, second: ElementType) {
            return defaultPrimitivesComparer(enumStringToInt(first as any, enumMetadataInfo), enumStringToInt(second as any, enumMetadataInfo), ascending);
        }
    }
    else {
        // We are comparing primitive types
        return function (first: ElementType, second: ElementType) {
            return defaultPrimitivesComparer(first, second, ascending);
        }
    }
}

const defaultPrimitivesComparer = (first: ElementType | null | undefined, second: ElementType | null | undefined, ascending: boolean) => {
    // nulls are first in ascending order in .NET
    const comparison = (first == second) ? 0 : (first == null || second != null && first < second) ? -1 : 1;
    return (ascending) ? comparison : comparison * -1;
}
