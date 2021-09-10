import dotvvm from '../dotvvm-root'
import { initDotvvm } from './helper'

test('DotVVM object seems to exist', () => {
    expect(dotvvm.isPostbackRunning()).toBe(false)
})

test('DotVVM can be initialized', () => {
    initDotvvm({
        viewModel: {
            $type: "t1",
            MyProperty: 1
        },
        typeMetadata: {
            t1: {
                type: "object",
                properties: {
                    MyProperty: {
                        type: "Int32"
                    }
                }
            }
        }
    })
})

test("get state", () => {
    const state = dotvvm.state as any
    expect(state.MyProperty).toBe(1)
})

test("read legacy viewModel", () => {
    const vm = dotvvm.viewModels.root.viewModel as any
    expect(vm.MyProperty).observable()
    expect(vm.MyProperty()).toBe(1)
})

test("write legacy viewModel", () => {
    const vm = dotvvm.viewModels.root.viewModel as any
    vm.MyProperty(52)
    expect(vm.MyProperty()).toBe(52)
})

test("call newState event", done => {
    const vm = dotvvm.viewModels.root.viewModel as any
    dotvvm.events.newState.subscribe((newState: any) => {
        try {
            expect(newState.MyProperty).toBe(52);
            done();
        } catch (error) {
            done(error);
        }
    })
    vm.MyProperty(52)
    expect(vm.MyProperty()).toBe(52)
})
