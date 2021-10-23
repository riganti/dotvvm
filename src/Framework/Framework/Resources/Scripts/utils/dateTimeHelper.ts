import { parseDate, serializeDate } from "../serialization/date";

export function toBrowserLocalTime(value: KnockoutObservable<string | null> | null) : KnockoutComputed<string | null> | null {
    if (value == null) return null;

    const convert = () => {
        const unwrappedValue = ko.unwrap(value);
        return serializeDate(parseDate(unwrappedValue, true), false);
    };
    const convertBack = (newVal: string | null) => {
        const result = serializeDate(parseDate(newVal, false), true);
        value(result);
    }

    if (ko.isWriteableObservable(value)) {
        return ko.pureComputed({
            read: convert,
            write: convertBack
        });
    }
    else {
        return ko.pureComputed(convert);
    }
}
