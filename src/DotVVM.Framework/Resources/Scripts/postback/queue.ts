export const updateProgressChangeCounter = ko.observable(0);
export const postbackQueues: {
    [name: string]: {
        queue: Array<(() => void)>,
        runningPostbacksCount: number
    }
} = {};

export function getPostbackQueue(name = "default") {
    if (!postbackQueues[name]) {
        postbackQueues[name] = { queue: [], runningPostbacksCount: 0 };
    }

    const entry = postbackQueues[name];
    return {
        queue: entry.queue,
        noRunning: entry.runningPostbacksCount
    }
}

export function enterActivePostback(queueName: string) {
    const queue = getPostbackQueue(queueName);
    queue.noRunning++;
    updateProgressChangeCounter(updateProgressChangeCounter() + 1);
}

export function leaveActivePostback(queueName: string) {
    const queue = getPostbackQueue(queueName);
    queue.noRunning--;
    updateProgressChangeCounter(updateProgressChangeCounter() - 1);
}

export function runNextInQueue(queueName: string) {
    const queue = getPostbackQueue(queueName);
    if (queue.queue.length > 0) {
        const callback = queue.queue.shift()!;
        window.setTimeout(callback, 0);
    }
}
