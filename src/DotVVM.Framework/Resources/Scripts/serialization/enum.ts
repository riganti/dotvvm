import { getMetadataInfo } from '../metadata/metadataInfo'

export function getEnumValue(viewModel: any, value: any): number | null {
    let enumMetadataInfo = getEnumMetadataInfo(viewModel, value);
    if (enumMetadataInfo !== null) {
        return enumMetadataInfo.values[ko.unwrap(value)];
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
