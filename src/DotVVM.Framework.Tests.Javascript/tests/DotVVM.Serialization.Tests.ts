/// <reference path="../scripts/typings/jasmine/jasmine.d.ts" />
/// <reference path="../../DotVVM.Framework/Resources/scripts/typings/knockout/knockout.d.ts" />
/// <reference path="./DotVVM.d.ts" />

var dotvvm = new DotVVM();
var assertObservable = (object: any): any => {
    expect(ko.isObservable(object)).toBeTruthy("Object was expected to be an observable.");
    return object();
}

var assertNotObservable = (object: any): any => {
    expect(ko.isObservable(object)).toBeFalsy("Object was an observable, where simple object was expected.");
    return object;
}

var assertObservableArray = (object: any) => {
    expect(ko.isObservable(object) && "removeAll" in object).toBeTruthy("Object was expected to be an observable array.");
    return object()
}

var asserObservableString = (object: any, expected: string) => {
    assertObservable(object);
    expect(object()).toBe(expected);
}

describe("DotVVM.Serialization - deserialize", () => {

    it("Deserialize scalar number value", () => {
        expect(dotvvm.serialization.deserialize(10)).toBe(10);
    });

    it("Deserialize scalar string value", () => {
        expect(dotvvm.serialization.deserialize("aaa")).toBe("aaa");
    });

    it("Deserialize scalar boolean value", () => {
        expect(dotvvm.serialization.deserialize(true)).toBe(true);
    });

    it("Deserialize null value", () => {
        expect(dotvvm.serialization.deserialize(null)).toBe(null);
    });

    it("Deserialize object with one property", () => {
        var obj = dotvvm.serialization.deserialize({ a: "aaa" });
        expect(ko.isObservable(obj)).toBeFalsy();
        expect(ko.isObservable(obj.a)).toBeTruthy();
        expect(obj.a()).toBe("aaa");
    });

    it("Deserialize object with doNotUpdate option", () => {
        var obj = dotvvm.serialization.deserialize({ a: "aaa", "a$options": { doNotUpdate: true } });
        expect(ko.isObservable(obj)).toBeFalsy();
        expect(ko.isObservable(obj.a)).toBeTruthy();
        expect(obj.a()).toBeUndefined();
    });

    it("Deserialize object with isDate option", () => {
        var obj = dotvvm.serialization.deserialize({ a: "2015-08-01T13:56:42.000", "a$options": { isDate: true } });
        expect(ko.isObservable(obj)).toBeFalsy();
        expect(ko.isObservable(obj.a)).toBeTruthy();
        expect(typeof obj.a()).toBe("string");
        expect(new Date(obj.a()).getTime()).toBe(new Date(2015, 7, 1, 13, 56, 42).getTime());
    });

    it("Deserialize Date scalar", () => {
        var obj = dotvvm.serialization.deserialize(new Date(Date.UTC(2015, 7, 1, 13, 56, 42)));
        expect(ko.isObservable(obj)).toBeFalsy();
        expect(typeof obj).toBe("string");
        expect(obj).toBe("2015-08-01T13:56:42.0000000");
    });

    it("Deserialize object with Date (it should set the options.isDate)", () => {
        var obj = dotvvm.serialization.deserialize({ a: new Date(Date.UTC(2015, 7, 1, 13, 56, 42)), a$options: {} });
        expect(ko.isObservable(obj)).toBeFalsy();
        expect(ko.isObservable(obj.a)).toBeTruthy();
        expect(typeof obj.a()).toBe("string");
        expect(obj.a()).toBe("2015-08-01T13:56:42.0000000");
        expect(obj.a$options.isDate).toBeTruthy();
    });

    it("Deserialize object with Date (it should create the options.isDate)", () => {
        var obj = dotvvm.serialization.deserialize({ a: new Date(Date.UTC(2015, 7, 1, 13, 56, 42)) });
        expect(ko.isObservable(obj)).toBeFalsy();
        expect(ko.isObservable(obj.a)).toBeTruthy();
        expect(typeof obj.a()).toBe("string");
        expect(obj.a()).toBe("2015-08-01T13:56:42.0000000");
        expect(obj.a$options.isDate).toBeTruthy();
    });

    it("Deserialize object with array", () => {
        var obj = dotvvm.serialization.deserialize({ a: ["aaa", "bbb", "ccc"] });
        expect(ko.isObservable(obj)).toBeFalsy();
        expect(ko.isObservable(obj.a)).toBeTruthy();
        expect(obj.a() instanceof Array).toBeTruthy();
        expect(obj.a().length).toBe(3);

        expect(ko.isObservable(obj.a()[0])).toBeTruthy();
        expect(obj.a()[0]()).toBe("aaa");

        expect(ko.isObservable(obj.a()[1])).toBeTruthy();
        expect(obj.a()[1]()).toBe("bbb");

        expect(ko.isObservable(obj.a()[2])).toBeTruthy();
        expect(obj.a()[2]()).toBe("ccc");
    });

    it("Deserialize array to target array", () => {
        var array = dotvvm.serialization.deserialize(["aaa", "bbb", "ccc"], ["aa", "bb"]);
        assertNotObservable(array);

        expect(array instanceof Array).toBeTruthy();
        expect(array.length).toBe(3);

        expect(ko.isObservable(array[0])).toBeTruthy();
        expect(array[0]()).toBe("aaa");

        expect(ko.isObservable(array[1])).toBeTruthy();
        expect(array[1]()).toBe("bbb");

        expect(ko.isObservable(array[2])).toBeTruthy();
        expect(array[2]()).toBe("ccc");
    });

    it("Deserialize observable element array to target observable element array", () => {
        var viewmodel = [
            ko.observable("aaa"),
            ko.observable("bbb"),
            ko.observable("ccc")];

        var target = [
            ko.observable("aa"),
            ko.observable("bb")];

        var array = dotvvm.serialization.deserialize(viewmodel, target);
        assertNotObservable(array);

        expect(array instanceof Array).toBeTruthy();
        expect(array.length).toBe(3);

        expect(ko.isObservable(array[0])).toBeTruthy();
        expect(array[0]()).toBe("aaa");

        expect(ko.isObservable(array[1]));
        expect(array[1]()).toBe("bbb");

        expect(ko.isObservable(array[2])).toBeTruthy();
        expect(array[2]()).toBe("ccc");
    });

    it("Deserialize observable array to target observable array", () => {
        var viewmodel = ko.observableArray([
            ko.observable("aaa"),
            ko.observable("bbb"),
            ko.observable("ccc")]);

        var target = ko.observableArray([
            ko.observable("aa"),
            ko.observable("bb")]);

        expect(() => dotvvm.serialization.deserialize(viewmodel, target))
            .toThrowError("Parameter viewModel should not be an observable. Maybe you forget to invoke the observable you are passing as a viewModel parameter.");
    });

    it("Deserialize observable array inside array to target array of observable string", function () {
        var target = [
            ko.observable("a")
        ];
        var viewmodel = [
            ko.observableArray([ko.observable("bb")])
        ];

        var result = assertNotObservable(dotvvm.serialization.deserialize(viewmodel, target));

        var subArray = assertObservableArray(result[0]);
        var element = assertObservable(subArray[0]);
        expect(element).toBe("bb");
    });

    it("Deserialize array inside object to target object with observable string property", function () {
        var target = {
            Prop: ko.observable("a")
        };
        var viewmodel = {
            Prop: [ko.observable("bb")]
        };

        var result = assertNotObservable(dotvvm.serialization.deserialize(viewmodel, target));

        assertObservableArray(target.Prop);

        var subArray = assertObservableArray(result.Prop);
        var element = assertObservable(subArray[0]);
        expect(element).toBe("bb");
    });

    it("Deserialize observable array inside array to target observable array of observable string", function () {
        var target = ko.observableArray([
            ko.observable("a")
        ]);
        var viewmodel = [
            ko.observableArray([ko.observable("bb")])
        ];

        var result = assertObservable(dotvvm.serialization.deserialize(viewmodel, target));

        var subArray = assertObservableArray(result[0]);
        var element = assertObservable(subArray[0]);
        expect(element).toBe("bb");
    });

    it("Deserialize observable array inside array to target array of non-observable string", function () {
        var target = [
            "a"
        ];
        var viewmodel = [
            ko.observableArray([ko.observable("bb")])
        ];

        var result = assertNotObservable(dotvvm.serialization.deserialize(viewmodel, target));

        var subArray = assertObservableArray(result[0]);
        var element = assertObservable(subArray[0]);
        expect(element).toBe("bb");
    });

    it("Deserialize observable array inside array to target array of observable strings (2)", function () {
        var target = [
            ko.observable("a"),
            ko.observable("b")
        ];
        var viewmodel = [
            ko.observableArray([ko.observable("bb")])
        ];

        var result = assertNotObservable(dotvvm.serialization.deserialize(viewmodel, target));

        var subArray = assertObservableArray(result[0]);
        var element = assertObservable(subArray[0]);
        expect(element).toBe("bb");
    });

    it("Deserialize non-observable complex object no target", () => {
        var viewmodel = createComplexNonObservableViewmodel();

        var resultObservable = dotvvm.serialization.deserialize(viewmodel);

        var result = assertNotObservable(resultObservable);
        assertHierarchy(result);
    });

    it("Deserialize observable complex object to target observable complex object", () => {
        var target = createComplexObservableTarget();
        var viewmodel = createComplexObservableViewmodel();

        var resultObservable = dotvvm.serialization.deserialize(viewmodel, target);

        var result = assertObservable(resultObservable);
        assertHierarchy(result);

        var targetObject = assertObservable(target);
        expect(targetObject instanceof Object).toBeTruthy();
        assertHierarchy(targetObject);
    });

    it("Deserialize observable complex object to target observable property", () => {
        var target = createComplexObservableTarget();
        var viewmodel = createComplexObservableSubViewmodel();

        var resultObservable = dotvvm.serialization.deserialize(viewmodel, target().Prop2);
        var result = assertObservable(resultObservable);
        assertSubHierarchy(result);

        var targetObject = assertObservable(target)
        asserObservableString(targetObject.Prop1, "a");
        let targetProp2Object = assertObservable(targetObject.Prop2);
        assertSubHierarchy(targetProp2Object);
    });

    it("Deserialize observable complex object to target observable property - target and model not linked", () => {
        var target = createComplexObservableTarget();
        var viewmodel = createComplexObservableSubViewmodel();

        dotvvm.serialization.deserialize(viewmodel, target().Prop2);

        assertSubHierarchiesNotLinked(viewmodel, target().Prop2());
    });

    it("Deserialize observable complex object property to target observable property", () => {
        var target = createComplexObservableTarget();
        var viewmodel = createComplexObservableViewmodel();

        dotvvm.serialization.deserialize(viewmodel.Prop2(), target().Prop2);

        let targetObject = assertObservable(target) as ObservableHierarchy;

        assertHierarchy(viewmodel);
        expect(targetObject.Prop1()).toBe("a");
        assertSubHierarchy(targetObject.Prop2())
    });

    it("Deserialize observable complex object to target with null property", () => {
        var target = createComplexObservableTargetWithNullSubHierarchy();
        var viewmodel = createComplexObservableViewmodel();

        dotvvm.serialization.deserialize(viewmodel, target);

        let targetObject = assertObservable(target) as ObservableHierarchy;

        assertHierarchy(viewmodel);
        assertHierarchy(targetObject);
    });

    it("Deserialize observable complex object to target with null array element", () => {
        var target = createComplexObservableTargetWithNullArrayElement();
        var viewmodel = createComplexObservableViewmodel();

        dotvvm.serialization.deserialize(viewmodel, target);

        let targetObject = assertObservable(target) as ObservableHierarchy;

        assertHierarchy(viewmodel);
        assertHierarchy(targetObject);
    });

    it("Deserialize observable complex object to target with array element property containing null in observable", () => {
        var target = createComplexObservableTargetWithArrayElementPropertyObservableNull();
        var viewmodel = createComplexObservableViewmodel();

        dotvvm.serialization.deserialize(viewmodel, target);

        let targetObject = assertObservable(target) as ObservableHierarchy;

        assertHierarchy(viewmodel);
        assertHierarchy(targetObject);
    });

    it("Deserialize observable complex object to target with array element property containing null", () => {
        var target = createComplexObservableTargetWithArrayElementPropertyNull();
        var viewmodel = createComplexObservableViewmodel();

        dotvvm.serialization.deserialize(viewmodel, target);

        let targetObject = assertObservable(target) as ObservableHierarchy;

        assertHierarchy(viewmodel);
        assertHierarchy(targetObject);
    });

    it("Deserialize observable complex object to target with array element with property missing", () => {
        var target = createComplexObservableTargetWithArrayElementPropertyMissing();
        var viewmodel = createComplexObservableViewmodel();

        dotvvm.serialization.deserialize(viewmodel, target);

        let targetObject = assertObservable(target) as ObservableHierarchy;

        assertHierarchy(viewmodel);
        assertHierarchy(targetObject);
    });

    it("Deserialize observable complex object to target with missing array element", () => {
        var target = createComplexObservableTargetWithMissingArrayElement();
        var viewmodel = createComplexObservableViewmodel();

        dotvvm.serialization.deserialize(viewmodel, target);

        let targetObject = assertObservable(target) as ObservableHierarchy;

        assertHierarchy(viewmodel);
        assertHierarchy(targetObject);
    });

    it("Deserialize observable complex object to target with missing and null array elements", () => {
        var target = createComplexObservableTargetWithArrayElementMissingAndNull();
        var viewmodel = createComplexObservableViewmodel();

        dotvvm.serialization.deserialize(viewmodel, target);

        let targetObject = assertObservable(target) as ObservableHierarchy;

        assertHierarchy(viewmodel);
        assertHierarchy(targetObject);
    });

    it("Deserialize observable complex object to target with missing property for sub-hierarchy", () => {
        var target = createComplexObservableTargetWithMissingSubHierarchy();
        var viewmodel = createComplexObservableViewmodel();

        dotvvm.serialization.deserialize(viewmodel, target);

        let targetObject = assertObservable(target) as ObservableHierarchy;

        assertHierarchy(viewmodel);
        assertHierarchy(targetObject);
    });

    it("Deserialize observable complex object property to target observable property - target and model not linked", () => {
        var target = createComplexObservableTarget();
        var viewmodel = createComplexObservableViewmodel();

        dotvvm.serialization.deserialize(viewmodel.Prop2(), target().Prop2);

        assertSubHierarchiesNotLinked(viewmodel.Prop2(), target().Prop2());
    });


    it("Deserialize object with arrays and subobjects", () => {
        var obj = dotvvm.serialization.deserialize({ a: [{ b: 1, c: [0, 1] }] });
        expect(ko.isObservable(obj)).toBeFalsy();
        expect(ko.isObservable(obj.a)).toBeTruthy();
        expect(obj.a() instanceof Array).toBeTruthy();

        expect(obj.a().length).toBe(1);
        expect(ko.isObservable(obj.a()[0])).toBeTruthy();

        var inner = obj.a()[0]();
        expect(typeof inner).toBe("object");

        expect(ko.isObservable(inner.b)).toBeTruthy();
        expect(inner.b()).toBe(1);

        expect(ko.isObservable(inner.c)).toBeTruthy();
        expect(inner.c() instanceof Array).toBeTruthy();

        expect(ko.isObservable(inner.c()[0])).toBeTruthy();
        expect(inner.c()[0]()).toBe(0);

        expect(ko.isObservable(inner.c()[1])).toBeTruthy();
        expect(inner.c()[1]()).toBe(1);
    });

    it("Deserialize into an existing instance - updating the observable property", () => {
        var obj = { a: "bbb" };
        var existing = {
            a: ko.observable("aaa")
        };

        var numberOfUpdates = 0;
        existing.a.subscribe(() => numberOfUpdates++);

        dotvvm.serialization.deserialize(obj, existing);

        expect(numberOfUpdates).toBe(1);
        expect(existing.a()).toBe("bbb");
    });

    it("Deserialize into an existing instance with hierarchy - updating only the inner the observable property", () => {
        var obj = { a: { b: "bbb" } };
        var existing = {
            a: ko.observable({
                b: ko.observable("aaa")
            })
        };

        var numberOfOuterUpdates = 0;
        var numberOfInnerUpdates = 0;
        existing.a.subscribe(() => numberOfOuterUpdates++);
        existing.a().b.subscribe(() => numberOfInnerUpdates++);

        dotvvm.serialization.deserialize(obj, existing);

        expect(numberOfOuterUpdates).toBe(0);
        expect(numberOfInnerUpdates).toBe(1);
        expect(existing.a().b()).toBe("bbb");
    });

    it("Deserialize into an existing instance - updating the observable array", () => {
        var obj = { a: ["bbb", "ccc"] };
        var existing = {
            a: ko.observableArray([ko.observable("aaa")])
        };

        var numberOfUpdates = 0;
        existing.a.subscribe(() => numberOfUpdates++);

        dotvvm.serialization.deserialize(obj, existing);

        expect(numberOfUpdates).toBe(1);
        expect(existing.a().length).toBe(2);

        expect(ko.isObservable(existing.a()[0])).toBeTruthy();
        expect(existing.a()[0]()).toBe("bbb");

        expect(ko.isObservable(existing.a()[1])).toBeTruthy();
        expect(existing.a()[1]()).toBe("ccc");
    });

    it("Deserialize into an existing instance - updating the observable array with same number of elements - the array itself must not change", () => {
        var obj = { a: ["bbb", "ccc"] };
        var existing = {
            a: ko.observableArray([ko.observable("aaa"), ko.observable("aaa2")])
        };

        var numberOfUpdates = 0;
        existing.a.subscribe(() => numberOfUpdates++);

        dotvvm.serialization.deserialize(obj, existing);

        expect(numberOfUpdates).toBe(0);
        expect(existing.a().length).toBe(2);

        expect(ko.isObservable(existing.a()[0])).toBeTruthy();
        expect(existing.a()[0]()).toBe("bbb");

        expect(ko.isObservable(existing.a()[1])).toBeTruthy();
        expect(existing.a()[1]()).toBe("ccc");
    });

    it("Deserialize into an existing instance - updating the observable array of objects - one element is the same as before", () => {
        var obj = { a: [{ b: 1 }, { b: 2 }] };
        var existing = {
            a: ko.observableArray([
                ko.observable({ b: ko.observable(2) }),
                ko.observable({ b: ko.observable(2) })
            ])
        };

        var numberOfUpdates = 0;
        var numberOfUpdates_obj1 = 0;
        var numberOfUpdates_obj2 = 0;
        var numberOfUpdates_obj1_b = 0;
        var numberOfUpdates_obj2_b = 0;
        existing.a.subscribe(() => numberOfUpdates++);
        existing.a()[0].subscribe(() => numberOfUpdates_obj1++);
        existing.a()[1].subscribe(() => numberOfUpdates_obj2++);
        existing.a()[0]().b.subscribe(() => numberOfUpdates_obj1_b++);
        existing.a()[1]().b.subscribe(() => numberOfUpdates_obj2_b++);

        dotvvm.serialization.deserialize(obj, existing);

        expect(numberOfUpdates).toBe(0);
        expect(numberOfUpdates_obj1).toBe(0);
        expect(numberOfUpdates_obj2).toBe(0);
        expect(numberOfUpdates_obj1_b).toBe(1);
        expect(numberOfUpdates_obj2_b).toBe(0);     // second element is not changed, so no update should occur
        expect(existing.a().length).toBe(2);

        expect(ko.isObservable(existing.a()[0])).toBeTruthy();
        expect(ko.isObservable(existing.a()[0]().b)).toBeTruthy();
        expect(existing.a()[0]().b()).toBe(1);

        expect(ko.isObservable(existing.a()[1])).toBeTruthy();
        expect(ko.isObservable(existing.a()[1]().b)).toBeTruthy();
        expect(existing.a()[1]().b()).toBe(2);
    });

    it("Deserialize into an existing instance - doNotUpdate is ignored in the deserializeAll mode", () => {
        var obj = { a: "bbb", "a$options": { doNotUpdate: true } };
        var existing = {
            a: ko.observable("aaa")
        };

        dotvvm.serialization.deserialize(obj, existing, true);
        expect(existing.a()).toBe("bbb");
    });

    it("Deserialize object - check whether the options are copied", () => {
        var obj = dotvvm.serialization.deserialize({ a: "aaa", "a$options": { myCustomOption: 1 } });
        expect(ko.isObservable(obj)).toBeFalsy();
        expect(ko.isObservable(obj.a)).toBeTruthy();
        expect(obj["a$options"].myCustomOption).toBe(1);
    });
});

