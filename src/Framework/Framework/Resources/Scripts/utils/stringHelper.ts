import { format as formatImpl } from "../DotVVM.Globalize";

export function contains(haystack: string, needle: string, options: string): boolean {
    const normalized = normalizeStrings(haystack, needle, options);
    return normalized.haystack.includes(normalized.needle);
}

export function endsWith(haystack: string, needle: string, options: string): boolean {
    const normalized = normalizeStrings(haystack, needle, options);
    return normalized.haystack.endsWith(normalized.needle);
}

export function startsWith(haystack: string, needle: string, options: string): boolean {
    const normalized = normalizeStrings(haystack, needle, options);
    return normalized.haystack.startsWith(normalized.needle);
}

export function indexOf(haystack: string, startIndex: number, needle: string, options: string): number {
    const normalized = normalizeStrings(haystack, needle, options);
    return normalized.haystack.indexOf(normalized.needle, startIndex);
}

export function lastIndexOf(haystack: string, startIndex: number, needle: string, options: string): number {
    const normalized = normalizeStrings(haystack, needle, options);
    return normalized.haystack.indexOf(normalized.needle, startIndex);
}

function normalizeStrings(haystack: string, needle: string, options: string): { haystack: string, needle: string } {
    if (options.endsWith("IgnoreCase")) {
        return { haystack: haystack.toUpperCase(), needle: needle.toUpperCase() };
    }
    return { haystack: haystack, needle: needle };
}

export function split(text: string, delimiter: string, options: string): string[] {
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

export function trimStart(text: string, char: string) {
    if (char != null) {
        char = char.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }
    else {
        char = "\\s";
    }
    return text.replace(new RegExp("^" + char + "*"), "");
}

export function trimEnd(text: string, char: string) {
    if (char != null) {
        char = char.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }
    else {
        char = "\\s";
    }
    return text.replace(new RegExp(char + "*$"), "");
}

export function padStart(text: string, length: number, char: string) {
    if (length <= text.length) {
        return text;
    }
    if (char == null) {
        char = " ";
    }
    return Array(length - text.length + 1).join(char) + text;
}

export function padEnd(text: string, length: number, char: string) {
    if (length <= text.length) {
        return text;
    }
    if (char == null) {
        char = " ";
    }
    return text + Array(length - text.length + 1).join(char);
}
