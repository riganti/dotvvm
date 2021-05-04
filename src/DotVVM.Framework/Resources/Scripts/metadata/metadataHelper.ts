import { getTypeInfo } from "./typeMap";

export function getTypeId(viewModel: any): string {
    if (typeof viewModel !== "object") {
        throw Error("Expected object but received: " + typeof viewModel);
    }

    return ko.unwrap(viewModel.$type);
}

export function getTypeMetadata(viewModel: any): TypeMetadata {
    let typeId = getTypeId(viewModel);
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
        // Ensure this number is not already defined
        for (const key in metadata.values) {
            if (metadata.values[key] === identifier) {
                return null;
            }
        }

        return identifier;
    }
}
