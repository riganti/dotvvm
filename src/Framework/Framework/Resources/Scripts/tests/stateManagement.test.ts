import { fc, waitForEnd } from "./helper";
import dotvvm from '../dotvvm-root'
import { getStateManager } from "../dotvvm-base";
import { lastSetErrorSymbol, StateManager } from "../state-manager";
import { serialize } from "../serialization/serialize";
import { deserialize } from "../serialization/deserialize";
import { serializeDate } from "../serialization/date";

require('./stateManagement.data')

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
    s.update(x => ({ ...x, ArrayWillBe: [{ $type: "t5", B: "ahoj" }] }))
    s.doUpdateNow()

    expect(vm.ArrayWillBe).observableArray()
    expect(vm.ArrayWillBe().length).toBe(1)
    expect(vm.ArrayWillBe()[0]().B).observable()
    expect(vm.ArrayWillBe()[0]().B()).toBe("ahoj")
})

test("Change observableArray to object", () => {
    // this should not happen IRL, but can when property of type `object` is used in viewModel

    s.update(x => ({ ...x, Array: { $type: "t4", P: "P" } }))
    s.doUpdateNow()

    expect(vm.Array).observable()
    expect(vm.Array().P()).toBe("P")

    s.update(x => ({ ...x, Array: [{ $type: "t2", Id: 17 }] }))
    s.doUpdateNow()

    expect(vm.Array).observableArray()
    expect(vm.Array()[0]).observable()
    expect(vm.Array()[0]().Id).observable()
    expect(vm.Array()[0]().Id()).toBe(17)
})

test("Add and remove type properties", () => {
    s.update(x => ({ ...x, Inner: { $type: "t3_a", P1: 5, P2: null } }))
    s.doUpdateNow()

    expect(vm.Inner().P1).observable()
    expect(vm.Inner().P1()).toBe(5)
    expect(vm.Inner().P2).observable()
    expect(vm.Inner().P2()).toBe(null)
    expect("P3" in vm.Inner()).toBeFalsy()
    expect("P4" in vm.Inner()).toBeFalsy()

    s.update(x => ({ ...x, Inner: { $type: "t3", P1: 6, P2: 2, P3: 3, P4: 4 } }))
    s.doUpdateNow()

    expect(vm.Inner().P1).observable()
    expect(vm.Inner().P1()).toBe(6)
    expect(vm.Inner().P2).observable()
    expect(vm.Inner().P2()).toBe(2)
    expect(vm.Inner().P3).observable()
    expect(vm.Inner().P3()).toBe(3)
    expect(vm.Inner().P4).observable()
    expect(vm.Inner().P4()).toBe(4)

    s.update(x => ({ ...x, Inner: { $type: "t3_a", P1: 5, P2: null } }))
    s.doUpdateNow()

    expect(vm.Inner().P1).observable()
    expect(vm.Inner().P1()).toBe(5)
    expect(vm.Inner().P2).observable()
    expect(vm.Inner().P2()).toBe(null)
    expect("P3" in vm.Inner()).toBeFalsy()
    expect("P4" in vm.Inner()).toBeFalsy()
})

test("Should not change object reference", () => {
    const innerObs = vm.Inner
    const innerObj = vm.Inner()

    let changed = false

    innerObs.subscribe(() => changed = true)

    s.update(x => ({ ...x, Inner: { $type: "t3_a", P1: 1, P2: null } }))
    s.doUpdateNow()

    expect(changed).toBeFalsy()
    expect((dotvvm.viewModels.root.viewModel as any).Inner).toBe(innerObs)
    expect((dotvvm.viewModels.root.viewModel as any).Inner()).toBe(innerObj)
    expect(innerObj.P1()).toBe(1)
    expect(innerObj.P2()).toBe(null)
})

test("Should not change array reference", () => {
    s.setState({ ...s.state, Array: [{ $type: "t2", Id: 1 }] })
    s.doUpdateNow()

    const arrayObs = vm.Array
    const arrayObj = vm.Array()

    let changed = false
    arrayObs.subscribe(() => changed = true)

    s.setState({ ...s.state, Array: [{ $type: "t2", Id: 2 }] })
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
    arrayObs.subscribe(() => changed = true, null, "beforeChange")

    s.setState({ ...s.state, Array: [{ $type: "t2", Id: 3 }, { $type: "t2", Id: 4 }] })
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
    s.update(x => ({ ...x, Int: x.Int + 1 }))
    s.doUpdateNow()
    expect(vm.Int()).toBe(746)
})

