import { logWarning } from "../utils/logging";

export function parseDate(value: string): Date | null {
    const match = value.match("^([0-9]{4})-([0-9]{2})-([0-9]{2})T([0-9]{2}):([0-9]{2}):([0-9]{2})(\\.[0-9]{3,7})$");
    if (match) {
        return new Date(parseInt(match[1], 10), parseInt(match[2], 10) - 1, parseInt(match[3], 10),
            parseInt(match[4], 10), parseInt(match[5], 10), parseInt(match[6], 10), match.length > 7 ? parseInt(match[7].substring(1, 4), 10) : 0);
    }
    return null;
}

export function parseDateTimeOffset(value: string): Date | null {
    const match = value.match("^([0-9]{4})-([0-9]{2})-([0-9]{2})T([0-9]{2}):([0-9]{2}):([0-9]{2})(\\.[0-9]{1,7})?(Z|[+-]([0-9]{1,2}):([0-9]{2}))$");
    if (match) {
        const offset = match[8] === "Z" ? 0 : ((match[8] === "-" ? -1 : 1) * (parseInt(match[9], 10) * 60 + parseInt(match[10], 10)));
        return new Date(parseInt(match[1], 10), parseInt(match[2], 10) - 1, parseInt(match[3], 10),
            parseInt(match[4], 10), parseInt(match[5], 10) + offset, parseInt(match[6], 10), match[7] ? parseInt(match[7].substring(1, 4), 10) : 0);
    }
    return null;
}

function padNumber(value: string | number, digits: number): string {
    value = value + ""
    while (value.length < digits) {
        value = "0" + value;
    }
    return value;
}

export function serializeDate(date: string | Date | null, convertToUtc: boolean = true): string | null {
    if (date == null) {
        return null;
    } else if (typeof date == "string") {
        // just print in the console if it's invalid
        if (parseDate(date) == null) {
            logWarning("coercer", `Date ${date} is invalid.`);
        }
        return date;
    }
    let date2 = new Date(date.getTime());
    if (convertToUtc) {
        date2.setMinutes(date.getMinutes() + date.getTimezoneOffset());
    } else {
        date2 = date;
    }

    const y = padNumber(date2.getFullYear(), 4);
    const m = padNumber((date2.getMonth() + 1), 2);
    const d = padNumber(date2.getDate(), 2);
    const h = padNumber(date2.getHours(), 2);
    const mi = padNumber(date2.getMinutes(), 2);
    const s = padNumber(date2.getSeconds(), 2);
    const ms = padNumber(date2.getMilliseconds(), 3);
    return `${y}-${m}-${d}T${h}:${mi}:${s}.${ms}0000`;
}

export function serializeTime(date: string | Date | null, convertToUtc: boolean = true): string | null {
    if (date == null) {
        return null;
    } else if (typeof date == "string") {
        // just print in the console if it's invalid
        if (parseDate(date) == null) {
            logWarning("coercer", `Date ${date} is invalid.`);
        }
        return date;
    }
    let date2 = new Date(date.getTime());
    if (convertToUtc) {
        date2.setMinutes(date.getMinutes() + date.getTimezoneOffset());
    } else {
        date2 = date;
    }

    const h = (date2.getTime() - new Date(1, 0, 1).getTime() / 1000 / 3600) | 0;
    const mi = padNumber(date2.getMinutes(), 2);
    const s = padNumber(date2.getSeconds(), 2);
    const ms = padNumber(date2.getMilliseconds(), 3);
    return `${h}:${mi}:${s}.${ms}0000`;
}