describe("Dotvvm.Deserialization - value type validation", () => {
    var supportedTypes = [
        "int64", "int32", "int16", "int8", "uint64", "uint32", "uint16", "uint8", "decimal", "double", "single"
    ];

    it("null is invalid",
        () => {
            for (var type in supportedTypes) {
                expect(dotvvm.serialization.validateType(null, supportedTypes[type])).toBe(false);
            }
        });

    it("undefined is invalid",
        () => {
            for (var type in supportedTypes) {
                expect(dotvvm.serialization.validateType(undefined, supportedTypes[type])).toBe(false);
            }
        });

    it("null is valid for nullable",
        () => {
            for (var type in supportedTypes) {
                expect(dotvvm.serialization.validateType(null, supportedTypes[type] + "?")).toBe(true);
            }
        });

    it("undefined is valid for nullable",
        () => {
            for (var type in supportedTypes) {
                expect(dotvvm.serialization.validateType(undefined, supportedTypes[type] + "?")).toBe(true);
            }
        });

    it("string is invalid",
        () => {
            for (var type in supportedTypes) {
                expect(dotvvm.serialization.validateType("string123", supportedTypes[type])).toBe(false);
            }
        });
});


describe("DotVVM.Serialization - serialize", () => {

    it("Serialize scalar number value", () => {
        var obj = ko.observable(10);
        expect(dotvvm.serialization.serialize(obj)).toBe(10);
    });

    it("Serialize scalar string value", () => {
        var obj = ko.observable("aaa");
        expect(dotvvm.serialization.serialize(obj)).toBe("aaa");
    });

    it("Serialize scalar boolean value", () => {
        var obj = ko.observable(true);
        expect(dotvvm.serialization.serialize(obj)).toBe(true);
    });

    it("Deserialize null value", () => {
        var obj = ko.observable(null);
        expect(dotvvm.serialization.serialize(obj)).toBe(null);
    });

    it("Serialize object with one property", () => {
        var obj = dotvvm.serialization.serialize({
            a: ko.observable("aaa")
        });
        expect(obj.a).toBe("aaa");
    });

    it("Serialize object with doNotPost option", () => {
        var obj = dotvvm.serialization.serialize({
            a: ko.observable("aaa"),
            "a$options": { doNotPost: true }
        });
        expect(obj.a).toBeUndefined();
        expect(obj["a$options"]).toBeUndefined();
    });

    it("Serialize object with Date property", () => {
        var obj = dotvvm.serialization.serialize({
            a: ko.observable(new Date(Date.UTC(2015, 7, 1, 13, 56, 42))),
            "a$options": { isDate: true }
        });
        expect(typeof obj.a).toBe("string");
        expect(new Date(obj.a).getTime()).toBe(new Date(2015, 7, 1, 13, 56, 42).getTime());
        expect(obj["a$options"]).toBeUndefined();
    });

    it("Serialize object with Date property for REST API", () => {
        var obj = dotvvm.serialization.serialize({
            a: ko.observable(new Date(Date.UTC(2015, 7, 1, 13, 56, 42))),
            "a$options": { isDate: true }
        }, {
            restApiTarget: true
        });
        expect(obj.a instanceof Date).toBeTruthy();
        expect(obj.a.getTime()).toBe(new Date(Date.UTC(2015, 7, 1, 13, 56, 42)).getTime());
        expect(obj["a$options"]).toBeUndefined();
    });

    it("Serialize object with array", () => {
        var obj = dotvvm.serialization.serialize({
            a: ko.observableArray([
                ko.observable("aaa"),
                ko.observable("bbb"),
                ko.observable("ccc")
            ])
        });
        expect(obj.a instanceof Array).toBeTruthy();
        expect(obj.a.length).toBe(3);
        expect(obj.a[0]).toBe("aaa");
        expect(obj.a[1]).toBe("bbb");
        expect(obj.a[2]).toBe("ccc");
    });

    it("Serialize object with arrays and subobjects", () => {
        var obj = dotvvm.serialization.serialize({
            a: ko.observableArray([
                ko.observable({
                    b: ko.observable(1),
                    c: ko.observableArray([
                        ko.observable(0),
                        ko.observable(1)
                    ])
                })
            ])
        });
        expect(obj.a instanceof Array).toBeTruthy();
        expect(obj.a.length).toBe(1);
        expect(obj.a[0].b).toBe(1);
        expect(obj.a[0].c instanceof Array).toBeTruthy();
        expect(obj.a[0].c[0]).toBe(0);
        expect(obj.a[0].c[1]).toBe(1);
    });

    it("Serialize - doNotPost is ignored in the serializeAll mode", () => {
        var obj = dotvvm.serialization.serialize({
            a: ko.observable("bbb"),
            "a$options": { doNotPost: true }
        }, { serializeAll: true });

        expect(obj.a).toBe("bbb");
        expect(obj["a$options"].doNotPost).toBeTruthy();
    });
    it("Serialize - ko.observable with undefined should be converted to null", () => {
        var obj = dotvvm.serialization.serialize({
            a: ko.observable(undefined)
        }, { serializeAll: true });

        expect(obj.a).toBe(null);
    });

    it("Deserialize - null replaced with object",
        () => {
            var viewModel = {
                selected: ko.observable(null),
                items: ko.observable([
                    ko.observable({
                        id: ko.observable(1)
                    }),
                    ko.observable({
                        id: ko.observable(2)
                    }),
                    ko.observable({
                        id: ko.observable(3)
                    })
                ])
            };

            dotvvm.serialization.deserialize(viewModel.items()[0](), viewModel.selected);
            expect(viewModel.selected().id()).toBe(1);
            expect(viewModel.selected()).not.toBe(viewModel.items()[0]());
            expect(viewModel.selected().id).not.toBe(viewModel.items()[0]().id);
        });

    it("Deserialize - null replaced with object and then with another object",
        () => {
            var viewModel = {
                selected: ko.observable(null),
                items: ko.observable([
                    ko.observable({
                        id: ko.observable(1)
                    }),
                    ko.observable({
                        id: ko.observable(2)
                    }),
                    ko.observable({
                        id: ko.observable(3)
                    })
                ])
            };

            dotvvm.serialization.deserialize(viewModel.items()[0](), viewModel.selected);
            dotvvm.serialization.deserialize(viewModel.items()[1](), viewModel.selected);
            expect(viewModel.selected().id()).toBe(2);
            expect(viewModel.items()[0]().id()).toBe(1);
            expect(viewModel.items()[1]().id()).toBe(2);
        });

    it("Deserialize - check that observable is returned if and only if target is observable - numeric to numeric", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getNumericTg());
        let tg = testData.getNumericTg();

        const numeralResult = assertNotObservable(dotvvm.serialization.deserialize(testData.numericVm, tg));
        const numeralUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.numericVm, observableTg));
        expect(numeralResult).toBe(testData.numericVm);
        expect(numeralUnwrapedResult).toBe(testData.numericVm);

        expect(observableTg()).toBe(testData.numericVm);
    });

    it("Deserialize - check that observable is returned if and only if target is observable - boolean to boolean", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getBoolTg());
        let tg = testData.getBoolTg();

        const boolResult = assertNotObservable(dotvvm.serialization.deserialize(testData.boolVm, tg));
        const boolUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.boolVm, observableTg));
        expect(boolResult).toBe(testData.boolVm);
        expect(boolUnwrapedResult).toBe(testData.boolVm);

        expect(observableTg()).toBe(testData.boolVm);
    });

    it("Deserialize - check that observable is returned if and only if target is observable - date to date", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getDateTg());
        let tg = testData.getDateTg();

        const dateResult = assertNotObservable(dotvvm.serialization.deserialize(testData.dateVm, tg));
        const dateUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.dateVm, observableTg));
        expect(dateResult).toBe(testData.dateVmString);
        expect(dateUnwrapedResult).toBe(testData.dateVmString);

        expect(observableTg()).toBe(testData.dateVmString);
    });

    it("Deserialize - check that observable is returned if and only if target is observable - string to string", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getStringTg());
        let tg = testData.getStringTg();

        const stringResult = assertNotObservable(dotvvm.serialization.deserialize(testData.stringVm, tg));
        const stringUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.stringVm, observableTg));
        expect(stringResult).toBe(testData.stringVm);
        expect(stringUnwrapedResult).toBe(testData.stringVm);

        expect(observableTg()).toBe(testData.stringVm);
    });

    it("Deserialize - check that observable is returned if and only if target is observable - array[2] to array[2]", () => {
        const testData = new TestData();
        let observableTg: any = ko.observable(testData.getArray2Tg());
        let tg: any = testData.getArray2Tg();

        const array2Result = assertNotObservable(dotvvm.serialization.deserialize(testData.array2Vm, tg));
        const array2UnwrapedResult = assertObservableArray(dotvvm.serialization.deserialize(testData.array2Vm, observableTg));
        testData.assertArray2Result(array2Result);
        testData.assertArray2Result(array2UnwrapedResult);

        testData.assertArray2Result(observableTg());
    });

    it("Deserialize - check that observable is returned if and only if target is observable - array[3] to array[2]", () => {
        const testData = new TestData();
        let observableTg: any = ko.observable(testData.getArray2Tg());
        let tg: any = testData.getArray2Tg();

        const array3Result = assertNotObservable(dotvvm.serialization.deserialize(testData.array3Vm, tg));
        const array3UnwrapedResult = assertObservableArray(dotvvm.serialization.deserialize(testData.array3Vm, observableTg));
        testData.assertArray3Result(array3Result);
        testData.assertArray3Result(array3UnwrapedResult);

        testData.assertArray3Result(observableTg());
    });

    it("Deserialize - check that observable is returned if and only if target is observable - object to object", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getObjectTg());
        let tg = testData.getObjectTg();

        const objectResult = assertNotObservable(dotvvm.serialization.deserialize(testData.objectVm, tg));
        const objectUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.objectVm, observableTg));
        testData.assertObjectResult(objectResult);
        testData.assertObjectResult(objectUnwrapedResult);

        testData.assertObjectResult(tg);
        testData.assertObjectResult(observableTg());
    });

    it("Deserialize - check that observable is returned if and only if target is observable - numeric to object", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getObjectTg());
        let tg = testData.getObjectTg();

        const numeralResult = assertNotObservable(dotvvm.serialization.deserialize(testData.numericVm, tg));
        const numeralUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.numericVm, observableTg));
        expect(numeralResult).toBe(testData.numericVm);
        expect(numeralUnwrapedResult).toBe(testData.numericVm);

        expect(observableTg()).toBe(testData.numericVm);
    });

    it("Deserialize - check that observable is returned if and only if target is observable - null to object", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getObjectTg());
        let tg = testData.getObjectTg();

        const nullResult = assertNotObservable(dotvvm.serialization.deserialize(null, tg));
        const nullUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(null, observableTg));
        expect(nullResult).toBe(null);
        expect(nullUnwrapedResult).toBe(null);

        expect(observableTg()).toBe(null);
    });

    it("Deserialize - check that observable is returned if and only if target is observable - boolean to object", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getObjectTg());
        let tg = testData.getObjectTg();

        const boolResult = assertNotObservable(dotvvm.serialization.deserialize(testData.boolVm, tg));
        const boolUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.boolVm, observableTg));
        expect(boolResult).toBe(testData.boolVm);
        expect(boolUnwrapedResult).toBe(testData.boolVm);

        expect(observableTg()).toBe(testData.boolVm);
    });

    it("Deserialize - check that observable is returned if and only if target is observable - string to object", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getObjectTg());
        let tg = testData.getObjectTg();

        const stringResult = assertNotObservable(dotvvm.serialization.deserialize(testData.stringVm, tg));
        const stringUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.stringVm, observableTg));
        expect(stringResult).toBe(testData.stringVm);
        expect(stringUnwrapedResult).toBe(testData.stringVm);

        expect(observableTg()).toBe(testData.stringVm);
    });

    it("Deserialize - check that observable is returned if and only if target is observable - date to object", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getObjectTg());
        let tg = testData.getObjectTg();

        const dateResult = assertNotObservable(dotvvm.serialization.deserialize(testData.dateVm, tg));
        const dateUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.dateVm, observableTg));
        expect(dateResult).toBe(testData.dateVmString);
        expect(dateUnwrapedResult).toBe(testData.dateVmString);

        expect(observableTg()).toBe(testData.dateVmString);
    });

    it("Deserialize - check that observable is returned if and only if target is observable - array[2] to object", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getObjectTg());
        let tg = testData.getObjectTg();

        const dateResult = assertNotObservable(dotvvm.serialization.deserialize(testData.array2Vm, tg));
        const dateUnwrapedResult = assertObservableArray(dotvvm.serialization.deserialize(testData.array2Vm, observableTg));
        testData.assertArray2Result(dateResult);
        testData.assertArray2Result(dateUnwrapedResult);

        testData.assertArray2Result(observableTg());
    });

    it("Deserialize - check that observable is returned if and only if target is observable - object to numeric", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getNumericTg());
        let tg = testData.getNumericTg();

        const objectResult = assertNotObservable(dotvvm.serialization.deserialize(testData.objectVm, tg));
        const objectUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.objectVm, observableTg));
        testData.assertObjectResult(objectResult);
        testData.assertObjectResult(objectUnwrapedResult);

        testData.assertObjectResult(observableTg());
    });

    it("Deserialize - check that observable is returned if and only if target is observable - object to null", () => {
        const testData = new TestData();
        let observableTg = ko.observable(null);
        let tg = null;

        const objectResult = assertNotObservable(dotvvm.serialization.deserialize(testData.objectVm, tg));
        const objectUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.objectVm, observableTg));
        testData.assertObjectResult(objectResult);
        testData.assertObjectResult(objectUnwrapedResult);

        testData.assertObjectResult(observableTg());
    });

    it("Deserialize - check that observable is returned if and only if target is observable - object to boolean", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getBoolTg());
        let tg = testData.getBoolTg();

        const objectResult = assertNotObservable(dotvvm.serialization.deserialize(testData.objectVm, tg));
        const objectUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.objectVm, observableTg));
        testData.assertObjectResult(objectResult);
        testData.assertObjectResult(objectUnwrapedResult);

        testData.assertObjectResult(observableTg());
    });

    it("Deserialize - check that observable is returned if and only if target is observable - object to string", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getStringTg());
        let tg = testData.getStringTg();

        const objectResult = assertNotObservable(dotvvm.serialization.deserialize(testData.objectVm, tg));
        const objectUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.objectVm, observableTg));
        testData.assertObjectResult(objectResult);
        testData.assertObjectResult(objectUnwrapedResult);

        testData.assertObjectResult(observableTg());
    });

    it("Deserialize - check that observable is returned if and only if target is observable - object to date", () => {
        const testData = new TestData();
        let observableTg = ko.observable(testData.getDateTg());
        let tg = testData.getDateTg();

        const objectResult = assertNotObservable(dotvvm.serialization.deserialize(testData.objectVm, tg));
        const objectUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.objectVm, observableTg));
        testData.assertObjectResult(objectResult);
        testData.assertObjectResult(objectUnwrapedResult);

        testData.assertObjectResult(tg);
        testData.assertObjectResult(observableTg());
    });

    it("Deserialize - check that observable is returned if and only if target is observable - object to array[2]", () => {
        const testData = new TestData();
        let tg = testData.getArray2Tg();
        let observableTg = ko.observable(testData.getArray2Tg());

        const objectResult = assertNotObservable(dotvvm.serialization.deserialize(testData.objectVm, tg));
        const objectUnwrapedResult = assertObservable(dotvvm.serialization.deserialize(testData.objectVm, observableTg));
        testData.assertObjectResult(objectResult);
        testData.assertObjectResult(objectUnwrapedResult);

        testData.assertObjectResult(tg);
        testData.assertObjectResult(observableTg());
    });
});

