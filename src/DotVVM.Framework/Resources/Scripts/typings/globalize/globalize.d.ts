interface GlobalizeStatic {
    format(value: Date | number, format: string, cultureSelector?: string): string;
    parseDate(value: string, formats: string, cultureSelector?: string, previousValue?: Date): Date | null;
    parseFloat(value: string, radix?: number, cultureSelector?: string): number;
    parseInt(value: string, radix?: number, cultureSelector?: string): number;
}

declare var dotvvm_Globalize: GlobalizeStatic;
