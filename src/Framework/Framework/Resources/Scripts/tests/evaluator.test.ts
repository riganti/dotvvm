import dotvvm from '../dotvvm-root'
import * as evaluator from '../utils/evaluator'

jest.mock("../metadata/typeMap", () => ({
    getTypeInfo(typeId: string) {
        return testTypeMap[typeId];
    },
    getObjectTypeInfo(typeId: string): ObjectTypeMetadata {
        return testTypeMap[typeId] as any;
    }
}));

const assertObservable = (object: any): any => {
    expect(object).observable()
    return object()
}

const assertNotObservable = (object: any): any => {
    expect(object).not.observable()
    return object
}

const assertObservableArray = (object: any) => {
    expect(object).observableArray()
    return object()
}

const assertObservableString = (object: any, expected: string) => {
    assertObservable(object)
    expect(object()).toBe(expected)
}

describe("DotVVM.Utils.Evaluator - traverseContext", () => {

    test("evaluate absolute path", () => {    
        const fakeContext = getParentedContext();

        const rootSlash = evaluator.traverseContext(fakeContext.$root,"/Prop2/Prop22")
        assertObservableString(rootSlash, "c");
    })

    test("evaluate absolute array path", () => {    
        const fakeContext = getParentedContext();

        const rootSlash = evaluator.traverseContext(fakeContext.$root,"/Prop2/Prop23/1/Prop231")
        assertObservableString(rootSlash, "e");
    })
})


function getParentedContext():ParentedContext{
    const hierarchy = createComplexObservableTargetWithNullArrayElement();
    
    return {
        $parent: hierarchy().Prop2!(),
        $parents: [hierarchy().Prop2!()],
        $data: hierarchy().Prop2!().Prop22(),
        $rawData: hierarchy().Prop2!().Prop22,
        $root: hierarchy()
    };
}

function createComplexObservableTargetWithNullArrayElement(): KnockoutObservable<ObservableHierarchy> {
    return ko.observable({
        $type: ko.observable("t5"),
        Prop1: ko.observable("a"),
        Prop2: ko.observable({
            $type: ko.observable("t5_a"),
            Prop21: ko.observable("b"),
            Prop22: ko.observable("c"),
            Prop23: ko.observableArray([
                null,
                ko.observable({
                    $type: ko.observable("t5_a_a"),
                    Prop231: ko.observable("e")
                })
            ])
        })
    })
}

function createComplexObservableTargetWithArrayElementPropertyMissing(): KnockoutObservable<any> {
    return ko.observable({
        $type: ko.observable("t5"),
        Prop1: ko.observable("a"),
        Prop2: ko.observable({
            $type: ko.observable("t5_a"),
            Prop21: ko.observable("b"),
            Prop22: ko.observable("c"),
            Prop23: ko.observableArray([
                ko.observable({
                    $type: ko.observable("t5_a_a"),
                }),
                ko.observable({
                    $type: ko.observable("t5_a_a"),
                    Prop231: ko.observable("e")
                })
            ])
        })
    })
}

function createComplexObservableTargetWithArrayElementPropertyNull(): KnockoutObservable<ObservableHierarchy> {
    return ko.observable({
        $type: ko.observable("t5"),
        Prop1: ko.observable("a"),
        Prop2: ko.observable({
            $type: ko.observable("t5_a"),
            Prop21: ko.observable("b"),
            Prop22: ko.observable("c"),
            Prop23: ko.observableArray([
                ko.observable({
                    $type: ko.observable("t5_a_a"),
                    Prop231: null
                }),
                ko.observable({
                    $type: ko.observable("t5_a_a"),
                    Prop231: ko.observable("e")
                })
            ])
        })
    })
}

function createComplexObservableTargetWithArrayElementMissingAndNull(): KnockoutObservable<ObservableHierarchy> {
    return ko.observable({
        $type: ko.observable("t5"),
        Prop1: ko.observable("a"),
        Prop2: ko.observable({
            $type: ko.observable("t5_a"),
            Prop21: ko.observable("b"),
            Prop22: ko.observable("c"),
            Prop23: ko.observableArray([
                null
            ])
        })
    })
}

function createComplexObservableTargetWithArrayElementPropertyObservableNull(): KnockoutObservable<ObservableHierarchy> {
    return ko.observable({
        $type: ko.observable("t5"),
        Prop1: ko.observable("a"),
        Prop2: ko.observable({
            $type: ko.observable("t5_a"),
            Prop21: ko.observable("b"),
            Prop22: ko.observable("c"),
            Prop23: ko.observableArray([
                ko.observable({
                    $type: ko.observable("t5_a_a"),
                    Prop231: ko.observable(null)
                }),
                ko.observable({
                    $type: ko.observable("t5_a_a"),
                    Prop231: ko.observable("e")
                })
            ])
        })
    })
}

function createComplexObservableTargetWithMissingArrayElement(): KnockoutObservable<ObservableHierarchy> {
    return ko.observable({
        $type: ko.observable("t5"),
        Prop1: ko.observable("a"),
        Prop2: ko.observable({
            $type: ko.observable("t5_a"),
            Prop21: ko.observable("b"),
            Prop22: ko.observable("c"),
            Prop23: ko.observableArray([
                ko.observable({
                    $type: ko.observable("t5_a_a"),
                    Prop231: ko.observable("e")
                })
            ])
        })
    })
}

