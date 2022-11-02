import { logWarning } from "../utils/logging";

export function parseDate(value: string | null, convertFromUtc: boolean = false): Date | null {
    if (value == null) return null;
    if (/^\d{4}-\d\d-\d\dT\d\d:\d\d:\d\d(\.\d{1,7})?$/.test(value)) {

        // for some reason, we want to support date with 00 everywhere,
        // so this hack sanitizes the date by setting day and month fields to 1
        const sanitizedValue = value.replace(/00T/, "01T").replace(/00-(\d\d)T/, "01-$1T")
        const d = Date.parse(sanitizedValue + (convertFromUtc ? "Z" : ""));
        if (isNaN(d)) {
            return null
        } else {
            return new Date(d)
        }
    }
    return null;
}

export function parseDateOnly(value: string | null): Date | null {
    return parseDate(`${value}T00:00:00.00`, false);
}

export function parseTimeOnly(value: string | null): Date | null {
    return parseDate(`1970-01-01T${value}`, false);
}

export function parseTimeSpan(value: string | null): number | null {
    if (value == null) return null;
    const match = /^(-?)(\d+\.)?(\d+):(\d\d):(\d\d)(\.\d{3,7})?$/.exec(value);
    if (match) {
        const sign = match[1] ? -1 : 1;
        const days = match[2] ? parseInt(match[2], 10) : 0;
        const ticks = (days * 24 + parseInt(match[3], 10)) * 3600 * 1000 +
            parseInt(match[4], 10) * 60 * 1000 +
            parseInt(match[5], 10) * 1000 + 
            (match[6] ? parseInt(match[6].substring(1, 4), 10) : 0);
        return sign * ticks;
    }
    return null;
}

export function parseDateTimeOffset(value: string | null): Date | null {
    if (value == null) return null;
    const d = Date.parse(value)
    if (d) {
        return new Date(d)
    }
    return null;
}

function padNumber(value: string | number, digits: number): string {
    return (value + "").padStart(digits, "0");
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
    return date
        .toLocaleString( 'sv', { timeZone: convertToUtc ? 'UTC' : undefined } )
        .replace(' ', 'T')
        + '.' + padNumber(date.getMilliseconds(), 3) + '0000';
}

export function serializeDateOnly(date: Date | null): string | null {
    if (date == null) {
        return null;
    }
    // https://stackoverflow.com/a/58633651/3577667
    // Note that I'm using Sweden as locale because it is one of the countries that uses ISO 8601 format.
    return date.toLocaleDateString('sv');
}

export function serializeTimeOnly(date: Date | null): string | null {
    if (date == null) {
        return null;
    }

    return date.toLocaleTimeString('sv') + '.' + padNumber(date.getMilliseconds(), 3) + '0000';
}

export function serializeTimeSpan(time: string | number | null): string | null {
    let ticks: number;

    if (time === null) {
        return null;
    } else if (typeof time == "string") {
        // just print in the console if it's invalid
        const parsedTime = parseTimeSpan(time);
        if (parsedTime === null) {
            logWarning("coercer", `TimeSpan ${time} is invalid.`);
            return null;
        }

        ticks = parsedTime;
    } else {
        ticks = time;
    }
    
    const sign = ticks >= 0 ? "" : "-";
    ticks = Math.abs(ticks);
    const hours = (ticks / 1000 / 3600) | 0;
    const minutes = (ticks / 1000 / 60) | 0;
    const seconds = (ticks / 1000) | 0;
    const milliseconds = (ticks % 1000) | 0;

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
