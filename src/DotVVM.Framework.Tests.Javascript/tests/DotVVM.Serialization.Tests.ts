/// <reference path="../scripts/typings/jasmine/jasmine.d.ts" />
/// <reference path="../../DotVVM.Framework/Resources/Scripts/DotVVM.d.ts" />

var dotvvm = new DotVVM();

describe("DotVVM.Serialization", () => {
    
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
        expect(obj.a().getTime()).toBe(Date.UTC(2015, 7, 1, 13, 56, 42));
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