import { serializeDate } from './date'
import { isPrimitive, keys } from '../utils/objects'
import { getObjectTypeInfo } from '../metadata/typeMap'
import { unmapKnockoutObservables } from '../state-manager'

interface ISerializationOptions {
    serializeAll?: boolean;
    ignoreSpecialProperties?: boolean;
    pathMatcher?: (vm: any) => boolean;
    path?: string[];
    pathOnly?: boolean;
    restApiTarget?: boolean;    // convert string dates to Date objects
}

export function serialize(viewModel: any, opt: ISerializationOptions = {}): any {
    return serializeCore(unmapKnockoutObservables(viewModel), { ...opt })
}

export function serializeCore(viewModel: any, opt: ISerializationOptions = {}): any {
    if (opt.pathOnly && opt.path && opt.path.length === 0) {
        opt.pathOnly = false;
    }

    if (isPrimitive(viewModel)) {
        return viewModel ?? null;
    }

    if (typeof (viewModel) == "function") {
        return null;
    }

    if (isPrimitive(viewModel)) {
        return viewModel ?? null;
    }

    if (viewModel instanceof Array) {
        if (opt.pathOnly && opt.path) {
            const index = parseInt(<string> opt.path.pop(), 10);
            const array = new Array(index + 1);
            array[index] = serializeCore(viewModel[index], opt);
            opt.path.push(index.toString());
            return array;
        } else {
            return viewModel.map(item => serializeCore(item, opt))
        }
    }

    if (viewModel instanceof Date) {
        if (opt.restApiTarget) {
            return viewModel;
        } else {
            return serializeDate(viewModel);
        }
    }

    const pathProp = opt.path && opt.path.pop();

    const typeId = ko.unwrap(viewModel["$type"]);
    if (!typeId) {
        throw `Missing type metadata for object ${ko.toJSON(viewModel)}!`;
    }
    const typeInfo = getObjectTypeInfo(typeId);    

    const result: any = {};
    for (const prop of keys(viewModel)) {
        const value = viewModel[prop];

        if (opt.pathOnly && prop !== pathProp) {
            continue;
        }
        if (opt.ignoreSpecialProperties && prop[0] === "$") {
            continue;
        }
        if (!opt.serializeAll && prop === "$validationErrors") {
            continue;
        }
        if (typeof (value) == "function") {
            continue;
        }

        const propInfo = typeInfo.properties[prop];
        if (!opt.serializeAll && propInfo && propInfo.post == "no") {
            // continue
        } else if (!opt.serializeAll && propInfo && propInfo.post == "pathOnly" && opt.pathMatcher) {
            let path = opt.path || findObject(value, opt.pathMatcher);
            if (path) {
                if (path.length === 0) {
                    result[prop] = serializeCore(value, opt);
                } else {
                    result[prop] = serializeCore(value, { ignoreSpecialProperties: opt.ignoreSpecialProperties, serializeAll: opt.serializeAll, path: path, pathOnly: true });
                }
            }
        } else {
            result[prop] = serializeCore(value, opt);
        }

        // TODO - do we need this?
        // if (propInfo && propInfo.type && !tryCoerce(result[prop], propInfo.type)) {
        //     delete result[prop];
        //     //options.wasInvalid = true;   
        // }
    }
    if (pathProp && opt.path) {
        opt.path.push(pathProp);
    }
    return result;
}

function findObject(obj: any, matcher: (o: any) => boolean): string[] | null {
    if (matcher(obj)) {
        return [];
    }
    if (!isPrimitive(obj)) {
        for (const p of keys(obj)) {
            const match = findObject(obj[p], matcher);
            if (match) {
                match.push(p);
                return match;
            }
        }
    }
    return null;
}
