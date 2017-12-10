/// <reference path="../scripts/typings/jasmine/jasmine.d.ts" />
/// <reference path="../../DotVVM.Framework/Resources/scripts/typings/knockout/knockout.d.ts" />
/// <reference path="../../DotVVM.Framework/Resources/Scripts/DotVVM.d.ts" />

var dotvvm = new DotVVM();

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

    it("Deserialize object with arrays and subobjects", () => {
        var obj = dotvvm.serialization.deserialize({ a: [ { b: 1, c: [ 0, 1] } ] });
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
        var obj = { a: [ "bbb", "ccc" ] };
        var existing = {
            a: ko.observableArray([ ko.observable("aaa") ])
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
        var obj = { a: [ { b: 1 }, { b: 2 } ] };
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
});