import { orderBy, orderByDesc } from './sortingHelper'

export {
    all,
    any,
    clear,
    distinct,
    firstOrDefault,
    forEach,
    lastOrDefault,
    orderBy,
    orderByDesc,
    remove,
    removeFirst
}

function any<T>(source: T[], predicate: (s: T) => boolean): boolean {
    return firstOrDefault(source, predicate) != null;
}

function all<T>(source: T[], predicate: (s: T) => boolean) {
    for (let i = 0; i < source.length; i++) {
        if (!predicate(source[i])) {
            return false;
        }
    }
    return true;
}

function clear<T>(source: T[]): void {
    source.splice(0, source.length);
}

function distinct<T>(source: T[]): T[] {
    let r = [];
    for (let i = 0; i < source.length; i++) {
        let found = false;
        for (var j = 0; j < r.length; j++) {
            if (r[j] == source[i]) {
                found = true;
                break;
            }
        }
        if (found)
            continue;
        r.push(source[i]);
    }
    return r;
}

function firstOrDefault<T>(source: T[], predicate: (s: T) => boolean): T | null {
    for (let i = 0; i < source.length; i++) {
        if (predicate(source[i])) {
            return source[i];
        }
    }
    return null;
}

function forEach<T>(items: T[], action: (s: T, i: number) => void): void {
    for (let i = 0; i < items.length; i++) {
        action(items[i], i);
    }
}

function lastOrDefault<T>(source: T[], predicate: (s: T) => boolean): T | null {
    let lastSatisfyingElement = null;
    for (let i = 0; i < source.length; i++) {
        if (predicate(source[i])) {
            lastSatisfyingElement = source[i];
        }
    }
    return lastSatisfyingElement;
}

function remove<T>(source: T[], predicate: (s: T) => boolean) {
    for (let i = 0; i < source.length; i++) {
        if (predicate(source[i])) {
            source.splice(i, 1);
            i--;
        }
    }
}

function removeFirst<T>(source: T[], predicate: (s: T) => boolean) {
    for (let i = 0; i < source.length; i++) {
        if (predicate(source[i])) {
            source.splice(i, 1);
            return;
        }
    }
}
