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

    public parseDotvvmDate(value: string): Date {
        var match = value.match("^([0-9]{4})-([0-9]{2})-([0-9]{2})T([0-9]{2}):([0-9]{2}):([0-9]{2})(\\.[0-9]{3,7})$");
        if (match) {
            return new Date(parseInt(match[1]), parseInt(match[2]) - 1, parseInt(match[3]),
                parseInt(match[4]), parseInt(match[5]), parseInt(match[6]), match.length > 7 ? parseInt(match[7].substring(1, 4)) : 0);
        }
        return null;
    }

    public parseNumber(value: string): number {
        return dotvvm_Globalize.parseFloat(value,10, dotvvm.culture);
    }

    public parseDate(value: string, format: string) {
        return dotvvm_Globalize.parseDate(value, format, dotvvm.culture);
    }
}