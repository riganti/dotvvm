import { logWarning } from "../utils/logging";

export function parseDate(value: string | null, convertFromUtc: boolean = false): Date | null {
    if (value == null) return null;
    const match = value.match("^([0-9]{4})-([0-9]{2})-([0-9]{2})T([0-9]{2}):([0-9]{2}):([0-9]{2})(\\.[0-9]{1,7})$");
    if (match) {
        const date = new Date(0);
        let month = parseInt(match[2], 10) - 1;
        let day = parseInt(match[3], 10);

         //Javascript date object does not support month 00 and rolls to december.
        //In case of user input of 00 we correct it to january.
        //This is more user friendly than suddendly having december.
        month = month < 0 ? 0 : month;

        //Javascript date object does not support day 0 and rolls to 30 or 31 and rolls the month value to previous month.
        //This results in unpredictable behaviour if user inputs 00 into date input field for instance
        //We sanitize it to 1st of the same month to avoid this unpredictability
        day = day < 1 ? 1 : day;

        //We set components of the date by hand, this prevents JS from 'corecting' years 00XX to 19XX
        date.setMilliseconds(match.length > 7 ? parseInt(match[7].substring(1, 4), 10) : 0);
        date.setSeconds(parseInt(match[6], 10));
        date.setMinutes(parseInt(match[5], 10));
        date.setHours(parseInt(match[4], 10));
        date.setDate(day);
        date.setMonth(month);
        date.setFullYear(parseInt(match[1], 10));

        if (convertFromUtc) {
            date.setMinutes(date.getMinutes() - date.getTimezoneOffset());
        }
        return date;
    }
    return null;
}

export function parseTimeSpan(value: string | null): number | null {
    if (value == null) return null;
    const match = value.match("^(-?)([0-9]+\\.)?([0-9]+):([0-9]{2}):([0-9]{2})(\\.[0-9]{3,7})?$");
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
