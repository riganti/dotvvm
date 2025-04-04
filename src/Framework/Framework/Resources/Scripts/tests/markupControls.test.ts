import { isObservableArray } from '../utils/knockout'
import { initDotvvm } from './helper'

const viewModel = {
	$type: { type: "dynamic" },
	X: "",
	ComplexObj: {
		A: 1,
		B: [ 1, 2, 3 ]
	},
	NullField: null
}
initDotvvm({viewModel})

test("dotvvm-with-control-properties correctly wraps primitive values", () => {
	const div = document.createElement("div")
	div.innerHTML = `
		<div data-bind="dotvvm-with-control-properties: { 
			Null: null,
			Undefined: undefined,
			Number: 1,
			String: 'test',
			Boolean: true,
			Object: {},
			Array: [],
			Function: function(a) {return a},
			Prop1: Prop1,
			Prop1_: Prop1(),
		}"> <span id=x data-bind="text: $control.Prop1" /> </div>
	`

	ko.applyBindings({ Prop1: ko.observable("A") }, div)

	const x = div.querySelector("#x")!
	expect(x.textContent).toBe("A")

	const context: any = ko.contextFor(x).$control

	expect(context.Null).observable()
	expect(context.Undefined).observable()
	expect(context.Number).observable()
	expect(context.String).observable()
	expect(context.Boolean).observable()
	expect(context.Object).observable()
	expect(context.Array).observable()
	expect(context.Function).observable()
	expect(context.Prop1).observable()
	expect(context.Prop1_).observable()

	expect(context.Null()).toStrictEqual(null)
	expect(context.Undefined()).toStrictEqual(undefined)
	expect(context.Number()).toStrictEqual(1)
	expect(context.String()).toStrictEqual("test")
	expect(context.Boolean()).toStrictEqual(true)
	expect(context.Object()).toStrictEqual({})
	expect(context.Array()).toStrictEqual([])
	expect(typeof context.Function()).toStrictEqual("function")
	expect(context.Function()("test")).toStrictEqual("test")
	expect(context.Prop1()).toStrictEqual("A")
	expect(context.Prop1_()).toStrictEqual("A")
})


test("dotvvm-with-control-properties correctly wraps state", () => {
	const div = document.createElement("div")
	div.innerHTML = `
		<div data-bind="dotvvm-with-control-properties: { 
			Obj: ComplexObj
		}"> <span id=x /> </div>
	`

	ko.applyBindings(dotvvm.viewModelObservables.root, div)

	const x = div.querySelector("#x")!
	const context: any = ko.contextFor(x).$control

	expect(context.Obj).observable()
	expect(context.Obj().A).observable()
	expect(context.Obj().A()).toBe(1)
	expect(context.Obj.state).toStrictEqual(viewModel.ComplexObj)

	dotvvm.patchState({ ComplexObj: { A: 42 } })
	expect(context.Obj.state).toStrictEqual({...viewModel.ComplexObj, A: 42})
	dotvvm.rootStateManager.doUpdateNow()
	expect(context.Obj().A()).toBe(42)

	context.Obj.patchState({ A: 43 })
	expect(context.Obj.state).toStrictEqual({...viewModel.ComplexObj, A: 43})
	expect(dotvvm.state.ComplexObj.A).toBe(43)
})

it.each([
	[ "{ Something: ComplexObj, Arr: [1, 2] }", false ],
	[ "ko.observable({ Something: ComplexObj, Arr: [1, 2] })", false ],
	[ "{ Something: ComplexObj, Arr: [1, ko.observable(2)] }", false ],
	[ "{ Something: ComplexObj, Arr: ko.observableArray([ko.observable(1), 2]) }", false ],
	[ "{ Something: ko.observable(ComplexObj()), Arr: ko.observableArray([ko.observable(1), 2]) }", true ],
	[ "{ Something: ko.pureComputed(() => ComplexObj()), Arr: ko.observableArray([ko.observable(1), 2]) }", true ],
	[ "{ Something: ComplexObj(), Arr: [1, 2] }", true ],
])("dotvvm-with-control-properties correctly creates state (%s)", (binding, requiresSync) => {
	dotvvm.setState(viewModel); dotvvm.rootStateManager.doUpdateNow()
	const div = document.createElement("div")
	div.innerHTML = `
		<div data-bind="dotvvm-with-control-properties: { 
			Obj1: ${binding}
		}">
			<div data-bind="dotvvm-with-control-properties: { Obj: $control.Obj1() }">
				<span id=x />
			</div>
		</div>
	`

	ko.applyBindings(dotvvm.viewModelObservables.root, div)

	const x = div.querySelector("#x")!
	const context: any = ko.contextFor(x).$control

	expect(context.Obj).observable()
	expect(context.Obj().Something).observable()
	expect(context.Obj().Something().A).observable()
	expect(context.Obj().Arr).observable()
	expect(context.Obj().Arr()[0]).observable()
	expect(context.Obj().Arr()[0]()).toBe(1)
	expect(context.Obj().Arr()[1]()).toBe(2)
	expect(context.Obj().Something().A()).toBe(1)
	expect(context.Obj.state).toStrictEqual({ Something: viewModel.ComplexObj, Arr: [1, 2] })

	dotvvm.patchState({ ComplexObj: { A: 4321 } })
	if (requiresSync) dotvvm.rootStateManager.doUpdateNow()
	expect(context.Obj.state.Something).toStrictEqual({...viewModel.ComplexObj, A: 4321})

	context.Obj().Something().A.updateState((a: number) => a * 10)
	if (requiresSync) dotvvm.rootStateManager.doUpdateNow()
	expect(context.Obj.state).toStrictEqual({ Something: {...viewModel.ComplexObj, A: 43210}, Arr: [1, 2] })
})

