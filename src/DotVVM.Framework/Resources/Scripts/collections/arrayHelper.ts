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

function add<T>(source: T[], element: T, observable: any): void {
    let copy = source.map(item => ko.unwrap(item));
    copy.push(ko.unwrap(element));

    incrementVersion(observable);
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

    incrementVersion(observable);
    observable.setState(copy);
}

function addRange<T>(source: T[], elements: T[], observable: any): void {
    let copy = source.map(item => ko.unwrap(item));
    for (let i = 0; i < elements.length; i++) {
        copy.push(ko.unwrap(elements[i]));
    }

    incrementVersion(observable);
    observable.setState(copy);
}

function any<T>(source: T[], predicate: (s: T) => boolean, observable: any): boolean {
    // Mutations are already checked inside firstOrDefault
    return firstOrDefault(source, predicate, observable) != null;
}

function all<T>(source: T[], predicate: (s: T) => boolean, observable: any): boolean {
    let version = getVersion(observable);
    let result = allImpl(source, predicate);

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

function clear<T>(source: T[], observable: any): void {
    incrementVersion(observable);
    observable.setState([]);
}

function concat<T>(source1: T[], source2: T[], observable1: any, observable2: any): T[] {
    let version1 = getVersion(observable1);
    let version2 = getVersion(observable2);
    let result = source1.concat(source2);

    ensureNotMutated(observable1, version1);
    ensureNotMutated(observable2, version2);
    return result;
}

function count<T>(source: T[], observable: any) {
    let version = getVersion(observable);
    let result = source.length;

    ensureNotMutated(observable, version);
    return result;
}

function distinct<T>(source: T[], observable: any): T[] {
    let version = getVersion(observable);
    let result = distinctImpl(source);

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

function firstOrDefault<T>(source: T[], predicate: (s: T) => boolean, observable: any): T | null {
    let version = getVersion(observable);
    let result = firstOrDefaultImpl(source, predicate);

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

function insert<T>(items: T[], index: number, element: T, observable: any): void {
    let copy = items.map(item => ko.unwrap(item));
    copy.splice(index, 0, element);

    incrementVersion(observable);
    observable.setState(copy);
}

function insertRange<T>(items: T[], index: number, elements: T[], observable: any): void {
    let copy = items.map(item => ko.unwrap(item));
    copy.splice(index, 0, ...elements.map(element => ko.unwrap(element)));

    incrementVersion(items);
    observable.setState(copy);
}

function lastOrDefault<T>(source: T[], predicate: (s: T) => boolean, observable: any): T | null {
    let version = getVersion(observable);
    let result = lastOrDefaultImpl(source, predicate);

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

function max<T>(source: T[], selector: (item: T) => number, observable: any): number {
    let version = getVersion(observable);
    let result = maxImpl(source, selector);

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

function min<T>(source: T[], selector: (item: T) => number, observable: any): number {
    let version = getVersion(observable);
    let result = minImpl(source, selector);

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

function orderBy<T>(source: T[], selector: (item: T) => ElementType, observable: any): T[] {
    let version = getVersion(observable);
    let result = orderByImpl(source, selector);

    ensureNotMutated(observable, version);
    return result;
}

function orderByDesc<T>(source: T[], selector: (item: T) => ElementType, observable: any): T[] {
    let version = getVersion(observable);
    let result = orderByDescImpl(source, selector);

    ensureNotMutated(observable, version);
    return result;
}

function removeAt<T>(source: T[], index: number, observable: any): void {
    let copy = source.map(item => ko.unwrap(item));
    copy.splice(index, 1);

    incrementVersion(observable);
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

    incrementVersion(observable);
    observable.setState(copy);
}

function removeRange<T>(source: T[], index: number, length: number, observable: any): void {
    let copy = source.map(item => ko.unwrap(item));
    copy.splice(index, length);

    incrementVersion(observable);
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

    incrementVersion(observable);
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

    incrementVersion(observable);
    observable.setState(copy);
}

function reverse<T>(source: T[], observable: any): void {
    let copy = source.map(item => ko.unwrap(item));
    copy.reverse();

    incrementVersion(observable);
    observable.setState(copy);
}

function select<T, U>(source: T[], selector: (e: T) => U, observable: any): U[] {
    let version = getVersion(observable);
    let result = source.map(selector);

    ensureNotMutated(observable, version);
    return result;
}

function skip<T>(source: T[], count: number, observable: any): T[] {
    let version = getVersion(observable);
    let result = source.slice(count);

    ensureNotMutated(observable, version);
    return result;
}

function take<T>(source: T[], count: number, observable: any): T[] {
    let version = getVersion(observable);
    let result = source.slice(0, count);

    ensureNotMutated(observable, version);
    return result;
}

function where<T>(source: T[], predicate: (e: T) => boolean, observable: any): T[] {
    let version = getVersion(observable);
    let result = source.filter(predicate);

    ensureNotMutated(observable, version);
    return result;
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
