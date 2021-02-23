import { tryCoerce } from "../metadata/coercer"
import { initDotvvm } from "./helper";

initDotvvm({
    viewModel: {
        $type: "t1",
        a: null
    },
    typeMetadata: {
        t1: {
            type: "object",
            properties: {
                a: {
                    type: "String"
                }                
            }
        },
        t2: {
            type: "object",
            properties: {
                b: {
                    type: "e1"
                }
            }
        },
        t3: {
            type: "object",
            properties: {
                c: {
                    type: "t1"
                }
            }
        },
        t4: {
            type: "object",
            properties: {
                d: {
                    type: [ "t5" ]
                }
            }
        },
        t5: {
            type: "object",
            properties: {
                f: {
                    type: { type: "nullable", inner: "Boolean" }
                }
            }
        },
        t6: {
            type: "object",
            properties: {
                g: {
                    type: [ [ "t1" ] ]
                }
            }
        },
        e1: {
            type: "enum",
            values: {
                Zero: 0,
                One: 1,
                Two: 2
            }
        },
        d1: {
            type: "dynamic"
        }
    }
}, "en-US");


test("number - valid, no coercion", () => {
    const result = tryCoerce(1, "Int32");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual(1);
})

test("number - valid, trim decimal places", () => {
    const result = tryCoerce(1.56, "Int32");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual(1);
})

test("number - valid, keep decimal places", () => {
    const result = tryCoerce(1.56, "Single");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual(1.56);
})

test("number - valid, convert from string and trim decimal places", () => {
    const result = tryCoerce("1234.56", "Int32");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual(1234);
})

test("number - valid, convert from string and keep decimal places", () => {
    const result = tryCoerce("1234.56", "Double");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual(1234.56);
})

test("number - invalid, out of range", () => {
    const result = tryCoerce(100000, "Int16");
    expect(result.isError).toBeTruthy();
})

test("number - invalid, null", () => {
    const result = tryCoerce(null, "Int32");
    expect(result.isError).toBeTruthy();
})

test("number - invalid, undefined", () => {
    const result = tryCoerce(void 0, "Int32");
    expect(result.isError).toBeTruthy();
})

test("number - invalid, unparsable string", () => {
    const result = tryCoerce("xxx", "Int32");
    expect(result.isError).toBeTruthy();
})

test("number - invalid, object", () => {
    const result = tryCoerce({}, "Int32");
    expect(result.isError).toBeTruthy();
})

test("number - invalid, array", () => {
    const result = tryCoerce([], "Int32");
    expect(result.isError).toBeTruthy();
})

test("nullable number - valid, no coercion", () => {
    const result = tryCoerce(1, { type: "nullable", inner: "Int32" });
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual(1);
})

test("nullable number - valid, null", () => {
    const result = tryCoerce(null, { type: "nullable", inner: "Int32" });
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual(null);
})

test("nullable number - valid, undefined", () => {
    const result = tryCoerce(void 0, { type: "nullable", inner: "Int32" });
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual(null);
})


test("Date - valid, native Date", () => {
    const result = tryCoerce(new Date(2020, 0, 10, 12, 34, 56), "DateTime");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual("2020-01-10T12:34:56.0000000");
})

test("Date - valid, string representation", () => {
    const result = tryCoerce("2020-01-10T12:34:56.0000000", "DateTime");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual("2020-01-10T12:34:56.0000000");
})

test("Date - invalid, string representation", () => {
    const result = tryCoerce("2020-01-10", "DateTime");         // TODO
    expect(result.isError).toBeTruthy();
})

test("Date - invalid, null", () => {
    const result = tryCoerce(null, "DateTime");
    expect(result.isError).toBeTruthy();
})

test("Date - invalid, undefined", () => {
    const result = tryCoerce(void 0, "DateTime");
    expect(result.isError).toBeTruthy();
})


test("Guid - valid", () => {
    const result = tryCoerce("00000000-0000-0000-0000-000000000000", "Guid");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual("00000000-0000-0000-0000-000000000000");
})

test("Guid - invalid, wrong format", () => {
    const result = tryCoerce("xx", "Guid");
    expect(result.isError).toBeTruthy();
})

test("Guid - invalid, null", () => {
    const result = tryCoerce(null, "Guid");
    expect(result.isError).toBeTruthy();
})

test("Guid - invalid, undefined", () => {
    const result = tryCoerce(void 0, "Guid");
    expect(result.isError).toBeTruthy();
})


test("char - valid", () => {
    const result = tryCoerce("a", "Char");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual("a");
})

test("char - valid, trimmed", () => {
    const result = tryCoerce("abcd", "Char");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual("a");
})

test("char - valid, converted from ASCII", () => {
    const result = tryCoerce(65, "Char");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual("A");
})

test("char - invalid, null", () => {
    const result = tryCoerce(null, "Char");
    expect(result.isError).toBeTruthy();
})

test("char - invalid, undefined", () => {
    const result = tryCoerce(void 0, "Char");
    expect(result.isError).toBeTruthy();
})



test("string - valid", () => {
    const result = tryCoerce("abcd", "String");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual("abcd");
})

test("string - valid, converted from number", () => {
    const result = tryCoerce(-15, "String");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual("-15.00");        // TODO - format
})

test("string - valid, converted from boolean", () => {
    const result = tryCoerce(true, "String");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual("true");
})

test("string - valid, converted from Date", () => {
    const result = tryCoerce(new Date(2020, 0, 10, 12, 34, 56), "String");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual("1/10/2020 12:34 PM");     // TODO - format
})

test("string - valid, null", () => {
    const result = tryCoerce(null, "String");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual(null);
})

test("string - valid, undefined", () => {
    const result = tryCoerce(void 0, "String");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual(null);
})

