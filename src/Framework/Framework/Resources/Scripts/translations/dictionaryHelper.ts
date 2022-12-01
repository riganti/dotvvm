type Dictionary<Key, Value> = { Key: Key, Value: Value }[];

export function clear(observable: any): void {
    observable.setState([]);
}

export function containsKey<Key, Value>(dictionary: Dictionary<Key, Value>, identifier: Key): boolean {
    return getKeyValueIndex(dictionary, identifier) !== null;
}

export function getItem<Key, Value>(dictionary: Dictionary<Key, Value>, identifier: Key): Value {
    const index = getKeyValueIndex(dictionary, identifier);
    if (index === null) {
        throw Error("Provided key \"" + identifier + "\" is not present in the dictionary!");
    }

    return ko.unwrap(ko.unwrap(dictionary[index]).Value);
}

export function remove<Key, Value>(observable: any, identifier: Key): boolean {
    let dictionary = [...observable.state];
    const index = getKeyValueIndex(dictionary, identifier);

    if (index === null) {
        return false;
    }
    else {
        dictionary.splice(index, 1);
        observable.setState(dictionary);
        return true;
    }
}

export function setItem<Key, Value>(observable: any, identifier: Key, value: Value): void {
    const dictionary = [...observable.state];
    const index = getKeyValueIndex(dictionary, identifier);

    if (index !== null) {
        let keyValuePair = dictionary[index];
        dictionary[index] = { Key: keyValuePair.Key, Value: value };
        observable.setState(dictionary);
    }
    else {
        dictionary.push({ Key: identifier, Value: value });
        observable.setState(dictionary);
    }
}

function getKeyValueIndex<Key, Value>(dictionary: Dictionary<Key, Value>, identifier: Key): number | null {
    for (let index = 0; index < dictionary.length; index++) {
        let keyValuePair = ko.unwrap(dictionary[index]);
        if (ko.unwrap(keyValuePair.Key) == identifier) {
            return index;
        }
    }

    return null;
}
