export function split<T>(text: string, delimiter: string, options: string): string[] {
    let tokens = text.split(delimiter);
    if (options === "RemoveEmptyEntries") {
        tokens = tokens.filter(t => t);
    }

    return tokens;
}
