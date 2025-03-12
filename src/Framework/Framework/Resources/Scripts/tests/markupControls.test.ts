import { initDotvvm } from './helper'

const viewModel = {
	$type: { type: "dynamic" },
	X: "",
	ComplexObj: {
		A: 1,
		B: [ 1, 2, 3 ]
	}
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
