interface GlobalizeStatic {
    format(value: Date, format: string, cultureSelector?: string): string;
    format(value: number, format: string, cultureSelector?: string): string;
    parseDate(value: string, formats: string, cultureSelector?: string): Date;
    parseFloat(value: string, radix?: number, cultureSelector?: string): number;
    parseInt(value: string, radix?: number, cultureSelector?: string): number;
}

declare var Globalize: GlobalizeStatic;