function assertSubHierarchiesNotLinked(viewmodel: ObservableSubHierarchy, target: ObservableSubHierarchy) {

    viewmodel.Prop21("xx");
    expect(target.Prop21()).toBe("bb");
    //array not linked
    viewmodel.Prop23.push(ko.observable({
        Prop231: ko.observable("ff")
    }));
    expect(target.Prop23().length).toBe(2);
    //array objects not linked
    viewmodel.Prop23()[0]().Prop231("yy");
    expect(target.Prop23()[0]().Prop231()).toBe("dd");
}

function assertHierarchy(result: ObservableHierarchy) {
    asserObservableString(result.Prop1, "aa");
    let prop2Object = assertObservable(result.Prop2);
    assertSubHierarchy(prop2Object);
}

function assertSubHierarchy(prop2Object: ObservableSubHierarchy) {
    asserObservableString(prop2Object.Prop21, "bb");
    asserObservableString(prop2Object.Prop22, "cc");
    let prop23Array = assertObservableArray(prop2Object.Prop23);
    let prop23ArrayFirst = assertObservable(prop23Array[0]);
    let prop23ArraySecond = assertObservable(prop23Array[1]);
    asserObservableString(prop23ArrayFirst.Prop231, "dd");
    asserObservableString(prop23ArraySecond.Prop231, "ee");
}

