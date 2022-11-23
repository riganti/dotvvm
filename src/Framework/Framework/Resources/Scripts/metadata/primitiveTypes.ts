import {
    parseDate as serializationParseDate,
    parseDateOnly as serializationParseDateOnly,
    parseTimeOnly as serializationParseTimeOnly,
    parseTimeSpan as serializationParseTimeSpan,
    parseDateTimeOffset as serializationParseDateTimeOffset,
    serializeDate,
    serializeDateOnly,
    serializeTimeOnly,
    serializeTimeSpan
} from "../serialization/date";
import { isNumber } from "../utils/isNumber";

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
    DateOnly: {
        tryCoerce: validateDateOnly
    },
    TimeOnly: {
        tryCoerce: validateTimeOnly
    },
    TimeSpan: {
        tryCoerce: validateTimeSpan
    },
    DateTimeOffset: {
        tryCoerce: validateDateTimeOffset
    }
};

function validateInt(value: any, min: number, max: number) {
    const originalValue = value
    if (!isNumber(value)) {
        return
    }
    value = Number(value)
    value = Math.trunc(value)
    
    if (value >= min && value <= max) {
        return { value, wasCoerced: value !== originalValue };
    }
}

function validateFloat(value: any) {
    if (isNumber(value)) {
        return { value: +value, wasCoerced: value !== +value };
    }
}

function validateString(value: any) {
    let wasCoerced = false;
    if (value === null) {
        wasCoerced = false;
    } else if (typeof value === "number") {
        value = value.toString();
        wasCoerced = true;
    } else if (value instanceof Date) {
        value = serializeDate(value);
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
    }
    
    if (value instanceof Date) {
        return { value: serializeDate(value, false), wasCoerced: true };
    }
}

function validateDateOnly(value: any) {
    if (typeof value === "string") {
        // strict DotVVM format parse
        const dateOnly = serializationParseDateOnly(value);
        if (dateOnly != null) {
            return { value: value, wasCoerced: false };
        }

        // less-strict DotVVM format parse (coercion from DateTime)
        const dateTime = serializationParseDate(value);
        if (dateTime != null) {
            // try to coerce DateTime to DateOnly
            const coercedDateOnly = serializeDateOnly(dateTime);
            if (coercedDateOnly != null) {
                return { value: coercedDateOnly, wasCoerced: true };
            }
        }
    }

    if (value instanceof Date) {
        return { value: serializeDateOnly(value), wasCoerced: true };
    }
}

function validateTimeOnly(value: any) {
    if (typeof value === "string") {
        // strict DotVVM format parse
        const timeOnly = serializationParseTimeOnly(value);
        if (timeOnly != null) {
            return { value: value, wasCoerced: false };
        }

        // less-strict DotVVM format parse (coercion from DateTime)
        const dateTime = serializationParseDate(value);
        if (dateTime != null) {
            // try to coerce DateTime toTimeOnly
            const coercedTimeOnly = serializeTimeOnly(dateTime);
            if (coercedTimeOnly != null) {
                return { value: coercedTimeOnly, wasCoerced: true }
            }
        }
    }

    if (value instanceof Date) {
        return { value: serializeTimeOnly(value), wasCoerced: true };
    }
}

function validateTimeSpan(value: any) {
    if (typeof value === "string") {
        // strict DotVVM format parse
        const parsedValue = serializationParseTimeSpan(value);
        if (parsedValue != null) {
            return { value: serializeTimeSpan(parsedValue) };
        }
    }
    
    if (typeof value === "number") {
        return { value: serializeTimeSpan(value), wasCoerced: true };
    }
}

function validateDateTimeOffset(value: any) {
    if (typeof value === "string") {
        // strict DotVVM format parse
        if (serializationParseDateTimeOffset(value)) {
            return { value };
        }        
    }
    
    // TODO: support conversion to date
    // if (value instanceof Date) {
    //     return { value: serializeDate(value, false), wasCoerced: true };
    // }
}
