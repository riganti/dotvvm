import { orderBy, orderByDesc } from './sortingHelper'

export {
    add,
    addOrUpdate,
    addRange,
    clear,
    distinct,
    contains,
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
    reverse,
    setItem
}

function add<T>(observable: any, element: T): void {
    let array = [...observable.state, element];
    observable.setState(array);
}

function addOrUpdate<T>(observable: any, element: T, matcher: (e: T) => boolean, updater: (e: T) => T): void {
    let array = Array.from<T>(observable.state);
    let found = false;

    for (let i = 0; i < array.length; i++) {
        if (!matcher(array[i])) {
            continue;
        }

        found = true;
        array[i] = updater(array[i]);
    }

    if (!found) {
        array.push(element);
    }

    observable.setState(array);
}

function addRange<T>(observable: any, elements: T[]): void {
    let array = Array.from<T>(observable.state);
    for (let i = 0; i < elements.length; i++) {
        array.push(ko.unwrap(elements[i]));
    }

    observable.setState(array);
}

function clear(observable: any): void {
    observable.setState([]);
}

function distinct<T>(array: T[]): T[] {
    return Array.from(new Set(array.map(e => ko.unwrap(e))));
}

function contains<T>(array: T[], value: T): boolean {
    return array.map(e => ko.unwrap(e)).includes(value);
}

function firstOrDefault<T>(array: T[], predicate: (s: T) => boolean): T | null {
    for (const item of array) {
        const itemUnwrapped = ko.unwrap(item)
        if (predicate(itemUnwrapped)) {
            return itemUnwrapped
        }
    }
    return null;
}

function insert<T>(observable: any, index: number, element: T): void {
    let array = Array.from<T>(observable.state);
    array.splice(index, 0, element);
    observable.setState(array);
}

function insertRange<T>(observable: any, index: number, elements: T[]): void {
    let array = Array.from<T>(observable.state);
    array.splice(index, 0, ...elements.map(element => ko.unwrap(element)));
    observable.setState(array);
}

function lastOrDefault<T>(array: T[], predicate: (s: T) => boolean): T | null {
    for (let i = array.length - 1; i >= 0; i--) {
        const itemUnwrapped = ko.unwrap(array[i])
        if (predicate(itemUnwrapped)) {
            return itemUnwrapped
        }
    }
    return null;
}

function max<T>(array: T[], selector: (item: T) => number, throwIfEmpty: boolean): number | null {
    if (array.length === 0) {
        if (throwIfEmpty) {
            throw new Error("Source is empty! Max operation cannot be performed.");
        }
        return null;
    }

    let max = selector(array[0]);
    for (let i = 1; i < array.length; i++) {
        let v = selector(array[i]);
        if (v > max)
            max = v;
    }
    return max;
}

function min<T>(array: T[], selector: (item: T) => number, throwIfEmpty: boolean): number | null {
    if (array.length === 0) {
        if (throwIfEmpty) {
            throw new Error("Source is empty! Min operation cannot be performed.");
        }
        return null;
    }

    let min = selector(array[0]);
    for (let i = 1; i < array.length; i++) {
        let v = selector(array[i]);
        if (v < min)
            min = v;
    }
    return min;
}

function removeAt<T>(observable: any, index: number): void {
    let array = Array.from<T>(observable.state);
    array.splice(index, 1);
    observable.setState(array);
}

function removeAll<T>(observable: any, predicate: (s: T) => boolean): void {
    let array = Array.from<T>(observable.state);
    for (let i = 0; i < array.length; i++) {
        if (predicate(array[i])) {
            array.splice(i, 1);
            i--;
        }
    }

    observable.setState(array);
}

function removeRange<T>(observable: any, index: number, length: number): void {
    let array = Array.from<T>(observable.state);
    array.splice(index, length);
    observable.setState(array);
}

function removeFirst<T>(observable: any, predicate: (s: T) => boolean): void {
    let array = Array.from<T>(observable.state);
    for (let i = 0; i < array.length; i++) {
        if (predicate(array[i])) {
            array.splice(i, 1);
            break;
        }
    }

    observable.setState(array);
}

function removeLast<T>(observable: any, predicate: (s: T) => boolean): void {
    let array = Array.from<T>(observable.state);
    for (let i = array.length - 1; i >= 0; i--) {
        if (predicate(array[i])) {
            array.splice(i, 1);
            break;
        }
    }

    observable.setState(array);
}

function reverse<T>(observable: any): void {
    let array = Array.from<T>(observable.state);
    array.reverse();
    observable.setState(array);
}

function setItem<T>(observable: any, index: number, value: T): void {
    let array = Array.from<T>(observable.state);
    if (index < 0 || index >= array.length)
        throw Error("Index out of range!");

    array[index] = value;
    observable.setState(array);
}