test("Propagate knockout object assignment", () => {

    vm.Inner({
        $type: ko.observable("t3"),
        P1: ko.observable(4),
        P2: ko.observable(5),
        P3: 4
    })

    expect(s.state.Inner).toStrictEqual({ $type: "t3", P1: 4, P2: 5, P3: 4, P4: null })
    s.doUpdateNow()
    expect(vm.Inner().P1()).toBe(4)
    expect(vm.Inner().P2()).toBe(5)
})

test("Propagate knockout array assignment", () => {

    vm.ArrayWillBe([
        ko.observable({
            $type: ko.observable("t5"),
            B: ko.observable("hmm")
        })
    ])

    expect(s.state.ArrayWillBe).toStrictEqual([{ $type: "t5", B: "hmm" }])
    s.doUpdateNow()
    expect(vm.ArrayWillBe()[0]().B()).toBe("hmm")
    vm.ArrayWillBe()[0]().B("hmm2")
    expect(s.state.ArrayWillBe).toStrictEqual([{ $type: "t5", B: "hmm2" }])
})

test("Propagate Date assignment", () => {
    const val = new Date(2000, 3, 3, 3, 3, 3)
    vm.DateTime(val)

    // The date gets converted to DotVVM serialized date format
    expect(s.state.DateTime).toBe(serializeDate(val, false))
    s.doUpdateNow()
    expect(vm.DateTime()).toBe(serializeDate(val, false))
})

test("Serialized computed updates on changes", () => {
    if (ko.options.deferUpdates) {
        // This test won't work this way (i.e. synchronously) with deferUpdate
        return
    }

    const computed = ko.pureComputed(() => serialize(vm))

    let lastValue = null
    computed.subscribe(val => lastValue = val.Str)
    vm.Str("a")
    expect(lastValue).toBe("a")
    s.setState({ ...s.state, Str: "b" })
    s.doUpdateNow()
    expect(lastValue).toBe("b")
})

test("lastSetError flag", () => {

    // modify value using observable setter
    vm.Int(1);  // valid
    expect(vm.Int[lastSetErrorSymbol]).toBeUndefined();
    expect(() => vm.Int(null)).toThrow();  // invalid
    expect(vm.Int[lastSetErrorSymbol]).toBeDefined();
    vm.Int(2);  // valid
    expect(vm.Int[lastSetErrorSymbol]).toBeUndefined();
    expect(() => vm.Int([])).toThrow();  // invalid
    expect(vm.Int[lastSetErrorSymbol]).toBeDefined();

    // changing state from state manager should reset the flag (if the value is different)
    s.patchState({ Int: 1 });
    s.doUpdateNow();
    expect(vm.Int[lastSetErrorSymbol]).toBeUndefined();

})

test("lastSetError flag - changed back to the original value", () => {

    // modify value using observable setter
    vm.Int(1);  // valid
    expect(vm.Int[lastSetErrorSymbol]).toBeUndefined();
    expect(() => vm.Int(null)).toThrow();  // invalid
    expect(vm.Int[lastSetErrorSymbol]).toBeDefined();

    // changing state from state manager should not reset the flag (if the actual value was not changed)
    s.patchState({ Int: 1 });
    s.doUpdateNow();
    expect(vm.Int[lastSetErrorSymbol]).toBeDefined();

})

test("lastSetError flag - triggers observable change even if the value hasn't really changed", () => {
    let changes = 0;
    const subscription = vm.Int.subscribe(() => changes++);
    try {
        vm.Int(2);
        s.doUpdateNow();
        expect(changes).toEqual(1);

        try {
            vm.Int("aaa");
        } catch { }
        s.doUpdateNow();
        expect(changes).toEqual(2);

        vm.Int(2);
        s.doUpdateNow();
        expect(changes).toEqual(3);

        try {
            vm.Int("aaa");
        } catch { }
        s.doUpdateNow();
        expect(changes).toEqual(4);

        s.patchState({ Int: 5 });
        s.doUpdateNow();
        expect(changes).toEqual(5);
    } finally {
        subscription.dispose();
    }
});

test("coercion happens before assigning to the observable", () => {

    vm.Int(1);
    s.doUpdateNow();
    expect(vm.Int()).toEqual(1);

    vm.Int("3");
    expect(vm.Int()).toEqual(3);
    s.doUpdateNow();
    expect(vm.Int()).toEqual(3);

    expect(() => vm.Int({})).toThrow();

})

