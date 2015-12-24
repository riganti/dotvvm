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

        if (format === "g") {
            return this.formatString("d", value) + " " + this.formatString("t", value);
        } else if (format === "G") {
            return this.formatString("d", value) + " " + this.formatString("T", value);
        }

        if (typeof value === "string" && value.match("^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(\\.[0-9]{1,7})?$")) {
            // JSON date in string
            value = new Date(value);
        }
        return Globalize.format(value, format, dotvvm.culture);
    }

}