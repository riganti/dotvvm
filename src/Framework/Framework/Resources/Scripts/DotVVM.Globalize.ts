import {
    parseDate as serializationParseDate,
    parseDateOnly as serializationParseDateOnly,
    parseTimeOnly as serializationParseTimeOnly,
    serializeDate,
    serializeDateOnly,
    serializeTimeOnly
} from './serialization/date'
import { getCulture } from './dotvvm-base';

function getGlobalize(): GlobalizeStatic {
    const g = (window as any)["dotvvm_Globalize"]
    if (compileConstants.debug && !g) {
        throw new Error("Resource 'globalize' is not included (symbol 'dotvvm_Globalize' could not be found).\nIt is usually included automatically when needed, but sometime it's not possible, so you will have to include it in your page using '<dot:RequiredResource Name=\"globalize\" />'")
    }
    return g;
}

export function format(format: string, ...values: any[]): string {
    return format.replace(/\{([1-9]?[0-9]+)(:[^}]+)?\}/g, (match, group0, group1) => {
        const value = values[parseInt(group0, 10)];
        if (group1) {
            group1 = group1.substring(1);
            return formatString(group1, value, null);
        } else {
            return value;
        }
    });
}

type GlobalizeFormattable = null | undefined | string | Date | number

export function formatString(format: string | null | undefined, value: GlobalizeFormattable | KnockoutObservable<GlobalizeFormattable>, type: string | null) {
    value = ko.unwrap(value);
    if (value == null || value === "") {
        return "";
    }

    if (typeof value === "string") {

        // DateTime, DateOnly or TimeOnly
        if (type === "dateonly") {
            value = serializationParseDateOnly(value);
            if (format == null || format.length == 0) {
                format = "D";
            }
        } else if (type == "timeonly") {
            value = serializationParseTimeOnly(value);
            if (format == null || format.length == 0) {
                format = "T";
            }
        } else {
            value = serializationParseDate(value);
        }

        if (value == null) {
            throw new Error(`Could not parse ${value} as a date`);
        }
    }

    if (format == null || format.length == 0) {
        format = "G";
    }

    return getGlobalize().format(value, format, getCulture());
}

export function parseNumber(value: string): number {
    return getGlobalize().parseFloat(value, 10, getCulture());
}

export function parseDate(value: string, format: string, previousValue?: Date | null) {
    return getGlobalize().parseDate(value, format, getCulture(), previousValue);
}

export const parseDotvvmDate = serializationParseDate;

export function bindingDateToString(value: GlobalizeFormattable | KnockoutObservable<GlobalizeFormattable>, format: string = "G") {
    return bindingDateLikeTypeToString(value, format, "datetime", serializeDate, serializationParseDate);
}

export function bindingDateOnlyToString(value: GlobalizeFormattable | KnockoutObservable<GlobalizeFormattable>, format: string = "D") {
    return bindingDateLikeTypeToString(value, format, "dateonly", serializeDateOnly, serializationParseDateOnly);
}

export function bindingTimeOnlyToString(value: GlobalizeFormattable | KnockoutObservable<GlobalizeFormattable>, format: string = "T") {
    return bindingDateLikeTypeToString(value, format, "timeonly", serializeTimeOnly, serializationParseTimeOnly);
}

function bindingDateLikeTypeToString(value: GlobalizeFormattable | KnockoutObservable<GlobalizeFormattable>, format: string, type: string, serializer: Function, parser: Function) {
    const unwrapDate = () => {
        const unwrappedVal = ko.unwrap(value);
        return typeof unwrappedVal == "string" ? parser(unwrappedVal) : unwrappedVal;
    };

    const formatDate = () => formatString(format, value, type);

    if (ko.isWriteableObservable(value)) {
        const unwrappedVal = unwrapDate();
        const setter = typeof unwrappedVal == "string" ? (v: Date | null) => {
            return value(v && serializer(v, false));
        } : value;
        return ko.pureComputed({
            read: formatDate,
            write: val => setter(parseDate(val, format) || parseDate(val, ""))
        });
    }
    else {
        return ko.pureComputed(formatDate);
    }
}

export function bindingNumberToString(value: GlobalizeFormattable | KnockoutObservable<GlobalizeFormattable>, format: string = "G") {
    const formatNumber = () => formatString(format, value, null);

    if (ko.isWriteableObservable(value)) {
        return ko.pureComputed({
            read: formatNumber,
            write: val => {
                const parsedFloat = parseNumber(val)
                const isValid = val == null || (parsedFloat != null && !isNaN(parsedFloat))

                value(isValid ? parsedFloat : null)
            }
        });
    }
    else {
        return ko.pureComputed(formatNumber);
    }
}
