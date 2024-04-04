import { initDotvvm, fc, waitForEnd } from "./helper";

initDotvvm({
    viewModel: {
        $type: "t1",
        Int: 1,
        Str: "A",
        Array: [{
            $type: "t2",
            Id: 1
        }],
        Enums: [ "A", "B", "A, B" ],
        ArrayWillBe: null,
        Inner: {
            $type: "t3",
            P1: 1,
            P2: 2,
            P3: 3
        },
        Inner2: null
    },
    typeMetadata: {
        t1: {
            type: "object",
            properties: {
                Int: {
                    type: "Int32"
                },
                Str: {
                    type: "String"
                },
                Array: {
                    type: [
                        "t2"
                    ]
                },
                Enums: {
                    type: [ "e1" ]
                },
                ArrayWillBe: {
                    type: [
                        "t5"
                    ]
                },
                Inner: {
                    type: "t3"
                },
                Inner2: {
                    type: "t3"
                },
                DateTime: { type: { type: "nullable", inner: "DateTime" } },
                Dynamic: {
                    type: { type: "dynamic" }
                }
            }
        },
        t2: {
            type: "object",
            properties: {
                Id: {
                    type: "Int32"
                }
            }
        },
        t3_a: {
            type: "object",
            properties: {
                "P1": {
                    type: "Int32"
                },
                "P2": {
                    type: { type: "nullable", inner: "Int32" }
                }
            }
        },
        t3: {
            type: "object",
            properties: {
                "P1": {
                    type: "Int32"
                },
                "P2": {
                    type: { type: "nullable", inner: "Int32" }
                },
                "P3": {
                    type: "Int32"
                },
                "P4": {
                    type: { type: "nullable", inner: "Int32" }
                }
            }
        },
        t4: {
            type: "object",
            properties: {
                "P": {
                    type: "String"
                }
            }
        },
        t5: {
            type: "object",
            properties: {
                "B": {
                    type: "String"
                }
            }
        },
        e1: {
            type: "enum",
            isFlags: true,
            values: {
                "A": 1,
                "B": 2,
                "C": 4,
                "D": 8,
            }
        },
        tValidated: {
            type: "object",
            properties: {
                RegexValidated: {
                    validationRules: [
                        { ruleName: "regularExpression", errorMessage: "Must have even length", parameters: ["^(..)+$"] }
                    ]
                }
            }
        }
    }
})

