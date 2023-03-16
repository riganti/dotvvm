import { initDotvvm } from "./helper";
import { getTypeMetadata, getEnumMetadata } from "../metadata/metadataHelper"

initDotvvm({
    viewModel: { },
    typeMetadata:
    {
        t1: {
            type: "object",
            properties: {
                a: {
                    type: "String"
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

test("getTypeMetadata of registered dynamic - valid", () => {
    const result = getTypeMetadata("d1");
    expect(result.type).toEqual("dynamic");
})

test("getTypeMetadata of explicit dynamic - valid", () => {
    const result = getTypeMetadata({ type: "dynamic" });
    expect(result.type).toEqual("dynamic");
})

test("getTypeMetadata of registered object - valid", () => {
    const result = getTypeMetadata("t1");
    expect(result.type).toEqual("object");
})

test("getTypeMetadata of unregistered object - not valid", () => {
    const action = () => {
        getTypeMetadata("unknownType");
    }
    expect(action).toThrow(Error);
    expect(action).toThrow("Cannot find type metadata for 'unknownType'!");
})

test("getTypeMetadata of registered enum - valid", () => {
    const result = getTypeMetadata("e1");
    expect(result.type).toEqual("enum");
})

test("getEnumMetadata of registered enum - valid", () => {
    const result = getEnumMetadata("e1");
    expect(result.type).toEqual("enum");
})

test("getEnumMetadata of unregistered enum - not valid", () => {
    const action = () => {
        getEnumMetadata("unknownEnum");
    }
    expect(action).toThrow(Error);
    expect(action).toThrow("Cannot find type metadata for 'unknownEnum'!");
})

test("getEnumMetadata of registered object - not valid", () => {
    const action = () => {
        getEnumMetadata("t1");
    }
    expect(action).toThrow(Error);
    expect(action).toThrow("Expected enum, but received 'object' with id 't1'");
})
