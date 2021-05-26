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

export function setItem<Key, Value>(observable: any, identifier: Key, value: Value): void {
    const dictionary = [...observable.state]
    for (let index = 0; index < dictionary.length; index++) {
        let keyValuePair = dictionary[index];
        if (keyValuePair.Key == identifier) {
            dictionary[index] = { Value: value, Key: keyValuePair.Key }
            observable.setState(dictionary)
            return;
        }
    }
    // Create new record if we did not find provided key
    dictionary.push({ "Key": identifier, "Value": value });
    observable.setState(dictionary);
}
