import { logWarning } from "../utils/logging";

const timeSpanBaseDate = new Date("0001-01-01 00:00:00.0000");

export function parseDate(value: string): Date | null {
    const match = value.match("^([0-9]{4})-([0-9]{2})-([0-9]{2})T([0-9]{2}):([0-9]{2}):([0-9]{2})(\\.[0-9]{1,7})$");
    if (match) {
        return new Date(parseInt(match[1], 10), parseInt(match[2], 10) - 1, parseInt(match[3], 10),
            parseInt(match[4], 10), parseInt(match[5], 10), parseInt(match[6], 10), match.length > 7 ? parseInt(match[7].substring(1, 4), 10) : 0);
    }
    return null;
}

export function parseTime(value: string): Date | null {
    const match = value.match("^(-?)([0-9]+\\.)?([0-9]+):([0-9]{2}):([0-9]{2})(\\.[0-9]{3,7})?$");
    if (match) {
        const sign = match[1] ? -1 : 1;
        const days = match[2] ? parseInt(match[2], 10) : 0;
        const ticks = (days * 24 + parseInt(match[3], 10)) * 3600 * 1000 +
            parseInt(match[4], 10) * 60 * 1000 +
            parseInt(match[5], 10) * 1000 + 
            (match[6] ? parseInt(match[6].substring(1, 4), 10) : 0);
        return new Date(timeSpanBaseDate.getTime() + sign * ticks);
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
        if (parseTime(date) == null) {
            logWarning("coercer", `TimeSpan ${date} is invalid.`);
        }
        return date;
    }
    let date2 = new Date(date.getTime());
    if (convertToUtc) {
        date2.setMinutes(date.getMinutes() + date.getTimezoneOffset());
    } else {
        date2 = date;
    }

    const ticks = Math.abs(date2.getTime() - timeSpanBaseDate.getTime()) | 0;
    const hours = (ticks / 1000 / 3600) | 0;
    const minutes = (ticks / 1000 / 60) | 0;
    const seconds = (ticks / 1000) | 0;
    const milliseconds = (ticks % 1000) | 0;

    const sign = date2.getTime() >= timeSpanBaseDate.getTime() ? "" : "-";
    const h = padNumber(hours % 24, 2);
    const mi = padNumber(minutes % 60, 2);
    const s = padNumber(seconds % 60, 2);
    const ms = milliseconds !== 0 ? ("." + padNumber(milliseconds, 3) + "0000") : "";

    if (hours < 24) {
        return `${sign}${h}:${mi}:${s}${ms}`;
    } else {
        const d = (hours / 24) | 0;
        return `${sign}${d}.${h}:${mi}:${s}${ms}`;
    }
}
