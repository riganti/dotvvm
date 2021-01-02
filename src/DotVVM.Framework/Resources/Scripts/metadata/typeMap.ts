import { keys } from "../utils/objects";


let types: TypeMap = {};

export function getTypeInfo(typeId: string): TypeMetadata {
    var result = types[typeId];
    if (!result) {
        throw `Cannot find type metadata for '${typeId}'!`;
    }
    return result;
}

export function getObjectTypeInfo(typeId: string): ObjectTypeMetadata {
    const typeInfo = getTypeInfo(typeId);
    if (typeInfo.type !== "object") {
        throw `Cannot convert object to an enum type ${typeId}!`;
    }
    return typeInfo;
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