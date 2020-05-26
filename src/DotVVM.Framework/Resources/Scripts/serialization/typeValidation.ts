
export function validateType(value: any, type: string) {
    const nullable = type[type.length - 1] == "?";
    if (nullable) {
        type = type.substr(0, type.length - 1);
    }
    if (nullable && (value == null || value == "")) {
        return true;
    }
    if (!nullable && (value == null)) {
        return false;
    }

    const intmatch = /(u?)int(\d*)/.exec(type);
    if (intmatch) {
        if (!/^-?\d*$/.test(value)) {
            return false;
        }

        const unsigned = intmatch[1] === "u";
        const bits = parseInt(intmatch[2], 10);
        let minValue = 0;
        let maxValue = Math.pow(2, bits) - 1;
        if (!unsigned) {
            minValue = -Math.floor(maxValue / 2);
            maxValue = maxValue + minValue;
        }
        const int = parseInt(value, 10);
        return int >= minValue && int <= maxValue && int === parseFloat(value);
    }
    if (type == "number" || type == "single" || type == "double" || type == "decimal") {
        // should check if the value is numeric or number in a string
        return +value === value || (!isNaN(+value) && typeof value == "string");
    }
    return true;
}
