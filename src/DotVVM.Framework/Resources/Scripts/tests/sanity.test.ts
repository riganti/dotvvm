import dotvvm from '../dotvvm-root'
import { initDotvvm } from './helper'

test('DotVVM object seems to exist', () => {
    expect(dotvvm.isPostbackRunning()).toBe(false)
})

test('DotVVM can be initialized', () => {
    initDotvvm({
        viewModel: {
            MyProperty: 1
        }
    })
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
