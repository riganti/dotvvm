export function parseDate(value: string): Date | null {
    var match = value.match("^([0-9]{4})-([0-9]{2})-([0-9]{2})T([0-9]{2}):([0-9]{2}):([0-9]{2})(\\.[0-9]{3,7})$");
    if (match) {
        return new Date(parseInt(match[1]), parseInt(match[2]) - 1, parseInt(match[3]),
            parseInt(match[4]), parseInt(match[5]), parseInt(match[6]), match.length > 7 ? parseInt(match[7].substring(1, 4)) : 0);
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
        if (parseDate(date) == null)
            console.error(new Error(`Date ${date} is invalid.`));
        return date;
    }
    var date2 = new Date(date.getTime());
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
