export const updateProgressChangeCounter = ko.observable(0);
export const postbackQueues: {
    [name: string]: {
        queue: Array<(() => void)>,
        noRunning: number
    }
} = {};

export function getPostbackQueue(name = "default") {
    if (!postbackQueues[name]) {
        postbackQueues[name] = { queue: [], noRunning: 0 };
    }

    const entry = postbackQueues[name];
    return entry;
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
