import { resetPostBackCounter } from './counter';
import { postbackQueues } from './queue';

var postbacksDisabled = false;

export const isSpaNavigationRunning = ko.observable(false);

export function arePostbacksDisabled() {
    return postbacksDisabled;
}

export function disablePostbacks() {
    resetPostBackCounter();
    for (const q in postbackQueues) {
        if (postbackQueues.hasOwnProperty(q)) {
            let postbackQueue = postbackQueues[q];
            postbackQueue.queue.length = 0;
            postbackQueue.noRunning = 0;
        }
    }

    // disable all other postbacks
    // but not in SPA mode, since we'll need them for the next page
    // and user might want to try another postback in case this navigation hangs
    if (!compileConstants.isSpa) {
        postbacksDisabled = true;
    }
}
