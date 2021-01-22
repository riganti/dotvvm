import dotvvm from '../dotvvm-root'
import { keys } from '../utils/objects'
import { events as validationEvents } from '../validation/validation'
import fc_types from '../../../node_modules/fast-check/lib/types/fast-check'

export const fc: typeof fc_types = require('fast-check');

export const delay = (time: number) => new Promise((r) => setTimeout(r, time))

export async function waitForEnd<T>(result: Promise<T>[], s: fc_types.Scheduler, assert: () => void) {
    const aggResult = Promise.all(result)

    let done = false
    aggResult.then(_ => done = true, _ => done = true)
    await delay(1)

    while (!done) {
        // console.log(s.report())
        expect(s.count()).toBeGreaterThan(0)
        await s.waitOne()

        assert()

        await delay(1)
    }
    // console.log("Done = ", done, aggResult)

    return await aggResult
}

type EventHistoryEntry = { 
    event: string, 
    args: any 
}

const eventHistory: EventHistoryEntry[] = [];

export function initDotvvm(viewModel: any, culture: string = "en-US") {
    window.compileConstants.isSpa = false;
    
    const input = window.document.createElement("input")
    input.value = JSON.stringify(viewModel)
    input.id = "__dot_viewmodel_root"
    document.body.appendChild(input)

    dotvvm.init(culture)
}

export function initDotvvmWithSpa(viewModel: any, culture: string = "en-US") {
    window.compileConstants.isSpa = true;

    const input = window.document.createElement("input")
    input.value = JSON.stringify(viewModel)
    input.id = "__dot_viewmodel_root"
    document.body.appendChild(input)
    dotvvm.init(culture)
}

export function watchEvents(consoleOutput: boolean = true) {
    const handlers: any = {}
    const allEvents = { ...dotvvm.events, ...validationEvents };
    for (const event of keys(allEvents)) {
        if ("subscribe" in (allEvents as any)[event]) {
            const h = function (args: any): void {
                if (consoleOutput) {
                    console.debug("Event " + event, args.postbackId ?? "")
                }
                eventHistory.push({ event, args })
            };
            (allEvents as any)[event].subscribe(h)
            handlers[event] = h
        }
    }

    return () => {
        for (const event of keys(handlers)) {
            (allEvents as any)[event].unsubscribe(handlers[event])
        }
        eventHistory.length = 0;
    }
}

export function getEventHistory() {
    return eventHistory;
}
