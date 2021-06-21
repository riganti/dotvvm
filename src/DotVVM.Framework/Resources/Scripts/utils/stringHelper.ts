export function split<T>(text: string, delimiter: string, options: string): string[] {
    let tokens = text.split(delimiter);
    if (options === "RemoveEmptyEntries") {
        tokens = tokens.filter(t => t);
    }

    return tokens;
}

export function join<T>(elements: T[], delimiter: string): string {
    let unwrappedElements = elements.map(function (element) { return ko.unwrap(element); });
    return unwrappedElements.join(delimiter);
}
