import { keys } from "../utils/objects";
import { primitiveTypes } from "./primitiveTypes";
import { getTypeInfo } from "./typeMap";

export function tryCoerce(value: any, type: TypeDefinition): CoerceResult {
    if (type instanceof Array) {
        return tryCoerceArray(value, type[0]);
    } else if (typeof type === "object") {
        if (type.type == "nullable") {
            return tryCoerceNullable(value, type.inner);
        } else if (type.type == "enum") {
            return tryCoerceEnum(value, type.values);
        }
    } else if (typeof type === "string") {
        if (primitiveTypes.hasOwnProperty(type)) {
            return tryCoercePrimitiveType(value, type);
        } else {
            return tryCoerceObject(value, type);
        }
    } 
    throw "Unsupported type metadata!"
}

function tryCoerceNullable(value: any, innerType: TypeDefinition): CoerceResult {
    if (value == null) {
        return { value: null };
    } else if (typeof value === "undefined" || value == "") {
        return { value: null, wasCoerced: true };
    }
    else {
        return tryCoerce(value, innerType);
    }
}    

function tryCoerceEnum(value: any, values: { [name: string]: number }): CoerceResult {
    if (typeof value === "string" && values.hasOwnProperty(value)) {
        return { value };
    } else if (typeof value === "number") {
        const matched = keys(values).filter(k => values[k] === value);
        if (matched.length) {
            return { value: matched[0], wasCoerced: true }
        } 
    }
}

function tryCoerceArray(value: any, innerType: TypeDefinition): CoerceResult {
    if (value instanceof Array) {
        const items = value.map(i => tryCoerce(i, innerType));
        let wasCoerced = false;
        for (let i = 0; i < items.length; i++) {
            if (!items[i]) {
                return;
            } else if (items[i]!.wasCoerced) {
                wasCoerced = true;
            }
        }
        if (!wasCoerced) {
            return { value };
        } else {
            return { value: items.map(i => i!.value), wasCoerced: true };
        }
    }
}

function tryCoercePrimitiveType(value: any, type: string): CoerceResult {
    return primitiveTypes[type].tryCoerce(value);
}

function tryCoerceObject(value: any, type: string): CoerceResult {
    if (value == null) {
        return { value: null };
    } else if (typeof value === "object") {
        const typeInfo = getTypeInfo(type);
        let wasCoerced = false;
        let patch: any = {};
        for (let k of keys(typeInfo)) {
            const result = tryCoerce(value[k], typeInfo[k].type);
            if (!result) {
                return;
            } else if (result.wasCoerced) {
                wasCoerced = true;
                patch[k] = result.value;
            }
        }
        if (!wasCoerced) {
            return { value };
        } else {
            return { value: { ...value, ...patch }, wasCoerced: true };
        }
    } else if (typeof value === "undefined") {
        return { value: null, wasCoerced: true };
    }
}
