import dotvvm from '../dotvvm-root'
import { keys } from '../utils/objects'

type EventHistoryEntry = { 
    event: string, 
    args: any 
}

const eventHistory: EventHistoryEntry[] = [];

export function initDotvvm(viewModel: any, culture: string = "en-US") {
    const input = window.document.createElement("input")
    input.value = JSON.stringify(viewModel)
    input.id = "__dot_viewmodel_root"
    document.body.appendChild(input)

    dotvvm.init(culture)

}

export function watchEvents(consoleOutput: boolean = true) {
    const handlers: any = {}
    for (const event of keys(dotvvm.events)) {
        if ("subscribe" in (dotvvm.events as any)[event]) {
            function h(args: any) {
                if (consoleOutput) {
                    console.debug("Event " + event, args.postbackId ?? "")
                }
                eventHistory.push({ event, args })
            }
            (dotvvm.events as any)[event].subscribe(h)
            handlers[event] = h
        }
    }

    return () => {
        for (const event of keys(handlers)) {
            (dotvvm.events as any)[event].unsubscribe(handlers[event])
        }
        eventHistory.length = 0;
    }
}

export function getEventHistory() {
    return eventHistory;
}
