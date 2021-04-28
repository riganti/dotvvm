import { orderBy, orderByDesc } from './sortingHelper'

export {
    add,
    addOrUpdate,
    addRange,
    all,
    any,
    clear,
    distinct,
    firstOrDefault,
    insert,
    insertRange,
    lastOrDefault,
    max,
    min,
    orderBy,
    orderByDesc,
    removeAll,
    removeAt,
    removeFirst,
    removeLast,
    removeRange,
    reverse
}

function add<T>(source: T[], element: T, observable: any): void {
    let copy = source.map(item => ko.unwrap(item));
    copy.push(ko.unwrap(element));

    observable.setState(copy);
}

function addOrUpdate<T>(source: T[], element: T, matcher: (e: T) => boolean, updater: (e: T) => T, observable: any): void {
    let copy = source.map(item => ko.unwrap(item));
    let found = false;

    for (let i = 0; i < source.length; i++) {
        if (!matcher(source[i])) {
            continue;
        }

        found = true;
        copy[i] = updater(copy[i]);
    }

    if (!found) {
        copy.push(element);
    }

    observable.setState(copy);
}

function addRange<T>(source: T[], elements: T[], observable: any): void {
    let copy = source.map(item => ko.unwrap(item));
    for (let i = 0; i < elements.length; i++) {
        copy.push(ko.unwrap(elements[i]));
    }

    observable.setState(copy);
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

function clear<T>(observable: any): void {
    observable.setState([]);
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

function insert<T>(items: T[], index: number, element: T, observable: any): void {
    let copy = items.map(item => ko.unwrap(item));
    copy.splice(index, 0, element);

    observable.setState(copy);
}

function insertRange<T>(items: T[], index: number, elements: T[], observable: any): void {
    let copy = items.map(item => ko.unwrap(item));
    copy.splice(index, 0, ...elements.map(element => ko.unwrap(element)));

    observable.setState(copy);
}

function lastOrDefault<T>(source: T[], predicate: (s: T) => boolean): T | null {
    for (let i = source.length - 1; i >= 0; i--) {
        if (predicate(source[i])) {
            return source[i];
        }
    }
    return null;
}

function max<T>(source: T[], selector: (item: T) => number): number {
    if (source.length === 0)
        throw new Error("Source is empty! Max operation cannot be performed.");
    if (source.length == 1)
        return selector(source[0]);
    let max = selector(source[0]);
    for (let i = 1; i < source.length; i++) {
        let v = selector(source[i]);
        if (v > max)
            max = v;
    }
    return max;
}

function min<T>(source: T[], selector: (item: T) => number): number {
    if (source.length === 0)
        throw new Error("Source is empty! Min operation cannot be performed.");
    if (source.length == 1)
        return selector(source[0]);
    let min = selector(source[0]);
    for (let i = 1; i < source.length; i++) {
        let v = selector(source[i]);
        if (v < min)
            min = v;
    }
    return min;
}

function removeAt<T>(source: T[], index: number, observable: any): void {
    let copy = source.map(item => ko.unwrap(item));
    copy.splice(index, 1);

    observable.setState(copy);
}

function removeAll<T>(source: T[], predicate: (s: T) => boolean, observable: any): void {
    let copy = source.map(item => ko.unwrap(item));
    for (let i = 0; i < copy.length; i++) {
        if (predicate(copy[i])) {
            copy.splice(i, 1);
            i--;
        }
    }

    observable.setState(copy);
}

function removeRange<T>(source: T[], index: number, length: number, observable: any): void {
    let copy = source.map(item => ko.unwrap(item));
    copy.splice(index, length);

    observable.setState(copy);
}

function removeFirst<T>(source: T[], predicate: (s: T) => boolean, observable: any): void {
    let copy = source.map(item => ko.unwrap(item));
    for (let i = 0; i < copy.length; i++) {
        if (predicate(copy[i])) {
            copy.splice(i, 1);
            break;
        }
    }

    observable.setState(copy);
}

function removeLast<T>(source: T[], predicate: (s: T) => boolean, observable: any): void {
    let copy = source.map(item => ko.unwrap(item));
    for (let i = copy.length - 1; i >= 0; i--) {
        if (predicate(copy[i])) {
            copy.splice(i, 1);
            break;
        }
    }

    observable.setState(copy);
}

function reverse<T>(source: T[], observable: any): void {
    let copy = source.map(item => ko.unwrap(item));
    copy.reverse();

    observable.setState(copy);
}
