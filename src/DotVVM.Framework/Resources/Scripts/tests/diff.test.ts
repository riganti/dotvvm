import { diffViewModel } from "../postback/updater"

const diff = diffViewModel
const diffEqual = {}

test("diff on numbers returns target value", () => {
    const orig = 1
    expect(diff(orig, 1)).toEqual(1)
    expect(diff(orig, 2)).toEqual(2)
})

test("diff on strings returns target value", () => {
    const orig = "a"
    expect(diff(orig, "a")).toEqual("a")
    expect(diff(orig, "b")).toEqual("b")

})

test("diff on array of primitive values returns array of primitive values if it is different", () => {

    const orig = ["a", "b"]
    expect(diff(orig, ["a", "b"])).toEqual(diffEqual)
    expect(diff(orig, ["a", "b", "c"])).toEqual(["a", "b", "c"])
    expect(diff(orig, ["a"])).toEqual(["a"])

})

test("diff on objects returns only changed properties", () => {

    const orig = { a: 1, b: "a" }
    expect(diff(orig, { a: 1, b: "a" })).toEqual(diffEqual)
    expect(diff(orig, { a: 2, b: "a" })).toEqual({ a: 2 })
    expect(diff(orig, { a: 2, b: "a", c: null })).toEqual({ a: 2, c: null })
    expect(diff(orig, { a: 2 })).toEqual({ a: 2 })

})

test("diff on objects recursive returns only changed properties", () => {

    const orig = { a: 1, bparent: { b: "a", c: 1 } }
    expect(diff(orig, { a: 1, bparent: { b: "a", c: 1 } })).toEqual(diffEqual)
    expect(diff(orig, { a: 2, b: "a" })).toEqual({ a: 2, b: "a" })
    expect(diff(orig, { a: 1, bparent: { b: "a", c: 2 } })).toEqual({ bparent: { c: 2 } })
    expect(diff(orig, { a: 1, bparent: { b: "a", c: 2, d: 3 } })).toEqual({ bparent: { c: 2, d: 3 } })
    expect(diff(orig, { a: 1, bparent: { b: "b" } })).toEqual({ bparent: { b: "b" } })

})

test("diff on array of objects", () => {

    const orig = [{ a: 1 }, { a: 2 }]
    expect(diff(orig, [{ a: 1 }, { a: 2 }])).toEqual(diffEqual)
    expect(diff(orig, [{ a: 3 }, { a: 2 }])).toEqual([{ a: 3 }, {}])
    expect(diff(orig, [{ a: 1 }, { a: 2 }, { a: 3 }])).toEqual([{}, {}, { a: 3 }])
    expect(diff(orig, [{ a: 2 }])).toEqual([{ a: 2 }])
    expect(diff(orig, [{ a: 1 }])).toEqual([{}])

})
