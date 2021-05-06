import { getTypeInfo } from "./typeMap";

export function getTypeId(viewModel: object): string | undefined {
    return ko.unwrap((viewModel as any).$type);
}

export function getTypeMetadata(typeId: string): TypeMetadata {
    return getTypeInfo(typeId);
}

export function getEnumMetadata(enumMetadataId: string): EnumTypeMetadata {
    let metadata = getTypeInfo(enumMetadataId);
    if (metadata.type !== "enum") {
        throw new Error("Expected enum, but received object");
    }

    return metadata as EnumTypeMetadata;
}

export function getEnumValue(identifier: string | number, enumMetadataId: string): number | null {
    let metadata = getEnumMetadata(enumMetadataId);
    if (typeof identifier === "string") {
        return metadata.values[identifier];
    }
    else {
        return identifier;
    }
}
