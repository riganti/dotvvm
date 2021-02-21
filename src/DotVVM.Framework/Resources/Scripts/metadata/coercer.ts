import { CoerceError } from "../shared-classes";
import { keys } from "../utils/objects";
import { primitiveTypes } from "./primitiveTypes";
import { getObjectTypeInfo, getTypeInfo } from "./typeMap";

/**
 * Validates type of value
 * @param type Expected type of type value.
 * @param originalValue Value that is known to be valid instance of type. It is used to perform incremental validation.
 */
export function tryCoerce(value: any, type: TypeDefinition, originalValue: any = undefined): CoerceResult {
    if (originalValue === value && value !== undefined) {
        // we trust that the originalValue is already valid
        // except when it's undefined - we use that as "we do not know" value - but revalidation is cheap in this case
        return { value }
    }

    if (value) {
        type = value.$type ?? type
    }

    if (Array.isArray(type)) {
        return tryCoerceArray(value, type[0], originalValue);
    } else if (typeof type === "object") {
        if (type.type == "nullable") {
            return tryCoerceNullable(value, type.inner, originalValue);
        } else if (type.type === "dynamic") {
            return tryCoerceDynamic(value, originalValue);
        }
    } else if (typeof type === "string") {
        if (type in primitiveTypes) {
            return tryCoercePrimitiveType(value, type);
        } else {
            var typeInfo = getTypeInfo(type);
            if (typeInfo && typeInfo.type === "object") {
                return tryCoerceObject(value, type, typeInfo, originalValue);
            }
            else if (typeInfo && typeInfo.type === "enum") {
                return tryCoerceEnum(value, typeInfo);
            }            
        }
    } 
    return new CoerceError(`Unsupported type metadata ${JSON.stringify(type)}!`);
}

export function coerce(value: any, type: TypeDefinition, originalValue: any = undefined): any {
    const x = tryCoerce(value, type, originalValue)
    if (x.isError) {
        throw x
    } else {
        return x.value
    }
}

function tryCoerceNullable(value: any, innerType: TypeDefinition, originalValue: any): CoerceResult {
    if (value === null) {
        return { value: null };
    } else if (typeof value === "undefined" || value == "") {       // TODO: shall we support empty strings?
        return { value: null, wasCoerced: true };
    } else {
        return tryCoerce(value, innerType, originalValue);
    }
}    

function tryCoerceEnum(value: any, type: EnumTypeMetadata): CoerceResult {
    if (typeof value === "string" && value in type.values) {
        return { value };
    } else if (typeof value === "number") {
        const matched = keys(type.values).filter(k => type.values[k] === value);
        if (matched.length) {
            return { value: matched[0], wasCoerced: true }
        }
    }
    return new CoerceError(`Cannot cast '${value}' to type 'Enum(${keys(type.values).join(",")})'.`);
}

function tryCoerceArray(value: any, innerType: TypeDefinition, originalValue: any): CoerceResult {
    if (value === null) {
        return { value: null };
    } else if (typeof value === "undefined") {
        return { value: null, wasCoerced: true };
    } else if (Array.isArray(value)) {
        originalValue = Array.isArray(originalValue) ? originalValue : []

        let wasCoerced = false;
        const items = [];
        for (let i = 0; i < value.length; i++) {
            const item = withPathError("#" + i, () => tryCoerce(value[i], innerType, originalValue[i]))
            if (item.isError) {                
                return item
            }
            if (item.wasCoerced) {
                wasCoerced = true;
            }
            items.push(item.value);
        }
        if (!wasCoerced) {
            return { value };
        } else {
            return { value: items, wasCoerced: true };
        }
    }
    return new CoerceError(`Value '${value}' is not an array of type '${innerType}'.`);
}

function tryCoercePrimitiveType(value: any, type: string): CoerceResult {
    return primitiveTypes[type].tryCoerce(value) || CoerceError.generic(value, type);
}

function tryCoerceObject(value: any, type: string, typeInfo: ObjectTypeMetadata, originalValue: any): CoerceResult {
    if (value === null) {
        return { value: null };
    } else if (typeof value === "undefined") {
        return { value: null, wasCoerced: true };
    } else if (typeof value === "object") {
        if (!originalValue || originalValue.$type !== type) {
            // revalidate entire object when type is changed
            originalValue = {}
        }
        let wasCoerced = false;
        let patch: any = {};
        for (let k of keys(typeInfo.properties)) {
            if (k === "$type") {
                continue;
            }
            const result = withPathError(k, () => tryCoerce(value[k], typeInfo.properties[k].type, originalValue[k]))
            if (result.isError) {
                return result;
            }
            if (result.wasCoerced) {
                wasCoerced = true;
                patch[k] = result.value;
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
    }
    return new CoerceError(`Value ${value} was expected to be object`)
}

function tryCoerceDynamic(value: any, originalValue: any): CoerceResult {
    if (typeof value === "undefined") {
        return { value: null, wasCoerced: true };
    }

    if (Array.isArray(value)) {
        // coerce array items (treat them as dynamic)
        return tryCoerceArray(value, [{ type: "dynamic" }], originalValue);

    } else if (value && typeof value === "object") {
        let innerType = value["$type"];
        if (typeof innerType === "string") {
            // known object type - coerce recursively
            return tryCoerceObject(value, innerType, getObjectTypeInfo(innerType), originalValue);
        }

        // unknown object - treat every property as dynamic
        let wasCoerced = false;
        let patch: any = {};
        for (let k of keys(value)) {
            const result = withPathError(k, () => tryCoerceDynamic(value[k], originalValue && originalValue[k]))
            if (result.isError) {
                return result;
            }
            if (result.wasCoerced) {
                wasCoerced = true;
                patch[k] = result.value;
            }
        }
        if (!wasCoerced) {
            return { value };
        } else {
            return { value: { ...value, ...patch }, wasCoerced: true };
        }
    } 
    
    return { value };
}

function withPathError(path: string, f: () => CoerceResult): CoerceResult {
    const x = f()
    if (x.isError) {
        x.prependPathFragment(path)
    }
    return x
}
