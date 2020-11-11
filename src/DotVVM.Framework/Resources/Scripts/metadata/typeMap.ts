import { keys } from "../utils/objects";


let types: TypeMap = {};

export function getTypeInfo(typeId: string) {
    return types[typeId];
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