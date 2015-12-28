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