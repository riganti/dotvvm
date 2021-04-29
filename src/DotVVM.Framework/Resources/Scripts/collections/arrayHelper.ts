import { orderBy as orderByImpl, orderByDesc as orderByDescImpl} from './sortingHelper'
type ElementType = string | number | boolean;
const versionSymbol = Symbol("version");

export {
    add,
    addOrUpdate,
    addRange,
    all,
    any,
    clear,
    concat,
    count,
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
    reverse,
    select,
    skip,
    take,
    where
}

function add<T>(observable: any, element: T): void {
    let copy = unwrapArray<T>(observable).map(item => ko.unwrap(item));
    copy.push(element);

    incrementVersion(observable);
    observable.setState(copy);
}

function addOrUpdate<T>(observable: any, element: T, matcher: (e: T) => boolean, updater: (e: T) => T): void {
    let copy = unwrapArray<T>(observable).map(item => ko.unwrap(item));
    let found = false;

    for (let i = 0; i < copy.length; i++) {
        if (!matcher(copy[i])) {
            continue;
        }

        found = true;
        copy[i] = updater(copy[i]);
    }

    if (!found) {
        copy.push(element);
    }

    incrementVersion(observable);
    observable.setState(copy);
}

function addRange<T>(observable: any, elements: T[]): void {
    let copy = unwrapArray<T>(observable).map(item => ko.unwrap(item));
    for (let i = 0; i < elements.length; i++) {
        copy.push(ko.unwrap(elements[i]));
    }

    incrementVersion(observable);
    observable.setState(copy);
}

function any<T>(observable: any, predicate: (s: T) => boolean): boolean {
    // Mutations are already checked inside firstOrDefault
    return firstOrDefault(observable, predicate) != null;
}

function all<T>(observable: any, predicate: (s: T) => boolean): boolean {
    let version = getVersion(observable);
    let result = allImpl(ko.unwrap(observable) as T[], predicate);

    ensureNotMutated(observable, version);
    return result;
}

function allImpl<T>(source: T[], predicate: (s: T) => boolean): boolean {
    for (let i = 0; i < source.length; i++) {
        if (!predicate(source[i])) {
            return false;
        }
    }
    return true;
}

function clear<T>(observable: any): void {
    incrementVersion(observable);
    observable.setState([]);
}

function concat<T>(observable1: any, observable2: any): T[] {
    let version1 = getVersion(observable1);
    let version2 = getVersion(observable2);
    let result = ko.unwrap(observable1).concat(ko.unwrap(observable2));

    ensureNotMutated(observable1, version1);
    ensureNotMutated(observable2, version2);
    return result;
}

function count<T>(observable: any) {
    let version = getVersion(observable);
    let result = ko.unwrap(observable).length;

    ensureNotMutated(observable, version);
    return result;
}

function distinct<T>(observable: any): T[] {
    let version = getVersion(observable);
    let result = distinctImpl(ko.unwrap(observable) as T[]);

    ensureNotMutated(observable, version);
    return result;
}

