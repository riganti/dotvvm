import timer from "../binding-handlers/timer"

const interval = 1000
let elements: HTMLElement[]

beforeEach(() => {
    jest.useFakeTimers()
    elements = []
})

afterEach(() => {
    elements.forEach(element => ko.cleanNode(element))
    jest.useRealTimers()
})

async function advanceTime(milliseconds: number) {
    await Promise.resolve()
    jest.advanceTimersByTime(milliseconds)
    await Promise.resolve()
}

function createTimer(initiallyEnabled = true) {
    const element = document.createElement("div")
    elements.push(element)
    const enabled = ko.observable(initiallyEnabled)
    const command = jest.fn(() => Promise.resolve({} as DotvvmAfterPostBackEventArgs))
    timer["dotvvm-timer"].init(element, () => ({ interval, enabled, command }))
    return { element, enabled, command }
}

function pendingCommand() {
    let complete!: () => void
    const promise = new Promise<DotvvmAfterPostBackEventArgs>(resolve => {
        complete = () => resolve({} as DotvvmAfterPostBackEventArgs)
    })
    return { promise, complete }
}

test("a disabled timer starts only after being enabled", async () => {
    const { enabled, command } = createTimer(false)
    await advanceTime(3 * interval)
    expect(command).not.toHaveBeenCalled()

    enabled(true)
    await advanceTime(interval - 1)
    expect(command).not.toHaveBeenCalled()
    await advanceTime(1)
    expect(command).toHaveBeenCalledTimes(1)
})

test("disabling cancels a pending timeout and allows restarting", async () => {
    const { enabled, command } = createTimer()
    enabled(false)
    await advanceTime(3 * interval)
    expect(command).not.toHaveBeenCalled()

    enabled(true)
    await advanceTime(interval)
    expect(command).toHaveBeenCalledTimes(1)
})

test("the next interval starts after the command completes", async () => {
    const { command } = createTimer()
    const pending = pendingCommand()
    command.mockReturnValueOnce(pending.promise)

    await advanceTime(3 * interval)
    expect(command).toHaveBeenCalledTimes(1)

    pending.complete()
    await advanceTime(0)
    await advanceTime(interval - 1)
    expect(command).toHaveBeenCalledTimes(1)
    await advanceTime(1)
    expect(command).toHaveBeenCalledTimes(2)
})

test.each(["disabled", "disposed"])("a timer %s during a command does not restart on completion", async action => {
    const { element, enabled, command } = createTimer()
    const pending = pendingCommand()
    command.mockReturnValueOnce(pending.promise)

    await advanceTime(interval)
    expect(command).toHaveBeenCalledTimes(1)

    if (action === "disabled") enabled(false)
    else ko.cleanNode(element)
    pending.complete()

    await advanceTime(3 * interval)
    expect(command).toHaveBeenCalledTimes(1)
    expect(jest.getTimerCount()).toBe(0)
})

test("re-enabling during a command does not create a second timeout chain", async () => {
    const { enabled, command } = createTimer()
    const pending = pendingCommand()
    command.mockReturnValueOnce(pending.promise)

    await advanceTime(interval)
    enabled(false)
    enabled(true)

    await advanceTime(interval / 2)
    pending.complete()
    await advanceTime(interval - 1)
    expect(command).toHaveBeenCalledTimes(1)
    await advanceTime(1)
    expect(command).toHaveBeenCalledTimes(2)

    await advanceTime(interval / 2)
    expect(command).toHaveBeenCalledTimes(2)
    expect(jest.getTimerCount()).toBe(1)
})

test("repeated toggling during slow commands keeps at most one command in flight", async () => {
    const { enabled, command } = createTimer()
    const first = pendingCommand()
    const second = pendingCommand()
    command.mockReturnValueOnce(first.promise).mockReturnValueOnce(second.promise)

    await advanceTime(interval)
    for (let i = 0; i < 3; i++) {
        enabled(false)
        enabled(true)
        await advanceTime(3 * interval)
        expect(command).toHaveBeenCalledTimes(1)
    }

    first.complete()
    await advanceTime(interval - 1)
    expect(command).toHaveBeenCalledTimes(1)
    await advanceTime(1)
    expect(command).toHaveBeenCalledTimes(2)

    enabled(false)
    enabled(true)
    await advanceTime(3 * interval)
    expect(command).toHaveBeenCalledTimes(2)

    // The most recent Enabled value also applies when the command finishes.
    enabled(false)
    second.complete()
    await advanceTime(3 * interval)
    expect(command).toHaveBeenCalledTimes(2)
    expect(jest.getTimerCount()).toBe(0)
})
