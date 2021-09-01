import { DotvvmEvent } from "../events"


test("subscribe works", () => {
    const e = new DotvvmEvent<{}>("test", false)
    let log = 0
    const handler = (a: any) => { log++ }
    e.subscribe(handler)

    expect(log).toBe(0)

    e.trigger({})

    expect(log).toBe(1)

    e.trigger({})

    expect(log).toBe(2)
})

test("subscribeOnce works", () => {
    const e = new DotvvmEvent<{}>("test", false)
    let log = 0
    const handler = (a: any) => { log++ }
    e.subscribeOnce(handler)

    expect(log).toBe(0)

    e.trigger({})

    expect(log).toBe(1)

    e.trigger({})

    expect(log).toBe(1)
})

test("unsubscribe works", () => {
    const e = new DotvvmEvent<{}>("test", false)
    let log1 = 0
    const handler1 = (a: any) => { log1++ }
    let log2 = 0
    const handler2 = (a: any) => { log2++ }

    e.subscribe(handler1)
    e.subscribe(handler2)

    expect(log1).toBe(0)
    expect(log2).toBe(0)

    e.trigger({})

    expect(log1).toBe(1)
    expect(log2).toBe(1)

    e.unsubscribe(handler2)
    e.trigger({})

    expect(log1).toBe(2)
    expect(log2).toBe(1)

    e.unsubscribe(handler1)
    e.trigger({})

    expect(log1).toBe(2)
    expect(log2).toBe(1)
})

test("subscribe with history works", () => {
    const e = new DotvvmEvent<{}>("test", true)
    let log = 0
    const handler = (a: any) => { log++ }

    e.trigger({})
    e.trigger({})
    e.subscribe(handler)

    expect(log).toBe(2)
})

test("subscribe and subscribeOnce complex test", () => {
    const e = new DotvvmEvent<{}>("test", false)

    let log1 = 0
    const handler1 = (a: any) => { log1++ }
    let log2 = 0
    const handler2 = (a: any) => { log2++ }
    let log3 = 0
    const handler3 = (a: any) => { log3++ }

    e.subscribeOnce(handler1)
    e.subscribe(handler2)
    e.subscribeOnce(handler3)
    expect(e["handlers"].length).toBe(3)

    e.trigger({})
    expect(log1).toBe(1)
    expect(log2).toBe(1)
    expect(log3).toBe(1)
    expect(e["handlers"].length).toBe(1)

    e.trigger({})
    expect(log1).toBe(1)       // it was unsubscribed automatically
    expect(log2).toBe(2)
    expect(log3).toBe(1)       // it was unsubscribed automatically

    e.unsubscribe(handler2)
    expect(e["handlers"].length).toBe(0)
})


