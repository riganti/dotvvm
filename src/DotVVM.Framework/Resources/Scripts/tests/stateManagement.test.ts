import { initDotvvm } from "./helper";
import dotvvm from '../dotvvm-root'
import { getStateManager } from "../dotvvm-base";
import { StateManager } from "../state-manager";
import { serialize } from "../serialization/serialize";

initDotvvm({
    viewModel: {
        Int: 1,
        Str: "A",
        Array: [ {
            Id: 1
        } ],
        ArrayWillBe: null,
        Inner: {
            P1: 1,
            P2: 2,
            P3: 3
        },
        Inner2: null
    }
})

const vm = dotvvm.viewModels.root.viewModel as any
const s = getStateManager() as StateManager<any>
s.doUpdateNow()

test("Initial knockout ViewModel", () => {
    expect(vm.Int).observable()
    expect(vm.Array).observableArray()
    expect(vm.ArrayWillBe).not.observableArray()
    expect(vm.ArrayWillBe()).toBeNull()
    expect(vm.Int()).toBe(1)
    expect(vm.Array()[0]).observable()
    expect(vm.Array()[0]().Id).observable()
    expect(vm.Array()[0]().Id()).toBe(1)
    expect(vm.Inner).observable()
    expect(vm.Inner().P1).observable()
    expect(vm.Inner().P2).observable()
    expect(vm.Inner().P3).observable()
    expect(vm.Inner().P1()).toBe(1)
    expect(vm.Inner().P2()).toBe(2)
    expect(vm.Inner().P3()).toBe(3)
})

test("Dirty flag", () => {
    expect(s.isDirty).toBeFalsy()
    s.setState(s.state) // same state should do nothing
    expect(s.isDirty).toBeFalsy()
    s.setState({ ...s.state, Str: "B" })
    expect(s.isDirty).toBeTruthy()
    s.doUpdateNow()
    expect(s.isDirty).toBeFalsy()
})

test("Upgrade null to observableArray", () => {
    s.update(x => ({...x, ArrayWillBe: [ { A: "ahoj" } ] }))
    s.doUpdateNow()

    expect(vm.ArrayWillBe).observableArray()
    expect(vm.ArrayWillBe().length).toBe(1)
    expect(vm.ArrayWillBe()[0]().A).observable()
    expect(vm.ArrayWillBe()[0]().A()).toBe("ahoj")
})

test("Change observableArray to object", () => {
    // this should not happen IRL, but can when property of type `object` is used in viewModel

    s.update(x => ({...x, Array: { P: "P" } }))
    s.doUpdateNow()

    expect(vm.Array).observable()
    expect(vm.Array().P()).toBe("P")

    s.update(x => ({...x, Array: [ { Id: 17 } ] }))
    s.doUpdateNow()

    expect(vm.Array).observableArray()
    expect(vm.Array()[0]).observable()
    expect(vm.Array()[0]().Id).observable()
    expect(vm.Array()[0]().Id()).toBe(17)
})

test("Remove type properties", () => {
    s.update(x => ({...x, Inner: { P1: 5 } }))
    s.doUpdateNow()

    expect(vm.Inner().P1).observable()
    expect(vm.Inner().P1()).toBe(5)
    expect(vm.Inner().P2).toBeUndefined()
    expect(vm.Inner().P3).toBeUndefined()
    expect("P3" in vm.Inner()).toBeFalsy()
    expect("P1" in vm.Inner()).toBeTruthy()
})

test("Add type properties", () => {
    s.update(x => ({...x, Inner: { P1: 5, P4: 4 } }))
    s.doUpdateNow()

    expect(vm.Inner().P1).observable()
    expect(vm.Inner().P4).observable()
    expect(vm.Inner().P1()).toBe(5)
    expect(vm.Inner().P4()).toBe(4)
    expect(vm.Inner().P2).toBeUndefined()
    expect(vm.Inner().P3).toBeUndefined()
    expect("P2" in vm.Inner()).toBeFalsy()
    expect("P4" in vm.Inner()).toBeTruthy()
})