function createComplexObservableTarget(): KnockoutObservable<ObservableHierarchy> {
    return ko.observable({
        Prop1: ko.observable("a"),
        Prop2: ko.observable({
            Prop21: ko.observable("b"),
            Prop22: ko.observable("c"),
            Prop23: ko.observableArray([
                ko.observable({
                    Prop231: ko.observable("d")
                }),
                ko.observable({
                    Prop231: ko.observable("e")
                })
            ])
        })
    });
}

function createComplexObservableTargetWithNullArrayElement(): KnockoutObservable<ObservableHierarchy> {
    return ko.observable({
        Prop1: ko.observable("a"),
        Prop2: ko.observable({
            Prop21: ko.observable("b"),
            Prop22: ko.observable("c"),
            Prop23: ko.observableArray([
                null,
                ko.observable({
                    Prop231: ko.observable("e")
                })
            ])
        })
    });
}

function createComplexObservableTargetWithArrayElementPropertyMissing(): KnockoutObservable<any> {
    return ko.observable({
        Prop1: ko.observable("a"),
        Prop2: ko.observable({
            Prop21: ko.observable("b"),
            Prop22: ko.observable("c"),
            Prop23: ko.observableArray([
                ko.observable({
                }),
                ko.observable({
                    Prop231: ko.observable("e")
                })
            ])
        })
    });
}

