import { serializeDate } from "../serialization/date";
import { jsonStringify } from "../serialization/serialize";
import { CoerceError } from "../shared-classes";
import { keys } from "../utils/objects";
import { tryCoerceEnum } from "./enums";
import { primitiveTypes } from "./primitiveTypes";
import { formatTypeName, getCurrentTypeMap, getObjectTypeInfo, getTypeInfo } from "./typeMap";

let validatedValues = new WeakMap<object, Set<string>>();
let validatedTypeMap: TypeMap | undefined;

/**
 * Validates type of value
 * @param type Expected type of type value.
 * @param originalValue Value that is known to be valid instance of type. It is used to perform incremental validation.
 */
export function tryCoerce(value: any, type: TypeDefinition | null | undefined, originalValue: any = undefined): CoerceResult {
    if (validatedTypeMap !== getCurrentTypeMap()) {
        validatedTypeMap = getCurrentTypeMap();
        validatedValues = new WeakMap();
    }
    const contract = JSON.stringify(type ?? null);

    function core() {
        if (originalValue === value && value && typeof value === "object" && validatedValues.get(value)?.has(contract)) {
            return { value }
        }

        if (typeof type === "object" && !Array.isArray(type) && type?.type === "nullable") {
            return tryCoerceNullable(value, type.inner, originalValue);
        }

        const expectedInfo = typeof type === "string" ? getCurrentTypeMap()[type] : undefined;
        if (value != null && expectedInfo?.type === "object" && expectedInfo.derivedTypes !== undefined && (typeof value !== "object" || Array.isArray(value))) {
            return CoerceError.generic(value, type!);
        }
        if (value != null && expectedInfo?.type === "object" && expectedInfo.derivedTypes !== undefined && expectedInfo.isAbstract && (value.$type == null || value.$type === type)) {
            return new CoerceError(`An explicit concrete '$type' is required for '${formatTypeName(type!)}'.`);
        }
        if (value) {
            if (expectedInfo?.type === "object" && expectedInfo.derivedTypes !== undefined && value.$type != null && value.$type !== type) {
                const actualInfo = getCurrentTypeMap()[value.$type];
                if (!expectedInfo.derivedTypes.includes(value.$type) || actualInfo?.type !== "object") {
                    return new CoerceError(`Type '${value.$type}' is not assignable to '${formatTypeName(type!)}'.`);
                }
            }
            type = value.$type ?? type
        }

        if (type == null || (type as any).type == "dynamic") {
            return tryCoerceDynamic(value, originalValue)
        }
        else if (Array.isArray(type)) {
            return tryCoerceArray(value, type[0], originalValue);
        } else if (typeof type === "object") {
            if (type.type === "nullable") {
                return tryCoerceNullable(value, type.inner, originalValue);
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
        return new CoerceError(`Unsupported type metadata ${formatTypeName(type)}!`);
    }

    const result = core();
    if (result instanceof CoerceError) {
        return result;      // we cannot freeze CoerceError because we modify its path property
    }
    Object.freeze(result.value)
    if (result.value && typeof result.value === "object") {
        let contracts = validatedValues.get(result.value);
        if (!contracts) {
            validatedValues.set(result.value, contracts = new Set());
        }
        contracts.add(contract);
    }
    return result
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
    } else if (typeof value === "undefined" || value === "") {
        return { value: null, wasCoerced: true };
    } else {
        return tryCoerce(value, innerType, originalValue);
    }
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
    return new CoerceError(`Value '${jsonStringify(value)}' is not an array of type '${formatTypeName(innerType)}'.`);
}

function tryCoercePrimitiveType(value: any, type: string): CoerceResult {
    return primitiveTypes[type].tryCoerce(value) || CoerceError.generic(value, type);
}

function tryCoerceObject(value: any, type: string, typeInfo: ObjectTypeMetadata, originalValue: any): CoerceResult {
    if (value === null) {
        return { value: null };
    } else if (typeof value === "undefined") {
        return { value: null, wasCoerced: true };
    } else if (typeInfo?.type === "object") {
        const previousTypeInfo = typeof originalValue?.$type === "string" && originalValue.$type !== type
            ? getCurrentTypeMap()[originalValue.$type] : undefined;
        const removedProperties = previousTypeInfo?.type === "object"
            ? keys(previousTypeInfo.properties).filter(k => k !== "$type" && !(k in typeInfo.properties) && k in value) : [];
        if (!originalValue || originalValue.$type !== type) {
            // revalidate entire object when type is changed
            originalValue = {}
        }
        let wasCoerced = removedProperties.length > 0;
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
            const result = { ...value, ...patch };
            for (const prop of removedProperties) {
                delete result[prop];
            }
            return { value: result, wasCoerced: true };
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
        return tryCoerceArray(value, { type: "dynamic" }, originalValue);
    } else if (value instanceof Date) {
        value = serializeDate(value, false)
    } else if (value && typeof value === "object") {
        const innerType = value["$type"];
        if (typeof innerType === "string") {
            // known object type - coerce recursively
            const innerTypeInfo = getObjectTypeInfo(innerType);
            if (innerTypeInfo.type === "object") {
                return tryCoerceObject(value, innerType, innerTypeInfo, originalValue);
            }
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
        if (wasCoerced) {
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