test("coercion on assigning to observable doesn't unwrap objects", () => {

    deserialize({
        P1: 13,
        P3: 1
    }, vm.Inner, true);
    expect(vm.Inner().P1).observable();
    expect(vm.Inner().P2).observable();
    expect(vm.Inner().P3).observable();
    expect(vm.Inner().P4).observable();
    const oldInnerValue = vm.Inner();
    const oldInnerP1 = vm.Inner().P1;
    s.doUpdateNow();
    expect(vm.Inner()).toEqual(oldInnerValue);
    expect(vm.Inner().P1).toEqual(oldInnerP1);

    deserialize([
        { Id: 5 }
    ], vm.Array, true);
    expect(vm.Array()[0]).observable();
    expect(vm.Array()[0]().Id).observable();
    const oldArrayValue = vm.Array();
    const oldArrayFirstObjectId = vm.Array()[0]().Id;
    s.doUpdateNow();
    expect(vm.Array()).toEqual(oldArrayValue);
    expect(vm.Array()[0]().Id).toEqual(oldArrayFirstObjectId);
})

test("state on the observable", () => {

    vm.Inner({ P1: 1, P3: 2 });
    s.doUpdateNow();
    const state = vm.Inner.state;
    expect(state).toBeDefined();
    expect(state.P1).toEqual(1);
    expect(state.P2).toBeNull();
    expect(state.P3).toEqual(2);
    expect(state.P4).toBeNull();

    vm.Array([
        { Id: 1 },
        { Id: 3 }
    ]);
    s.doUpdateNow();
    const state2 = vm.Array.state;
    expect(Array.isArray(state2)).toBeTruthy();
    expect(state2.length).toEqual(2);
    expect(state2[0].Id).toEqual(1);
    expect(state2[1].Id).toEqual(3);
})

test("patchState on the observable", () => {

    vm.Inner({ P1: 1, P3: 2 });
    s.doUpdateNow();
    vm.Inner.patchState({ P1: 10 });
    const state = vm.Inner.state;
    expect(state).toBeDefined();
    expect(state.P1).toEqual(10);
    expect(state.P2).toBeNull();
    expect(state.P3).toEqual(2);
    expect(state.P4).toBeNull();

    vm.Array([
        { Id: 1 },
        { Id: 3 }
    ]);
    s.doUpdateNow();
    vm.Array.patchState([{}, { Id: 20 }, { Id: 5 }])
    const state2 = vm.Array.state;
    expect(Array.isArray(state2)).toBeTruthy();
    expect(state2.length).toEqual(3);
    expect(state2[0].Id).toEqual(1);
    expect(state2[1].Id).toEqual(20);
    expect(state2[2].Id).toEqual(5);
})

test("setState on the observable", () => {

    vm.Inner({ P1: 1, P3: 2 });
    s.doUpdateNow();
    vm.Inner.setState({ P1: 10, P3: 10, P4: 18 });
    const state = vm.Inner.state;
    expect(state).toBeDefined();
    expect(state.P1).toEqual(10);
    expect(state.P2).toBeNull();
    expect(state.P3).toEqual(10);
    expect(state.P4).toEqual(18);

    vm.Array([
        { Id: 1 },
        { Id: 3 }
    ]);
    s.doUpdateNow();
    vm.Array.setState([{ Id: 0 }])
    const state2 = vm.Array.state;
    expect(Array.isArray(state2)).toBeTruthy();
    expect(state2.length).toEqual(1);
    expect(state2[0].Id).toEqual(0);
})

test("push on observable array - automatic coercion happens", () => {

    vm.Array([
        { Id: 1 },
        { Id: 3 }
    ]);
    s.doUpdateNow();
    expect(vm.Array().length).toEqual(2);
    vm.Array.push({ Id: 4 });
    expect(vm.Array().length).toEqual(3);

    expect(vm.Array()[2]).observable();
    expect(vm.Array()[2]().Id).observable();
    expect(vm.Array()[2]().Id()).toEqual(4);
    expect(vm.Array()[2]().$type()).toEqual("t2");

    const oldValue = vm.Array()[2];
    s.doUpdateNow();
    expect(vm.Array()[2]).toEqual(oldValue);

})

test("update of element in array that stops existing", () => {

    vm.Array([
        { Id: 1 },
        { Id: 3 }
    ])
    expect(vm.Array.state.length).toEqual(2)
    const a = vm.Array()[1]()
    a.Id(10)
    expect(vm.Array.state[1].Id).toEqual(10)
    vm.Array([ { Id: 1 } ])
    expect(vm.Array.state.length).toEqual(1)
    a.Id(10)
    expect(vm.Array.state.length).toEqual(1)
    expect(vm.Array.state[0]).toEqual({ $type: "t2", Id: 1 })

})