function distinctImpl<T>(source: T[]): T[] {
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

function firstOrDefault<T>(observable: any, predicate: (s: T) => boolean): T | null {
    let version = getVersion(observable);
    let result = firstOrDefaultImpl(ko.unwrap(observable) as T[], predicate);

    ensureNotMutated(observable, version);
    return result;
}

function firstOrDefaultImpl<T>(source: T[], predicate: (s: T) => boolean): T | null {
    for (let i = 0; i < source.length; i++) {
        if (predicate(source[i])) {
            return source[i];
        }
    }
    return null;
}

function insert<T>(observable: any, index: number, element: T): void {
    let copy = unwrapArray<T>(observable).map(item => ko.unwrap(item));
    copy.splice(index, 0, element);

    incrementVersion(observable);
    observable.setState(copy);
}

function insertRange<T>(observable: any, index: number, elements: T[]): void {
    let copy = unwrapArray<T>(observable).map(item => ko.unwrap(item));
    copy.splice(index, 0, ...elements.map(element => ko.unwrap(element)));

    incrementVersion(observable);
    observable.setState(copy);
}

function lastOrDefault<T>(observable: any, predicate: (s: T) => boolean): T | null {
    let version = getVersion(observable);
    let result = lastOrDefaultImpl(ko.unwrap(observable) as T[], predicate);

    ensureNotMutated(observable, version);
    return result;
}

function lastOrDefaultImpl<T>(source: T[], predicate: (s: T) => boolean): T | null {
    for (let i = source.length - 1; i >= 0; i--) {
        if (predicate(source[i])) {
            return source[i];
        }
    }
    return null;
}

function max<T>(observable: any, selector: (item: T) => number): number {
    let version = getVersion(observable);
    let result = maxImpl(ko.unwrap(observable) as T[], selector);

    ensureNotMutated(observable, version);
    return result;
}

function maxImpl<T>(source: T[], selector: (item: T) => number): number {
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

function min<T>(observable: any, selector: (item: T) => number): number {
    let version = getVersion(observable);
    let result = minImpl(ko.unwrap(observable) as T[], selector);

    ensureNotMutated(observable, version);
    return result;
}

function minImpl<T>(source: T[], selector: (item: T) => number): number {
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

function orderBy<T>(observable: any, selector: (item: T) => ElementType): T[] {
    let version = getVersion(observable);
    let result = orderByImpl(ko.unwrap(observable) as T[], selector);

    ensureNotMutated(observable, version);
    return result;
}

function orderByDesc<T>(observable: any, selector: (item: T) => ElementType): T[] {
    let version = getVersion(observable);
    let result = orderByDescImpl(ko.unwrap(observable) as T[], selector);

    ensureNotMutated(observable, version);
    return result;
}

function removeAt<T>(observable: any, index: number): void {
    let copy = unwrapArray<T>(observable).map(item => ko.unwrap(item));
    copy.splice(index, 1);

    incrementVersion(observable);
    observable.setState(copy);
}

function removeAll<T>(observable: any, predicate: (s: T) => boolean): void {
    let copy = unwrapArray<T>(observable).map(item => ko.unwrap(item));
    for (let i = 0; i < copy.length; i++) {
        if (predicate(copy[i])) {
            copy.splice(i, 1);
            i--;
        }
    }

    incrementVersion(observable);
    observable.setState(copy);
}

function removeRange<T>(observable: any, index: number, length: number): void {
    let copy = unwrapArray<T>(observable).map(item => ko.unwrap(item));
    copy.splice(index, length);

    incrementVersion(observable);
    observable.setState(copy);
}

function removeFirst<T>(observable: any, predicate: (s: T) => boolean): void {
    let copy = unwrapArray<T>(observable).map(item => ko.unwrap(item));
    for (let i = 0; i < copy.length; i++) {
        if (predicate(copy[i])) {
            copy.splice(i, 1);
            break;
        }
    }

    incrementVersion(observable);
    observable.setState(copy);
}

function removeLast<T>(observable: any, predicate: (s: T) => boolean): void {
    let copy = unwrapArray<T>(observable).map(item => ko.unwrap(item));
    for (let i = copy.length - 1; i >= 0; i--) {
        if (predicate(copy[i])) {
            copy.splice(i, 1);
            break;
        }
    }

    incrementVersion(observable);
    observable.setState(copy);
}

function reverse<T>(observable: any): void {
    let copy = unwrapArray<T>(observable).map(item => ko.unwrap(item));
    copy.reverse();

    incrementVersion(observable);
    observable.setState(copy);
}

function select<T, U>(observable: any, selector: (e: T) => U): U[] {
    let version = getVersion(observable);
    let result = unwrapArray<T>(observable).map(selector);

    ensureNotMutated(observable, version);
    return result;
}

function skip<T>(observable: any, count: number): T[] {
    let version = getVersion(observable);
    let result = unwrapArray<T>(observable).slice(count);

    ensureNotMutated(observable, version);
    return result;
}

function take<T>(observable: any, count: number): T[] {
    let version = getVersion(observable);
    let result = unwrapArray<T>(observable).slice(0, count);

    ensureNotMutated(observable, version);
    return result;
}

function where<T>(observable: any, predicate: (e: T) => boolean): T[] {
    let version = getVersion(observable);
    let result = unwrapArray<T>(observable).filter(predicate);

    ensureNotMutated(observable, version);
    return result;
}

function unwrapArray<T>(observable: any): T[] {
    return ko.unwrap(observable) as T[];
}

function getVersion(obserable: any): number {
    if (obserable[versionSymbol] === undefined) {
        obserable[versionSymbol] = 1;
    }

    return obserable[versionSymbol];
}

function incrementVersion(obserable: any): void {
    let currentVersion = getVersion(obserable);
    obserable[versionSymbol] = currentVersion + 1;
}

function ensureNotMutated(obserable: any, lastVersion: number): void {
    let currentVersion = getVersion(obserable);
    if (currentVersion !== lastVersion) {
        throw Error("Collection modified during enumeration!");
    }
}
