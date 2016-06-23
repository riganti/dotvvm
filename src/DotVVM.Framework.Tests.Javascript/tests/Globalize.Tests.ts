describe("Globalize.js G number format", () => {

    it("Zero", () => {
        expect(Globalize.format(10, "G", "en-US")).toBe("10");
    });

    it("Decimal number", () => {
        expect(Globalize.format(10.123, "G", "en-US")).toBe("10.123");
    });

    it("Negative number", () => {
        expect(Globalize.format(-25.000, "G", "en-US")).toBe("-25");
    });

    it("Large number", () => {
        expect(Globalize.format(123456789.1234567890123456789, "G", "en-US")).toBe("123456789.12345680");
    });

});

describe("Globalize.js custom number format", () => {

    it("Decimal number 1 round", () => {
        expect(Globalize.format(10.125, "#.0#", "en-US")).toBe("10.13");
    });

    it("Decimal number 2 make it decimal", () => {
        expect(Globalize.format(10, "#.0#", "en-US")).toBe("10.0");
    });

    it("Decimal number 3 add zeroes and round", () => {
        expect(Globalize.format(1.255, "0#.0#", "en-US")).toBe("01.26");
    });

    it("Decimal Number 4 add zeroes", () => {
        expect(Globalize.format(123.5, "0000.00", "en-US")).toBe("0123.50");
    });

    it("Decimal number 5 round", () => {
        expect(Globalize.format(123.456, "###0.00", "en-US")).toBe("123.46");
    });
    
    it("Decimal Number 6 thousands separator", () => {
        expect(Globalize.format(123.5, "0,000.00", "en-US")).toBe("0,123.50");
    });

    it("Decimal number 7 thousands separator", () => {
        expect(Globalize.format(123.456, "#,##0.00", "en-US")).toBe("123.46");
    });

    it("Decimal Number 8 thousands separator", () => {
        expect(Globalize.format(123456789.98765, "#,###.0##", "en-US")).toBe("123,456,789.988");
    });

    it("Decimal Number 9 negative number", () => {
        expect(Globalize.format(-123456789.98765, "#,##0.0##", "en-US")).toBe("-123,456,789.988");
    });
});
