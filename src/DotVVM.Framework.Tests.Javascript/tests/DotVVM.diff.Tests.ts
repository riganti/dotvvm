/// <reference path="../scripts/typings/jasmine/jasmine.d.ts" />
/// <reference path="../../DotVVM.Framework/Resources/scripts/typings/knockout/knockout.d.ts" />
/// <reference path="../../DotVVM.Framework/Resources/Scripts/DotVVM.d.ts" />

var dotvvm = new DotVVM();

describe("DotVVM.diff", () => {
    
    it("diff on numbers returns target value", () => {

        var orig = 1;
        expect(dotvvm.diff(orig, 1)).toEqual(1);
        expect(dotvvm.diff(orig, 2)).toEqual(2);

    });

    it("diff on strings returns target value", () => {

        var orig = "a";
        expect(dotvvm.diff(orig, "a")).toEqual("a");
        expect(dotvvm.diff(orig, "b")).toEqual("b");

    });

    it("diff on array of primitive values returns array of primitive values if it is different", () => {

        var orig = ["a", "b"];
        expect(dotvvm.diff(orig, ["a", "b"])).toEqual(dotvvm.diffEqual);
        expect(dotvvm.diff(orig, ["a", "b", "c"])).toEqual(["a", "b", "c"]);
        expect(dotvvm.diff(orig, ["a"])).toEqual(["a"]);

    });
    
    it("diff on objects returns only changed properties", () => {

        var orig = { a: 1, b: "a" };
        expect(dotvvm.diff(orig, { a: 1, b: "a" })).toEqual(dotvvm.diffEqual);
        expect(dotvvm.diff(orig, { a: 2, b: "a" })).toEqual({ a: 2 });
        expect(dotvvm.diff(orig, { a: 2, b: "a", c: null })).toEqual({ a: 2, c: null });
        expect(dotvvm.diff(orig, { a: 2 })).toEqual({ a: 2 });

    });

    it("diff on objects recursive returns only changed properties", () => {

        var orig = { a: 1, bparent: { b: "a", c: 1 } };
        expect(dotvvm.diff(orig, { a: 1, bparent: { b: "a", c: 1 } })).toEqual(dotvvm.diffEqual);
        expect(dotvvm.diff(orig, { a: 2, b: "a" })).toEqual({ a: 2, b: "a" });
        expect(dotvvm.diff(orig, { a: 1, bparent: { b: "a", c: 2 } })).toEqual({ bparent: { c: 2 } });
        expect(dotvvm.diff(orig, { a: 1, bparent: { b: "a", c: 2, d: 3 } })).toEqual({ bparent: { c: 2, d: 3 } });
        expect(dotvvm.diff(orig, { a: 1, bparent: { b: "b" } })).toEqual({ bparent: { b: "b" } });

    });

    it("diff on array of objects", () => {

        var orig = [{ a: 1 }, { a: 2 }];
        expect(dotvvm.diff(orig, [{ a: 1 }, { a: 2 }])).toEqual(dotvvm.diffEqual);
        expect(dotvvm.diff(orig, [{ a: 3 }, { a: 2 }])).toEqual([{ a: 3 }, {}]);
        expect(dotvvm.diff(orig, [{ a: 1 }, { a: 2 }, { a: 3 }])).toEqual([{}, {}, { a: 3 }]);
        expect(dotvvm.diff(orig, [{ a: 2 }])).toEqual([{ a: 2 }]);
        expect(dotvvm.diff(orig, [{ a: 1 }])).toEqual([{}]);

    });

});
