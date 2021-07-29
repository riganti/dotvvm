import { format as formatImpl } from "../DotVVM.Globalize";

export function split<T>(text: string, delimiter: string, options: string): string[] {
    let tokens = text.split(delimiter);
    if (options === "RemoveEmptyEntries") {
        tokens = tokens.filter(t => t);
    }

    return tokens;
}

export function join<T>(elements: T[], delimiter: string): string {
    let unwrappedElements = elements.map(ko.unwrap);
    return unwrappedElements.join(delimiter);
}

export function format(pattern: string, expressions: any[]): string {
    return formatImpl(pattern, ...expressions);
}