test("Should not change object reference", () => {
    const innerObs = vm.Inner
    const innerObj = vm.Inner()

    let changed = false

    innerObs.subscribe(() => changed = true)

    s.update(x => ({ ...x, Inner: { P1: 1, P4: null } }))
    s.doUpdateNow()

    expect(changed).toBeFalsy()
    expect((dotvvm.viewModels.root.viewModel as any).Inner).toBe(innerObs)
    expect((dotvvm.viewModels.root.viewModel as any).Inner()).toBe(innerObj)
    expect(innerObj.P1()).toBe(1)
    expect(innerObj.P4()).toBe(null)
})

test("Should not change array reference", () => {
    s.setState({ ...s.state, Array: [{ Id: 1 }] })
    s.doUpdateNow()

    const arrayObs = vm.Array
    const arrayObj = vm.Array()

    let changed = false
    arrayObs.subscribe(() => changed = true)

    s.setState({ ...s.state, Array: [{ Id: 2 }] })
    s.doUpdateNow()

    expect(changed).toBeFalsy()
    expect((dotvvm.viewModels.root.viewModel as any).Array).toBe(arrayObs)
    expect((dotvvm.viewModels.root.viewModel as any).Array()).toBe(arrayObj)
    expect(arrayObj[0]().Id()).toBe(2)
})

test("Should change array reference when length changes", () => {
    // this behavior is not strictly needed, we could do with one array
    // However, the changed variable MUST be set to true

    const arrayObs = vm.Array
    const arrayObj = vm.Array()

    let changed = false
    arrayObs.subscribe(() => changed = true)

    s.setState({ ...s.state, Array: [{ Id: 3 }, { Id: 4 }] })
    s.doUpdateNow()

    expect(changed).toBeTruthy()
    // observable should be the same, but object should be different
    expect((dotvvm.viewModels.root.viewModel as any).Array).toBe(arrayObs)
    const arrayObj2 = (dotvvm.viewModels.root.viewModel as any).Array()
    expect(arrayObj2).not.toBe(arrayObj) // | these lines are not strictly needed. Rest of the test is essential
    expect(arrayObj.length).toBe(1)      // |
    expect(arrayObj2.length).toBe(2)
    expect(arrayObj2[0]().Id()).toBe(3)
    expect(arrayObj2[1]().Id()).toBe(4)
})

test("Propagate knockout observable change", () => {

    vm.Inner(null)
    vm.Int(745)
    vm.Array()[0]().Id(500)

    expect(s.state.Inner).toBeNull()
    expect(s.state.Array[0].Id).toBe(500)
    s.update(x => ({...x, Int: x.Int + 1 }))
    s.doUpdateNow()
    expect(vm.Int()).toBe(746)
})

test("Propagate knockout object assignment", () => {

    vm.Inner({
        P1: ko.observable(4),
        P2: ko.observable(5)
    })

    expect(s.state.Inner).toStrictEqual({ P1: 4, P2: 5 })
    s.doUpdateNow()
    expect(vm.Inner().P1()).toBe(4)
    expect(vm.Inner().P2()).toBe(5)
})

test("Propagate knockout array assignment", () => {

    vm.ArrayWillBe([
        ko.observable({
            B: ko.observable("hmm")
        })
    ])

    expect(s.state.ArrayWillBe).toStrictEqual([ { B: "hmm" } ])
    s.doUpdateNow()
    expect(vm.ArrayWillBe()[0]().B()).toBe("hmm")
    vm.ArrayWillBe()[0]().B("hmm2")
    expect(s.state.ArrayWillBe).toStrictEqual([ { B: "hmm2" } ])
})

test("Propagate Date assignment", () => {
    const val = new Date(2000, 3, 3, 3, 3, 3)
    vm.Str(val)

    expect(s.state.Str).toBe(val)
    s.doUpdateNow()
    expect(vm.Str()).toBe(val)
})

test("Prop$options should not be observable", () => {
    s.update(x => ({ ...x, Inner: { P$options: { isDate: true } } }))
    s.doUpdateNow()

    expect(vm.Inner().P$options).not.observable()
    expect(vm.Inner().P$options).toStrictEqual({ isDate: true })
})

test("Serialized computed updates on changes", () => {
    const computed = ko.pureComputed(() => serialize(vm))

    let lastValue = null
    computed.subscribe(val => lastValue = val.Str)
    vm.Str("a")
    expect(lastValue).toBe("a")
    s.setState({...s.state, Str: "b"})
    s.doUpdateNow()
    expect(lastValue).toBe("b")
})
