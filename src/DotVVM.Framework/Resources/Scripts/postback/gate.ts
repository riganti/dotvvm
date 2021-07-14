import { backUpPostBackCounter } from './counter';

let postbacksDisabled = false
let lastDisabledPostback = -1

export function isPostbackDisabled(postbackId: number) {
    return postbacksDisabled || lastDisabledPostback >= postbackId
}

export function enablePostbacks() {
    postbacksDisabled = false
    lastDisabledPostback = backUpPostBackCounter()
}

export function disablePostbacks() {
    postbacksDisabled = true
}
