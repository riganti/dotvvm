import { getTypeInfo } from "../metadata/typeMap";
import { primitiveTypes } from "../metadata/primitiveTypes";
import { keys } from "../utils/objects";

export function getMetadataInfo(viewModel: any, value: any): TypeMetadata | null {
    var path = getPath(viewModel, value);
    if (path === null) {
        return null;
    }
    else {
        let typeId = ko.unwrap((ko.unwrap(viewModel).$type)) as string;
        let type = typeId as TypeDefinition;
        let pathSegmentIndex = 0;

        while (pathSegmentIndex < path.length) {
            if (Array.isArray(type)) {
                const index = parseInt(path[pathSegmentIndex++]);
                type = type[index];
            }
            else if (typeof type === "object") {
                if (type.type == "nullable") {
                    type = type.inner;
                }
                else if (type.type === "dynamic") {
                    // No metadata available
                    return null;
                }
            }
            else if (typeof type === "string") {
                if (type in primitiveTypes) {
                    // No metadata available
                    return null;
                } else {
                    let metadata = getTypeInfo(type);
                    if (metadata && metadata.type === "object") {
                        let pathSegment = path[pathSegmentIndex++];
                        type = metadata.properties[pathSegment].type;
                    }
                    else if (metadata && metadata.type === "enum") {
                        return metadata;
                    }
                }
            }
        }

        return resolveMetadata(type);
    }
}

function resolveMetadata(type: TypeDefinition): TypeMetadata | null {
    if (!Array.isArray(type) && typeof type === "object" && type.type === "nullable") {
        // Unwrap nullables
        type = type.inner;
    }

    if (Array.isArray(type) || (typeof type === "object" && type.type === "dynamic")) {
        // We can not retrieve metadata for arrays and dynamics
        throw new Error("Could not resolve metadata!");
    }

    if (type as string in primitiveTypes) {
        // No metadata available
        return null;
    }
    else {
        return getTypeInfo(type as string);
    }
}

function getPath(from: any, target: any): string[] | null {
    if (from === target)
        return [];

    let unwrappedFrom = ko.unwrap(from);
    for (let key of keys(unwrappedFrom)) {
        let item = unwrappedFrom[key];
        if (item && typeof ko.unwrap(item) === "object") {
            let subPath = getPath(item, target);
            if (subPath) {
                subPath.unshift(key);
                return subPath;
            }
        }
        else if (item === target) {
            return [key];
        }
    }

    return null;
}
