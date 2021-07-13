import * as stringHelper from '../utils/stringHelper'

test("string.Contains translation", () => {
    expect(stringHelper.contains("Hello world", "world", "")).toBe(true);
    expect(stringHelper.contains("Hello world", "WORLD", "")).toBe(false);
    expect(stringHelper.contains("Hello world", "WORLD", "InvariantCultureIgnoreCase")).toBe(true);
})

test("string.EndsWith translation", () => {
    expect(stringHelper.endsWith("Hello world!", "world", "")).toBe(false);
    expect(stringHelper.endsWith("Hello world", "WORLD", "")).toBe(false);
    expect(stringHelper.endsWith("Hello world", "world", "")).toBe(true);
    expect(stringHelper.endsWith("Hello world", "WORLD", "InvariantCultureIgnoreCase")).toBe(true);
})

test("string.StartsWith translation", () => {
    expect(stringHelper.startsWith("Hello world", "hello", "")).toBe(false);
    expect(stringHelper.startsWith("!Hello world", "Hello", "")).toBe(false);
    expect(stringHelper.startsWith("Hello world", "Hello", "")).toBe(true);
    expect(stringHelper.startsWith("Hello world", "HELLO", "InvariantCultureIgnoreCase")).toBe(true);
})

test("string.IndexOf translation", () => {
    expect(stringHelper.indexOf("Hello world", 0, "aaaa", "")).toBe(-1);
    expect(stringHelper.indexOf("Hello world", 0, "Hello", "")).toBe(0);
    expect(stringHelper.indexOf("Hello world", 1, "Hello", "")).toBe(-1);
    expect(stringHelper.indexOf("Hello world", 6, "WORLD", "")).toBe(-1);
    expect(stringHelper.indexOf("Hello world", 6, "WORLD", "InvariantCultureIgnoreCase")).toBe(6);
})

test("string.LastIndexOf translation", () => {
    expect(stringHelper.lastIndexOf("Hello world", 0, "aaaa", "")).toBe(-1);
    expect(stringHelper.lastIndexOf("Hello world", 0, "Hello", "")).toBe(0);
    expect(stringHelper.lastIndexOf("Hello world", 1, "Hello", "")).toBe(-1);
    expect(stringHelper.lastIndexOf("Hello world", 6, "WORLD", "")).toBe(-1);
    expect(stringHelper.lastIndexOf("Hello world", 6, "WORLD", "InvariantCultureIgnoreCase")).toBe(6);
})
