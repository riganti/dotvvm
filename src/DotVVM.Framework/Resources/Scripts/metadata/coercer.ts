import { CoerceError } from "../shared-classes";
import { keys } from "../utils/objects";
import { primitiveTypes } from "./primitiveTypes";
import { getTypeInfo } from "./typeMap";

export function tryCoerce(value: any, type: TypeDefinition, strict: boolean = false): CoerceResult {
    if (type instanceof Array) {
        return tryCoerceArray(value, type[0], strict);
    } else if (typeof type === "object") {
        if (type.type == "nullable") {
            return tryCoerceNullable(value, type.inner, strict);
        } else if (type.type == "enum") {
            return tryCoerceEnum(value, type.values, strict);
        }
    } else if (typeof type === "string") {
        if (type in primitiveTypes) {
            return tryCoercePrimitiveType(value, type, strict);
        } else {
            return tryCoerceObject(value, type, strict);
        }
    } 
    throw "Unsupported type metadata!";
}

export function coerce(value: any, type: TypeDefinition): any {
    return tryCoerce(value, type, true)!.value;
}

function tryCoerceNullable(value: any, innerType: TypeDefinition, strict: boolean = false): CoerceResult {
    if (value == null) {
        return { value: null };
    } else if (typeof value === "undefined" || value == "") {       // TODO: shall we support empty strings?
        return { value: null, wasCoerced: true };
    } else {
        return tryCoerce(value, innerType, strict);
    }
}    

function tryCoerceEnum(value: any, values: { [name: string]: number }, strict: boolean): CoerceResult {
    if (typeof value === "string" && value in values) {
        return { value };
    } else if (typeof value === "number") {
        const matched = keys(values).filter(k => values[k] === value);
        if (matched.length) {
            return { value: matched[0], wasCoerced: true }
        }
    }
    if (strict) {
        throw new CoerceError(`Cannot cast '${value}' to type 'Enum(${keys(values).join(",")})'.`);
    }
}

function tryCoerceArray(value: any, innerType: TypeDefinition, strict: boolean): CoerceResult {
    if (value instanceof Array) {
        let wasCoerced = false;
        const items = [];
        for (let i = 0; i < value.length; i++) {
            try {
                const item = tryCoerce(value[i], innerType, strict);
                if (!item) {                
                    return;
                }
                if (item.wasCoerced) {
                    wasCoerced = true;
                }
                items.push(item.value);
            } catch (err) {
                if (err instanceof CoerceError) {
                    err.prependPathFragment("#" + i);
                }
                throw err;
            }
        }
        if (!wasCoerced) {
            return { value };
        } else {
            return { value: items, wasCoerced: true };
        }
    }
    if (strict) {
        throw new CoerceError(`Value '${value}' is not an array of type '${innerType}'.`);
    }
}

function tryCoercePrimitiveType(value: any, type: string, strict: boolean): CoerceResult {
    const result = primitiveTypes[type].tryCoerce(value);
    if (!result && strict) {
        throw new CoerceError(`Cannot coerce '${value}' to type '${type}'.`);
    }
    return result;
}

function tryCoerceObject(value: any, type: string, strict: boolean): CoerceResult {
    if (value == null) {
        return { value: null };
    } else if (typeof value === "object") {
        const typeInfo = getTypeInfo(type);
        let wasCoerced = false;
        let patch: any = {};
        for (let k of keys(typeInfo)) {
            try {
                const result = tryCoerce(value[k], typeInfo[k].type, strict);
                if (!result) {
                    return;
                }
                if (result.wasCoerced) {
                    wasCoerced = true;
                    patch[k] = result.value;
                }
            } catch (err) {
                if (err instanceof CoerceError) {
                    err.prependPathFragment(k);
                }
                throw err;
            }
        }
        if (!("$type" in value)) {
            patch["$type"] = type;
            wasCoerced = true;
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
