import { keys } from "../utils/objects";
import { primitiveTypes } from "./primitiveTypes";


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

export function getCurrentTypeMap() {
    return types;
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


export function formatTypeName(type: TypeDefinition, prefix = "", suffix = ""): string {
    if (!compileConstants.debug)
        return JSON.stringify(type)

    if (typeof type === "string") {
        let debugName = types[type]?.debugName
        if (debugName)
            return `${prefix}${debugName}${suffix} (${prefix}${type}${suffix})`
        else
            return prefix + type + suffix
    }
    if (Array.isArray(type)) {
        return formatTypeName(type[0], prefix, "[]" + suffix)
    }
    if (type.type == "nullable") {
        return formatTypeName(type.inner, prefix, "?" + suffix)
    }
    if (type.type == "dynamic") {
        return prefix + "dynamic" + suffix
    }
    const typeCheck: never = type
    return undefined as any
}

type KeyFunction = (item: any) => string;
const keyFunctions: { [name: string]: KeyFunction | undefined } = {};
export function tryGetKeyFunction(type: string): KeyFunction | undefined {
    if (type in keyFunctions) {
        return keyFunctions[type];
    }
    return keyFunctions[type] = buildKeyFunction(type);
}
function buildKeyFunction(type: string): KeyFunction | undefined {
    if (!(type in primitiveTypes)) {
        const typeInfo = getTypeInfo(type);
        if (typeInfo.type === "object") {
            const props = getTypeProperties(type);
            // NB: validation that property type is primitive or nullable is done on the server
            const keyProperties = Object.entries(props).filter(e => e[1].isKey).map(e => e[0]);
            if (keyProperties.length) {
                return (item: any) => JSON.stringify(keyProperties.map(p => ko.unwrap(ko.unwrap(item)?.[p])));
            }
        }
    }
}