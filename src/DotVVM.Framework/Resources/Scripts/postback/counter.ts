var postBackCounter: number = 0;

export function backUpPostBackCounter(): number {
    return ++postBackCounter;
}

export function isPostBackStillActive(currentPostBackCounter: number): boolean {
    return postBackCounter === currentPostBackCounter;
}

