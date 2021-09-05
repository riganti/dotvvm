import { fc, waitForEnd } from "./helper";
import dotvvm from '../dotvvm-root'
import { getStateManager } from "../dotvvm-base";
import { StateManager } from "../state-manager";

require('./stateManagement.data')

const vm = dotvvm.viewModels.root.viewModel as any
const s = getStateManager() as StateManager<any>
s.doUpdateNow()

test("Stress test - simple increments", async () => {
    jest.setTimeout(120_000);

    // watchEvents()

    await fc.assert(fc.asyncProperty(
        fc.integer(1, 100),
        fc.scheduler(),
        async (steps, scheduler) => {
            vm.Int(0)
            expect(vm.Int()).toBe(0)
            s.doUpdateNow()
            expect(s.state.Int).toBe(0);

            await waitForEnd([
                (async () => {
                    for (let i = 0; i < steps / 8 + 1; i++) {
                        await scheduler.schedule(Promise.resolve(), `Sync ${i}`)
                        s.doUpdateNow()
                    }
                })(),
                (async () => {
                    for (let i = 0; i < steps; i++) {
                        await scheduler.schedule(Promise.resolve(), `Inc ${i} + 1`)
                        expect(vm.Int()).toBe(i)
                        vm.Int(vm.Int() + 1)
                        expect(vm.Int()).toBe(i + 1)
                    }
                })()],
                scheduler,
                () => {
                    expect(vm.Int()).toBe(s.state.Int)
                })
        }
    ), { timeout: 20000 })
})

test("Stress test - simple increments with postbacks in background", async () => {
    jest.setTimeout(120_000);

    // watchEvents()

    await fc.assert(fc.asyncProperty(
        fc.integer(1, 100),
        fc.integer(1, 100),
        fc.scheduler(),
        async (steps, postbacks, scheduler) => {
            vm.Int(0)
            expect(vm.Int()).toBe(0)
            s.doUpdateNow()
            expect(s.state.Int).toBe(0);

            await waitForEnd([
                (async () => {
                    for (let i = 0; i < steps / 8 + 1; i++) {
                        await scheduler.schedule(Promise.resolve(), `Sync ${i}`)
                        s.doUpdateNow()
                    }
                })(),
                (async () => {
                    for (let i = 0; i < postbacks; i++) {
                        await scheduler.schedule(Promise.resolve(), `Postback ${i}`)
                        s.setState(JSON.parse(JSON.stringify(s.state)));
                    }
                })(),
                (async () => {
                    for (let i = 0; i < steps; i++) {
                        await scheduler.schedule(Promise.resolve(), `Inc ${i} + 1`)
                        expect(vm.Int()).toBe(i)
                        vm.Int(vm.Int() + 1)
                        expect(vm.Int()).toBe(i + 1)
                    }
                })()],
                scheduler,
                () => {
                    expect(vm.Int()).toBe(s.state.Int)
                })
        }
    ), { timeout: 20000 })
})
