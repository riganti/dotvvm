import { initDotvvm } from "./helper";
import dotvvm from '../dotvvm-root'
import { getStateManager } from "../dotvvm-base";
import { StateManager } from "../state-manager";
import { serialize } from "../serialization/serialize";

ko.options.deferUpdates = true

require('./stateManagement.test')



const vm = dotvvm.viewModels.root.viewModel as any
const s = getStateManager() as StateManager<any>


test("Sanity check: deferUpdates actually does something", () => {

    expect(ko.options.deferUpdates).toBe(true)
    const o = ko.observable(0)
    let dirtyCalled = 0,
        changedCalled = 0,
        beforeChangeCalled = 0
    o.subscribe(() => dirtyCalled++, null, "dirty")
    o.subscribe(() => beforeChangeCalled++, null, "beforeChange")
    o.subscribe(() => changedCalled++, null, "change")

    o(1)
    expect([dirtyCalled, changedCalled, beforeChangeCalled]).toStrictEqual([1, 0, 1])

    o(2)
    expect([dirtyCalled, changedCalled, beforeChangeCalled]).toStrictEqual([2, 0, 1])
})

