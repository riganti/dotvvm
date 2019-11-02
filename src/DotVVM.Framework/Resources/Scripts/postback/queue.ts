var updateProgressChangeCounter = ko.observable(0);
var postbackQueues: { 
    [name: string]: { 
        queue: (() => void)[], 
        runningPostbacksCount: number
    }
} = {};

export function getPostbackQueue(name = "default") {
    if (!postbackQueues[name]) {
        postbackQueues[name] = { queue: [], runningPostbacksCount: 0 };
    }

    let entry = postbackQueues[name];
    return {
        queue: entry.queue,
        noRunning: entry.runningPostbacksCount
    }
}

export function enterActivePostback(queueName: string) {
    let queue = getPostbackQueue(queueName);
    queue.noRunning++;
    updateProgressChangeCounter(updateProgressChangeCounter() + 1);
}

export function leaveActivePostback(queueName: string) {
    let queue = getPostbackQueue(queueName);
    queue.noRunning--;
    updateProgressChangeCounter(updateProgressChangeCounter() - 1);
}

export function runNextInQueue(queueName: string) {
    let queue = getPostbackQueue(queueName);
    if (queue.queue.length > 0) {
        const callback = queue.queue.shift()!;
        window.setTimeout(callback, 0);
    }
}