function createComplexObservableTargetWithArrayElementPropertyNull(): KnockoutObservable<ObservableHierarchy> {
    return ko.observable({
        Prop1: ko.observable("a"),
        Prop2: ko.observable({
            Prop21: ko.observable("b"),
            Prop22: ko.observable("c"),
            Prop23: ko.observableArray([
                ko.observable({
                    Prop231: null
                }),
                ko.observable({
                    Prop231: ko.observable("e")
                })
            ])
        })
    });
}

function createComplexObservableTargetWithArrayElementMissingAndNull(): KnockoutObservable<ObservableHierarchy> {
    return ko.observable({
        Prop1: ko.observable("a"),
        Prop2: ko.observable({
            Prop21: ko.observable("b"),
            Prop22: ko.observable("c"),
            Prop23: ko.observableArray([
                null
            ])
        })
    });
}

function createComplexObservableTargetWithArrayElementPropertyObservableNull(): KnockoutObservable<ObservableHierarchy> {
    return ko.observable({
        Prop1: ko.observable("a"),
        Prop2: ko.observable({
            Prop21: ko.observable("b"),
            Prop22: ko.observable("c"),
            Prop23: ko.observableArray([
                ko.observable({
                    Prop231: ko.observable(null)
                }),
                ko.observable({
                    Prop231: ko.observable("e")
                })
            ])
        })
    });
}

