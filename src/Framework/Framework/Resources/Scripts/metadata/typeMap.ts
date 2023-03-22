import { keys } from "../utils/objects";


let types: TypeMap = {};

export function getTypeInfo(typeId: string | object): TypeMetadata {

    if (typeof typeId === "string") {
        var result = types[typeId];
        if (result) {
            return result;
        }
    }
    else if (typeof typeId === "object") {
        var typeInfo = typeId as any;
        if (ko.unwrap(typeInfo?.type) === "dynamic")
            return { type: "dynamic" };
    }

    throw new Error(`Cannot find type metadata for '${typeId}'!`);
}

export function getObjectTypeInfo(typeId: string): ObjectTypeMetadata | DynamicTypeMetadata {
    const typeInfo = getTypeInfo(typeId);
    if (typeInfo.type === "enum") {
        throw new Error(`Cannot convert object to an enum type ${typeId}!`);
    }
    return typeInfo;
}

export function getTypeProperties(typeId: string | object | null | undefined): { [prop: string]: PropertyMetadata } {
    if (typeof typeId === "string") {
        var typeInfo = getObjectTypeInfo(typeId) as ObjectTypeMetadata;
        return typeInfo.properties;
    }
    // unknown type or dynamic type
    return {}
}

export function getKnownTypes() {
    return keys(types);
}

export function updateTypeInfo(newTypes: TypeMap | undefined) {
    types = { ...types, ...newTypes };
}

export function replaceTypeInfo(newTypes: TypeMap | undefined) {
    types = newTypes || {};
}

export function areObjectTypesEqual(currentValue: any, newVal: any): boolean {
    if (currentValue["$type"] && currentValue["$type"] === newVal["$type"]) {
        // objects with type must have a same type
        return true;
    }
    else if (!currentValue["$type"] && !newVal["$type"]) {
        // dynamic objects must have the same properties
        let currentValueKeys = keys(currentValue);
        let newValKeys = keys(newVal);
        return currentValueKeys.length == newValKeys.length &&
            new Set([...currentValueKeys, ...newValKeys]).size == currentValueKeys.length;
    }
    return false;
}
