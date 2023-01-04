import { unmapKnockoutObservables } from "../state-manager"
test("Unmaps ko.observable", () => {
	const obj = {
		foo: ko.observable("bar"),
		bar: "foo"
	}
	const result = unmapKnockoutObservables(obj)
	expect(obj.foo).observable()
	expect(result.foo).toBe("bar")
	expect(result.bar).toBe("foo")
})

test("Returns the same object, when no observables are present", () => {
    expect(unmapKnockoutObservables(1)).toBe(1)
    expect(unmapKnockoutObservables("aaa")).toBe("aaa")

	const obj1 = { a: 1, b: "bbb" }
    expect(unmapKnockoutObservables(obj1)).toBe(obj1)

	const obj2 = { a: 1, b: "bbb", c: { d: 2, e: "eee" } }
	expect(unmapKnockoutObservables(obj2)).toBe(obj2)

	const obj3 = { a: 1, b: "bbb", c: { d: 2, e: "eee" }, f: [1, 2, 3] }
	expect(unmapKnockoutObservables(obj3)).toBe(obj3)

	const obj4 = { a: 1, b: "bbb", c: { d: 2, e: "eee" }, f: [1, 2, 3], g: [ { h: 1, i: "iii" }, { h: 1, i: "iii" } ] }
	expect(unmapKnockoutObservables(obj4)).toBe(obj4)
})