function createComplexObservableTargetWithMissingArrayElement(): KnockoutObservable<ObservableHierarchy> {
    return ko.observable({
        Prop1: ko.observable("a"),
        Prop2: ko.observable({
            Prop21: ko.observable("b"),
            Prop22: ko.observable("c"),
            Prop23: ko.observableArray([
                ko.observable({
                    Prop231: ko.observable("e")
                })
            ])
        })
    });
}

function createComplexObservableTargetWithNullSubHierarchy(): KnockoutObservable<ObservableHierarchy> {
    return ko.observable({
        Prop1: ko.observable("a"),
        Prop2: null
    });
}

function createComplexObservableTargetWithMissingSubHierarchy(): KnockoutObservable<any> {
    return ko.observable({
        Prop1: ko.observable("a")
    });
}

function createComplexObservableViewmodel(): ObservableHierarchy {
    return {
        Prop1: ko.observable("aa"),
        Prop2: ko.observable(createComplexObservableSubViewmodel())
    };
}

function createComplexObservableSubViewmodel(): ObservableSubHierarchy {
    return {
        Prop21: ko.observable("bb"),
        Prop22: ko.observable("cc"),
        Prop23: ko.observableArray([
            ko.observable({
                Prop231: ko.observable("dd")
            }),
            ko.observable({
                Prop231: ko.observable("ee")
            })
        ])
    };
}

