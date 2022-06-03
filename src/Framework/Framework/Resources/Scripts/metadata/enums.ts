import { CoerceError } from "../shared-classes"
import { keys } from "../utils/objects"

export function enumStringToInt(value: number | string, type: EnumTypeMetadata): number | null {
    if (!isNaN(+value)) {
        return +value
    }

    if (value in type.values) {
        return type.values[value]
    }

    if (type.isFlags) {
        // flags - comma-separated values
        const parts = (value as string).split(',')
        let result = 0

        for (const fragment of parts) {
            // trim the value if needed
            const trimmed = fragment.trim()
            if (trimmed in type.values) {
                result |= type.values[trimmed]
            }
            else {
                return null
            }
        }
        return result
    }
    return null
}

export function enumIntToString(value: number, type: EnumTypeMetadata): string | null {
    value |= 0

    const matched = keys(type.values).filter(k => type.values[k] === value)
    if (matched.length) {
        return matched[0]
    }

    if (type.isFlags) {
        // try to represent the enum with comma-separated strings
        if (value) {
            let result: number = value
            let stringValue = ""
            for (const k of keys(type.values).reverse()) {
                if (type.values[k] !== 0 && (result & type.values[k]) === type.values[k]) {
                    result -= type.values[k]
                    if (stringValue !== "") stringValue = "," + stringValue
                    stringValue = k + stringValue
                }
            }
            if (!result) {
                return stringValue
            }
        }
    }

    return null
}


export function tryCoerceEnum(value: any, type: EnumTypeMetadata): CoerceResult {
    if (value in type.values) {
        return { value }
    }

    const intValue = enumStringToInt(value, type)
    if (intValue != null) {
        const stringValue = enumIntToString(intValue, type)
        if (stringValue != null) {
            const wasCoerced = stringValue != value
            return { value: stringValue, wasCoerced }
        }
    }

    return new CoerceError(`Cannot cast '${value}' to type 'Enum(${keys(type.values).join(",")})'.`)
}
