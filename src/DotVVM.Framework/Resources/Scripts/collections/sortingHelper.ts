import { getTypeInfo, getObjectTypeInfo } from "../metadata/typeMap";
import { primitiveTypes } from "../metadata/primitiveTypes";
type ElementType = string | number | boolean | Date | object;

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
            let firstNumeric = enumMetadataInfo.values[first as string];
            let secondNumeric = enumMetadataInfo.values[second as string];
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

function getMetadataInfo(original: any, selected: any): TypeMetadata | null {
    var path = getPath(original, selected);
    if (path === null) {
        return null;
    }
    else {
        let typeId = ko.unwrap((ko.unwrap(original).$type)) as string;
        let type = typeId as TypeDefinition;
        let pathSegmentIndex = 0;

        while (pathSegmentIndex < path.length) {
            if (Array.isArray(type)) {
                const index = parseInt(path[pathSegmentIndex++]);
                type = type[index];
            }
            else if (typeof type === "object") {
                if (type.type == "nullable") {
                    type = type.inner;
                }
                else if (type.type === "dynamic") {
                    // No metadata available
                    return null;
                }
            }
            else if (typeof type === "string") {
                if (type in primitiveTypes) {
                    // No metadata available
                    return null;
                } else {
                    let metadata = getTypeInfo(type);
                    if (metadata && metadata.type === "object") {
                        let pathSegment = path[pathSegmentIndex++];
                        type = metadata.properties[pathSegment].type;
                    }
                    else if (metadata && metadata.type === "enum") {
                        return metadata;
                    }
                }
            }
        }

        return resolveMetadata(type);
    }
}

function resolveMetadata(type: TypeDefinition): TypeMetadata | null { 
    if (!Array.isArray(type) && typeof type === "object" && type.type === "nullable") {
        // Unwrap nullables
        type = type.inner;
    }
    
    if (Array.isArray(type) || (typeof type === "object" && type.type === "dynamic")) {
        // We can not retrieve metadata for arrays and dynamics
        throw new Error("Could not resolve metadata!");
    }

    if (type as string in primitiveTypes) {
        // No metadata available
        return null;
    }
    else {
        return getTypeInfo(type as string);
    }
}

function getPath(from: any, target: any): string[] | null {
    from = ko.unwrap(from);
    target = ko.unwrap(target);

    for (let key in from) {
        let item = ko.unwrap(from[key]);
        if (item && typeof item === "object") {
            let subPath = getPath(item, target);
            if (subPath) {
                subPath.unshift(key);
                return subPath;
            }
        }
        else if (item === target) {
            return [key];
        }
    }

    return null;
}

const defaultPrimitivesComparer = (first: ElementType, second: ElementType, ascending: boolean) => {
    let comparision = (first < second) ? -1 : (first == second) ? 0 : 1;
    return (ascending) ? comparision : comparision * -1;
}