function createComplexNonObservableViewmodel() {
    return {
        Prop1: "aa",
        Prop2: {
            Prop21: "bb",
            Prop22: "cc",
            Prop23: [
                {
                    Prop231: "dd"
                },
                {
                    Prop231: "ee"
                }
            ]
        }
    };
}


class TestData {
    numericVm: number = 5;
    boolVm: boolean = true;
    stringVm: string = "viewmodel";
    dateVm: Date = new Date(1995, 11, 17);
    dateVmString: string = "1995-12-16T23:00:00.0000000";
    array2Vm = ["aa", "bb"]
    array3Vm = ["aa", "bb", "cc"];
    objectVm = { Prop1: "aa", Prop2: "bb" };

    getNumericTg(): number {
        return 7;
    }
    getBoolTg(): boolean {
        return false;
    }
    getDateTg(): Date {
        return new Date(2019, 1, 1);
    }
    getStringTg(): string {
        return "target";
    }
    getArray2Tg(): string[] {
        return ["a", "b"];
    }
    getObjectTg(): any {
        return {
            Prop1: "a",
            Prop2: "b"
        };
    }

    assertArray2Result(array2: KnockoutObservable<string>[]): void {
        expect(array2 instanceof Array).toBeTruthy();
        expect(array2.length).toBe(2);

        var item0 = assertObservable(array2[0]);
        expect(item0).toBe("aa");

        var item1 = assertObservable(array2[1]);
        expect(item1).toBe("bb");
    }

    assertArray3Result(array3: KnockoutObservable<string>[]): void {
        expect(array3 instanceof Array).toBeTruthy();
        expect(array3.length).toBe(3);

        var item0 = assertObservable(array3[0]);
        expect(item0).toBe("aa");

        var item1 = assertObservable(array3[1]);
        expect(item1).toBe("bb");

        var item2 = assertObservable(array3[2]);
        expect(item2).toBe("cc");
    }

    assertObjectResult(object: any): void {
        expect(object instanceof Object).toBeTruthy();

        var prop1 = assertObservable(object.Prop1);
        expect(prop1).toBe("aa");

        var prop2 = assertObservable(object.Prop2);
        expect(prop2).toBe("bb");
    }
}

interface ObservableHierarchy {
    Prop1: KnockoutObservable<string>;
    Prop2: KnockoutObservable<ObservableSubHierarchy>;
}

interface ObservableSubHierarchy {
    Prop21: KnockoutObservable<string>;
    Prop22: KnockoutObservable<string>;
    Prop23: KnockoutObservableArray<KnockoutObservable<{ Prop231: KnockoutObservable<string>; }>>;
} 
