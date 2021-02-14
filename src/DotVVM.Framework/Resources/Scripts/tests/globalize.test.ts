test("Zero", () => {
    expect(dotvvm_Globalize.format(10, "G", "en-US")).toBe("10");
});

test("Decimal number", () => {
    expect(dotvvm_Globalize.format(10.123, "G", "en-US")).toBe("10.123");
});

test("Negative number", () => {
    expect(dotvvm_Globalize.format(-25.000, "G", "en-US")).toBe("-25");
});

test("Large number", () => {
    expect(dotvvm_Globalize.format(123456789.1234567890123456789, "G", "en-US")).toBe("123456789.12345680");
});
test("Decimal number 1 round", () => {
    expect(dotvvm_Globalize.format(10.125, "#.0#", "en-US")).toBe("10.13");
});

test("Decimal number 2 make it decimal", () => {
    expect(dotvvm_Globalize.format(10, "#.0#", "en-US")).toBe("10.0");
});

test("Decimal number 3 add zeroes and round", () => {
    expect(dotvvm_Globalize.format(1.255, "0#.0#", "en-US")).toBe("01.26");
});

test("Decimal Number 4 add zeroes", () => {
    expect(dotvvm_Globalize.format(123.5, "0000.00", "en-US")).toBe("0123.50");
});

test("Decimal number 5 round", () => {
    expect(dotvvm_Globalize.format(123.456, "###0.00", "en-US")).toBe("123.46");
});

test("Decimal Number 6 thousands separator", () => {
    expect(dotvvm_Globalize.format(123.5, "0,000.00", "en-US")).toBe("0,123.50");
});

test("Decimal number 7 thousands separator", () => {
    expect(dotvvm_Globalize.format(123.456, "#,##0.00", "en-US")).toBe("123.46");
});

test("Decimal Number 8 thousands separator", () => {
    expect(dotvvm_Globalize.format(123456789.98765, "#,###.0##", "en-US")).toBe("123,456,789.988");
});

test("Decimal Number 9 negative number", () => {
    expect(dotvvm_Globalize.format(-123456789.98765, "#,##0.0##", "en-US")).toBe("-123,456,789.988");
});

test("Decimal to integer Number 1", () => {
    expect(dotvvm_Globalize.format(123456789.98765, "#,##0", "en-US")).toBe("123,456,790");
});

test("Decimal to integer Number 2 negative number", () => {
    expect(dotvvm_Globalize.format(-123456789.98765, "#,##0", "en-US")).toBe("-123,456,790");
});

test("Decimal to integer Number 3", () => {
    expect(dotvvm_Globalize.format(12.98765, "#,000", "en-US")).toBe("013");
});

test("Decimal to integer Number 4", () => {
    expect(dotvvm_Globalize.format(1234.98765, "#,000", "en-US")).toBe("1,235");
});

test("Integer Number 1", () => {
    expect(dotvvm_Globalize.format(1234, "#0,000", "en-US")).toBe("1,234");
});

test("Integer Number 2", () => {
    expect(dotvvm_Globalize.format(1234, "#,##0", "en-US")).toBe("1,234");
});

test("Integer Number 3", () => {
    expect(dotvvm_Globalize.format(-1234, "#,##0", "en-US")).toBe("-1,234");
});

test("Integer Number 4", () => {
    expect(dotvvm_Globalize.format(1234, "00", "en-US")).toBe("1234");
});

test("Integer Number 5", () => {
    expect(dotvvm_Globalize.format(1, "00", "en-US")).toBe("01");
});

test("Integer Number 6", () => {
    expect(dotvvm_Globalize.format(-1, "00", "en-US")).toBe("-01");
});

