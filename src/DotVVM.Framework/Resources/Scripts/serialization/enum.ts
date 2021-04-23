import { getMetadataInfo } from '../metadata/metadataInfo'

export function getEnumValue(viewModel: any, value: any): number | null {
    if (typeof value === "string") {
        let enumMetadataInfo = getEnumMetadataInfo(viewModel, value);
        if (enumMetadataInfo !== null && value in enumMetadataInfo.values) {
            return enumMetadataInfo.values[value];
        }
    }
    else if (typeof value === "number") {
        return value;
    }

    return null;
}

export function getEnumMetadataInfo(viewModel: any, value: any): EnumTypeMetadata | null {
    let metadataInfo = getMetadataInfo(viewModel, value);
    if (metadataInfo !== null && metadataInfo.type === "enum") {
        return metadataInfo;
    }

    return null;
}
