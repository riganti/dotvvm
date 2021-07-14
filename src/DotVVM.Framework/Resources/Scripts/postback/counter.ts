var postBackCounter: number = 0;

export function backUpPostBackCounter(): number {
    return ++postBackCounter;
}