it.each([
	[ "[1,2]", true, false ],
	[ "ko.observableArray([1, 2])", true, true ],
	[ "ComplexObj().B", true, true ],
	[ "ComplexObj().B()", true, false ],
	[ "ComplexObj().B().map(_ => 1)", true, false ],
	[ "ComplexObj().B().length", false, false ],
	[ "ComplexObj", false, false ],
	[ "ComplexObj()", false, false ],
	[ "null", false, false ],
	[ "NullField", false, false ],
])("dotvvm-with-control-properties makes observable array before its touched (%s)", (binding, isArray, tryObservableArray) => {
	dotvvm.setState(viewModel); dotvvm.rootStateManager.doUpdateNow()

	const div = document.createElement("div")
	div.innerHTML = `
		<div data-bind="dotvvm-with-control-properties: { 
			Arr: ${binding}
		}">
			<span id=x />
			<div data-bind="dotvvm-with-control-properties: { Arr: $control.Arr }">
				<span id=y />
			</div>
		</div>
	`

	ko.applyBindings(dotvvm.viewModelObservables.root, div)

	const x = div.querySelector("#x")!
	const y = div.querySelector("#y")!
	const contextX: any = ko.contextFor(x).$control
	const contextY: any = ko.contextFor(y).$control

	expect(contextX.Arr).observable()
	expect(contextY.Arr).observable()

	expect(isObservableArray(contextX.Arr)).toBe(isArray)
	expect(isObservableArray(contextY.Arr)).toBe(isArray)

	if (tryObservableArray) {
		contextX.Arr.unshift(99)
		const ix = contextX.Arr.indexOf(contextX.Arr()[0])
		const ix2 = contextY.Arr.indexOf(contextX.Arr()[0])
		expect(ix2).toEqual(ix)
	}
})

test("dotvvm-with-control-properties correctly wraps null changed to array", () => {
	dotvvm.setState(viewModel)

	dotvvm.patchState({ ComplexObj: null })
	dotvvm.rootStateManager.doUpdateNow()

	const div = document.createElement("div")
	div.innerHTML = `
		<div data-bind="dotvvm-with-control-properties: { 
			Arr: ComplexObj()?.B
		}"> <span id=x /> </div>
	`
	ko.applyBindings(dotvvm.viewModelObservables.root, div)
	const x = div.querySelector("#x")!
	const context: any = ko.contextFor(x).$control
	expect(context.Arr).observable()
	expect(context.Arr.state).toEqual(undefined)
	expect(context.Arr()).toEqual(undefined)
	expect("push" in context.Arr).toBe(false)

	dotvvm.patchState({ ComplexObj: { B: [1, 2] } })
	dotvvm.rootStateManager.doUpdateNow()

	expect(context.Arr()?.map((x: any) => x())).toStrictEqual([1, 2])
	expect(context.Arr.state).toStrictEqual([1, 2])
	expect("push" in context.Arr).toBe(true)

	context.Arr.push(3)
	expect(context.Arr()?.map((x:any) => x())).toStrictEqual([1, 2, 3])
	expect(dotvvm.state.ComplexObj.B).toStrictEqual([1, 2, 3])

	context.Arr.unshift(0)
	context.Arr.splice(1, 1)

	expect(dotvvm.state.ComplexObj.B).toStrictEqual([0, 2, 3])
})