function createComplexObservableTargetWithNullSubHierarchy(): KnockoutObservable<ObservableHierarchy> {
    return ko.observable({
        $type: ko.observable("t5"),
        Prop1: ko.observable("a"),
        Prop2: null
    })
}

function createComplexObservableTargetWithMissingSubHierarchy(): KnockoutObservable<any> {
    return ko.observable({
        $type: ko.observable("t5"),
        Prop1: ko.observable("a")
    })
}

function createComplexObservableViewmodel(): ObservableHierarchy {
    return {
        $type: ko.observable("t5"),
        Prop1: ko.observable("aa"),
        Prop2: ko.observable(createComplexObservableSubViewmodel())
    }
}

function createComplexObservableSubViewmodel(): ObservableSubHierarchy {
    return {
        $type: ko.observable("t5_a"),
        Prop21: ko.observable("bb"),
        Prop22: ko.observable("cc"),
        Prop23: ko.observableArray([
            ko.observable({
                $type: ko.observable("t5_a_a"),
                Prop231: ko.observable("dd")
            }),
            ko.observable({
                $type: ko.observable("t5_a_a"),
                Prop231: ko.observable("ee")
            })
        ])
    }
}

function createComplexNonObservableViewmodel() {
    return {
        $type: "t5",
        Prop1: "aa",
        Prop2: {
            $type: "t5_a",
            Prop21: "bb",
            Prop22: "cc",
            Prop23: [
                {
                    $type: "t5_a_a",
                    Prop231: "dd"
                },
                {
                    $type: "t5_a_a",
                    Prop231: "ee"
                }
            ]
        }
    }
}


interface ParentedContext {
    $parent: ObservableSubHierarchy,
    $parents: Array<ObservableSubHierarchy>,
    $data:  string,
    $rawData: KnockoutObservable<string>,
    $root: ObservableHierarchy
}

interface ObservableHierarchy {
    $type: string | KnockoutObservable<string>
    Prop1: KnockoutObservable<string>
    Prop2: null | KnockoutObservable<ObservableSubHierarchy>
}

interface ObservableSubHierarchy {
    $type: string | KnockoutObservable<string>
    Prop21: KnockoutObservable<string>
    Prop22: KnockoutObservable<string>
    Prop23: KnockoutObservableArray<null | KnockoutObservable<{ 
        Prop231: null | KnockoutObservable<null | string>,
        $type: string | KnockoutObservable<string>
    }>>
}

const testTypeMap: TypeMap = {
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
            a: {
                type: "String",
                update: "no",
                post: "no"
            }
        }
    },
    t3: {
        type: "object",
        properties: {
            a: {
                type: "DateTime"
            }
        }
    },
    t4: {
        type: "object",
        properties: {
            a: {
                type: [ "String" ]
            }
        }
    },
    t5: {
        type: "object",
        properties: {
            Prop1: {
                type: "String"
            },
            Prop2: {
                type: "t5_a"
            }
        }
    },
    t5_a: {
        type: "object",
        properties: {
            Prop21: {
                type: "String"
            },
            Prop22: {
                type: "String"
            },
            Prop23: {
                type: [ "t5_a_a" ]
            }
        }
    },
    t5_a_a: {
        type: "object",
        properties: {
            Prop231: {
                type: "String"
            }
        }
    },
    t6: {        
        type: "object",
        properties: {
            a: {
                type: [ "t6_a" ]
            }
        }
    },
    t6_a: {
        type: "object",
        properties: {        
            b: {
                type: "Int32"
            },
            c: {
                type: [ "Int32" ]
            }
        }
    },
    t7: {
        type: "object",
        properties: {
            a: {
                type: "t7_a"
            }
        }
    },
    t7_a: {
        type: "object",
        properties: {
            b: {
                type: "String"
            }
        }
    },
    t8: {
        type: "object",
        properties: {
            a: {
                type: [ "t8_a" ]
            }
        }
    },
    t8_a: {
        type: "object",
        properties: {
            b: {
                type: "Int32"
            }
        }
    },
    t9: {
        type: "object",
        properties: {
            a: {
                type: "Int32"
            }
        }
    },
    t10: {
        type: "object",
        properties: {
            a: {
                type: "t10_a"
            }
        }
    },
    t10_a: {        
        type: "object",
        properties: { }
    },
    t11: {
        type: "object",
        properties: {
            selected: {
                type: "t11_a"
            },
            items: {
                type: [ "t11_a" ]
            }
        }
    },
    t11_a: {
        type: "object",
        properties: {
            id: {
                type: "Int32"
            }
        }
    },
    t12: {
        type: "object",
        properties: {
            Prop1: {
                type: "String"
            },
            Prop2: {
                type: "String"
            }
        }
    },
    t13: {
        type: "object",
        properties: {
            Prop: {
                type: "String"
            }
        }
    },
    t14: {
        type: "object",
        properties: {
            Prop: {
                type: [ "String" ]
            }
        }
    }
};
