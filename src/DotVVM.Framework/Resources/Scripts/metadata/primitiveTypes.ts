import { formatString, parseDate as globalizeParseDate } from "../DotVVM.Globalize";
import { parseDate as serializationParseDate, serializeDate, serializeTime } from "../serialization/date";
import { CoerceError } from "../shared-classes";

type PrimitiveTypes = { 
    [name: string]: { 
        tryCoerce: (value: any) => CoerceResult | undefined
    } 
};

export const primitiveTypes: PrimitiveTypes = {

    Boolean: {
        tryCoerce: (value: any) => {
            if (typeof value === "boolean") {
                return { value };
            } else if (value === "true" || value === "True") {
                return { value: true, wasCoerced: true };
            } else if (value === "false" || value === "False") {
                return { value: true, wasCoerced: true };
            } else if (typeof value === "number") {
                return { value: !!value, wasCoerced: true };
            }
        }
    },
    Byte: {
        tryCoerce: (value: any) => validateInt(value, 0, 255)
    },
    SByte: {
        tryCoerce: (value: any) => validateInt(value, -128, 127)
    },
    Int16: {
        tryCoerce: (value: any) => validateInt(value, -32768, 32767)
    },
    UInt16: {
        tryCoerce: (value: any) => validateInt(value, 0, 65535)
    },
    Int32: {
        tryCoerce: (value: any) => validateInt(value, -2147483648, 2147483647)
    },
    UInt32: {
        tryCoerce: (value: any) => validateInt(value, 0, 4294967295)
    },
    Int64: {
        tryCoerce: (value: any) => validateInt(value, -9223372036854775808, 9223372036854775807)
    },
    UInt64: {
        tryCoerce: (value: any) => validateInt(value, 0, 18446744073709551615)
    },
    Single: {
        tryCoerce: validateFloat
    },
    Double: {
        tryCoerce: validateFloat
    },
    Decimal: {
        tryCoerce: validateFloat
    },
    String: {
        tryCoerce: validateString
    },
    Char: {
        tryCoerce: validateChar
    },
    Guid: {
        tryCoerce: validateGuid
    },
    DateTime: {
        tryCoerce: validateDateTime
    },
    TimeSpan: {
        tryCoerce: validateDateTime
    }

};

function validateInt(value: any, min: number, max: number) {
    let wasCoerced = false;
    if (typeof value === "string") {
        if (value === "") {
            return;
        }
        value = Number(value);
        if (isNaN(value)) {
            // TODO: parse based on current culture
            return;
        }
        wasCoerced = true;
    } else if (typeof value !== "number") {
        return;
    }
    
    if (Math.trunc(value) !== value) {
        value = Math.trunc(value);
        wasCoerced = true;
    }
    
    if (!isNaN(value) && value >= min && value <= max) {
        return { value, wasCoerced };
    }
}

function validateFloat(value: any) {
    let wasCoerced = false;
    if (typeof value === "string") {
        value = Number(value);
        if (isNaN(value)) {
            // TODO: parse based on current culture
            return;
        }
        wasCoerced = true;
    } else if (typeof value !== "number") {
        return;
    }
        
    if (!isNaN(value)) {
        return { value, wasCoerced };
    }
}

function validateString(value: any) {
    let wasCoerced = false;
    if (value === null) {
        wasCoerced = false;
    } else if (typeof value === "number") {
        value = formatString("n", value);
        wasCoerced = true;
    } else if (value instanceof Date) {
        value = formatString("g", value);
        wasCoerced = true;
    } else if (typeof value === "boolean") {
        value = value ? "true" : "false";
        wasCoerced = true;
    } else if (typeof value === "undefined") {
        value = null;
        wasCoerced = true;
    } else if (typeof value !== "string") {
        return;
    }

    return { value, wasCoerced };
}

function validateChar(value: any) {
    if (typeof value === "number" && (value | 0) === value && value >= 0 && value <= 65535) {
        return { value: String.fromCharCode(value), wasCoerced: true };
    } else if (typeof value !== "string") {
        return;
    }

    if (value.length === 1) {
        return { value };
    } else if (value.length > 1) {
        return { value: value.substring(0, 1), wasCoerced: true };
    }
}

function validateGuid(value: any) {
    if (typeof value !== "string") {
        return;
    }

    if (/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/.test(value)) {
        return { value };
    }
}

function validateDateTime(value: any) {
    if (typeof value === "string") {
        // strict DotVVM format parse
        if (serializationParseDate(value)) {
            return { value };
        } 
        
        // loose parse (the format parameter is intentionally blank - let Globalize.js use default formats from current culture)
        value = globalizeParseDate(value, "");
    }
    
    if (value instanceof Date) {
        return { value: serializeDate(value, false), wasCoerced: true };
    }
}
