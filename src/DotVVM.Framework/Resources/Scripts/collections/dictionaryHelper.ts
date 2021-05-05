type Dictionary<Key, Value> = { Key: Key, Value: Value }[];

export function getItem<Key, Value>(dictionary: Dictionary<Key, Value>, identifier: Key): Value {
    for (let index = 0; index < dictionary.length; index++) {
        let keyValuePair = ko.unwrap(dictionary[index]);
        if (ko.unwrap(keyValuePair.Key) == identifier) {
            return keyValuePair.Value;
        }
    }

    throw Error("Provided key \"" + identifier + "\" is not present in the dictionary!");
}

export function setItem<Key, Value>(dictionary: Dictionary<Key, Value>, identifier: Key, value: Value, observable: any): void {
    for (let index = 0; index < dictionary.length; index++) {
        let keyValuePair = ko.unwrap(dictionary[index]);
        if (ko.unwrap(keyValuePair.Key) == identifier) {
            (keyValuePair.Value as any).setState(value);
            return;
        }
    }

    // Create new record if we did not find provided key
    let patch = dictionary.map(function (e) { return {}; }).concat({ "Key": identifier, "Value": value });
    observable.patchState(patch);
}
