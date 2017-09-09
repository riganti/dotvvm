class DotvvmGlobalize {

    public format(format: string, ...values: string[]) {
        return format.replace(/\{([1-9]?[0-9]+)(:[^}])?\}/g, (match, group0, group1) => {
            var value = values[parseInt(group0)];
            if (group1) {
                return this.formatString(group1, value);
            } else {
                return value;
            }
        });
    }

    public formatString(format: string, value: any) {
        value = ko.unwrap(value);
        if (value == null) return "";

        if (typeof value === "string") {
            // JSON date in string
            value = this.parseDotvvmDate(value);
        }

        if (format === "" || format === null) {
            format = "G";
        }

        return dotvvm_Globalize.format(value, format, dotvvm.culture);
    }

    public parseDotvvmDate(value: string): Date | null {
        var match = value.match("^([0-9]{4})-([0-9]{2})-([0-9]{2})T([0-9]{2}):([0-9]{2}):([0-9]{2})(\\.[0-9]{3,7})$");
        if (match) {
            return new Date(parseInt(match[1]), parseInt(match[2]) - 1, parseInt(match[3]),
                parseInt(match[4]), parseInt(match[5]), parseInt(match[6]), match.length > 7 ? parseInt(match[7].substring(1, 4)) : 0);
        }
        return null;
    }

    public parseNumber(value: string): number {
        return dotvvm_Globalize.parseFloat(value, 10, dotvvm.culture);
    }

    public parseDate(value: string, format: string, previousValue?: Date) {
        return dotvvm_Globalize.parseDate(value, format, dotvvm.culture, previousValue);
    }

    public bindingDateToString(value: KnockoutObservable<string | Date | null> | string | Date | null, format: string = "G") {
        const unwrapedVal = ko.isObservable(value) ? value.peek() : value;
        const getDate = (v = unwrapedVal) => typeof v == "string" ? this.parseDotvvmDate(v) : v;
        if (ko.isWriteableObservable(value)) {
            const setter = typeof unwrapedVal == "string" ? v => value(v && dotvvm.serialization.serializeDate(v, false)) : value
            let lastInvalidValue: KnockoutObservable<string | null> = value["lastInvalidDate"] = (<any>value["lastInvalidDate"] || ko.observable(null))
            return ko.pureComputed({
                read: () => {
                    const date = getDate(value())
                    const fallbackValue = lastInvalidValue()
                    return (date && dotvvm_Globalize.format(date, format, dotvvm.culture)) || fallbackValue;
                },
                write: val => {
                    const parsed = val && dotvvm_Globalize.parseDate(val, format, dotvvm.culture)
                    lastInvalidValue(parsed == null ? val : null)
                    setter(parsed)
                }
            });
        }
        else {
            const date = getDate()
            if (date == null) return "";
            return dotvvm_Globalize.format(date, format, dotvvm.culture);
        }
    }

    public bindingNumberToString(value: KnockoutObservable<string | number> | string | number, format: string = "G") {
        const unwrapedVal = ko.isObservable(value) ? value.peek() : value;
        const getNum = (v = unwrapedVal) => typeof unwrapedVal == "string" ? this.parseNumber(unwrapedVal) : unwrapedVal;
        if (ko.isWriteableObservable(value)) {
            let lastInvalidValue: KnockoutObservable<string | null> = value["lastInvalidDate"] = (<any>value["lastInvalidDate"] || ko.observable(null))
            return ko.pureComputed({
                read: () => {
                    const num = getNum(value())
                    const fallbackValue = lastInvalidValue()
                    return (num && dotvvm_Globalize.format(num, format, dotvvm.culture)) || fallbackValue;
                },
                write: val => {
                    const parsed = val && dotvvm_Globalize.parseFloat(val, 10, dotvvm.culture)
                    lastInvalidValue(parsed == null ? val : null)
                    value(parsed)
                }
            });
        }
        else {
            if (isNaN(getNum())) return "";
            return dotvvm_Globalize.format(getNum(), format, dotvvm.culture);
        }
    }

}