test("string - invalid, object", () => {
    const result = tryCoerce({}, "String");
    expect(result.isError).toBeTruthy();
})

test("string - invalid, array", () => {
    const result = tryCoerce([], "String");
    expect(result.isError).toBeTruthy();
})


test("boolean - valid, true", () => {
    const result = tryCoerce(true, "Boolean");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual(true);
})

test("boolean - valid, false", () => {
    const result = tryCoerce(false, "Boolean");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual(false);
})

test("boolean - valid, converted from number", () => {
    const result = tryCoerce(2, "Boolean");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual(true);
})

test("boolean - valid, converted from string", () => {
    const result = tryCoerce("true", "Boolean");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual(true);
})

test("boolean - invalid, null", () => {
    const result = tryCoerce(null, "Boolean");
    expect(result.isError).toBeTruthy();
})

test("boolean - invalid, undefined", () => {
    const result = tryCoerce(void 0, "Boolean");
    expect(result.isError).toBeTruthy();
})


test("object - valid", () => {
    const result = tryCoerce({ $type: "t1", a: "aa" }, "t1");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ $type: "t1", a: "aa" });
})

test("object - valid, infer $type", () => {
    const result = tryCoerce({ a: "aa" }, "t1");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual({ $type: "t1", a: "aa" });
})

test("object - valid, with coercion", () => {
    const result = tryCoerce({ $type: "t1", a: 15 }, "t1");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual({ $type: "t1", a: "15.00" });
})

test("object - valid, enum property", () => {
    const result = tryCoerce({ $type: "t2", b: "One" }, "t2");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ $type: "t2", b: "One" });
})

test("object - valid, enum property with coercion", () => {
    const result = tryCoerce({ $type: "t2", b: 2 }, "t2");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual({ $type: "t2", b: "Two" });
})

test("object - invalid, enum property unknown value", () => {
    const result = tryCoerce({ $type: "t2", b: "xxx" }, "t2");
    expect(result.isError).toBeTruthy();
})

test("object - valid, child object null", () => {
    const result = tryCoerce({ $type: "t3", c: null }, "t3");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ $type: "t3", c: null });
})

test("object - valid, child object", () => {
    const result = tryCoerce({ $type: "t3", c: { $type: "t1", a: null } }, "t3");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ $type: "t3", c: { $type: "t1", a: null } });
})

test("object - valid, child object, infer $type", () => {
    const result = tryCoerce({ $type: "t3", c: { a: null } }, "t3");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual({ $type: "t3", c: { $type: "t1", a: null } });
})

test("object - valid, child array null", () => {
    const result = tryCoerce({ $type: "t4", d: null }, "t4");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ $type: "t4", d: null });
})

test("object - valid, child array empty", () => {
    const result = tryCoerce({ $type: "t4", d: [] }, "t4");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ $type: "t4", d: [] });
})

test("object - valid, child array", () => {
    const result = tryCoerce({ $type: "t4", d: [ { $type: "t5", f: null }, { $type: "t5", f: true } ] }, "t4");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ $type: "t4", d: [ { $type: "t5", f: null }, { $type: "t5", f: true } ] });
})

test("object - valid, child array, infer $type", () => {
    const result = tryCoerce({ $type: "t4", d: [ { f: null }, { f: true } ] }, "t4");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual({ $type: "t4", d: [ { $type: "t5", f: null }, { $type: "t5", f: true } ] });
})

test("object - valid, child array, infer $type, coerce child values", () => {
    const result = tryCoerce({ $type: "t4", d: [ { }, { f: true } ] }, "t4");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual({ $type: "t4", d: [ { $type: "t5", f: null }, { $type: "t5", f: true } ] });
})

test("object - valid, child array of arrays null", () => {
    const result = tryCoerce({ $type: "t6", g: null }, "t6");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ $type: "t6", g: null });
})

test("object - valid, child array of arrays inner null", () => {
    const result = tryCoerce({ $type: "t6", g: [ null ] }, "t6");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ $type: "t6", g: [ null ] });
})

test("object - valid, child array of arrays empty", () => {
    const result = tryCoerce({ $type: "t6", g: [[]] }, "t6");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ $type: "t6", g: [[]] });
})

test("object - valid, child array of arrays", () => {
    const result = tryCoerce({ $type: "t6", g: [[ { $type: "t1", a: "aaa" } ]] }, "t6");
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ $type: "t6", g: [[ { $type: "t1", a: "aaa" } ]] });
})

test("object - valid, child array of arrays, infer $type", () => {
    const result = tryCoerce({ $type: "t6", g: [[ { a: "aaa" } ]] }, "t6");
    expect(result.wasCoerced).toBeTruthy();
    expect(result.value).toEqual({ $type: "t6", g: [[ { $type: "t1", a: "aaa" } ]] });
})


test("dynamic - valid, primitive", () => {
    const result = tryCoerce(1, { type: "dynamic" });
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual(1);
})

test("dynamic - valid, object of known type", () => {
    const result = tryCoerce({ $type: "t1", a: "aa" }, { type: "dynamic" });
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ $type: "t1", a: "aa" });
})

test("dynamic - valid, object of unknown type", () => {
    const result = tryCoerce({ a: 15 }, { type: "dynamic" });
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ a: 15 });
})

test("dynamic - valid, object of unknown type, nested known object", () => {
    const result = tryCoerce({ inner: { $type: "t1", a: "aa" } }, { type: "dynamic" });
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ inner: { $type: "t1", a: "aa" } });
})

test("dynamic - valid, object of unknown type, nested known object", () => {
    const result = tryCoerce({ inner: { $type: "t1", a: "aa" } }, { type: "dynamic" });
    expect(result.wasCoerced).toBeFalsy();
    expect(result.value).toEqual({ inner: { $type: "t1", a: "aa" } });
})
