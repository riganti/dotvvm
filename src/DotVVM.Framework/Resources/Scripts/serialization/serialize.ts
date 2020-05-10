import { wrapObservable } from '../utils/knockout'
import { serializeDate } from './date'
import { isPrimitive, keys } from '../utils/objects'
import { validateType } from './typeValidation'

interface ISerializationOptions {
    serializeAll?: boolean;
    oneLevel?: boolean;
    ignoreSpecialProperties?: boolean;
    pathMatcher?: (vm: any) => boolean;
    path?: string[];
    pathOnly?: boolean;
    restApiTarget?: boolean;    // convert string dates to Date objects
}

export function serialize(viewModel: any, opt: ISerializationOptions = {}): any {
    opt = ko.utils.extend({}, opt)
    viewModel = ko.unwrap(viewModel)

    if (opt.pathOnly && opt.path && opt.path.length === 0) {
        opt.pathOnly = false;
    }

    if (isPrimitive(viewModel)) {
        return viewModel;
    }

    if (typeof (viewModel) == "function") {
        return null;
    }

    if (viewModel instanceof Array) {
        if (opt.pathOnly && opt.path) {
            const index = parseInt(<string> opt.path.pop(), 10);
            const array = new Array(index + 1);
            array[index] = serialize(viewModel[index], opt);
            opt.path.push(index.toString());
            return array;
        } else {
            const array: any[] = [];
            for (const item of viewModel) {
                array.push(serialize(item, opt));
            }
            return array;
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

    const result: any = {};
    for (const prop of keys(viewModel)) {
        if (opt.pathOnly && prop !== pathProp) {
            continue;
        }

        const value = viewModel[prop];
        if (opt.ignoreSpecialProperties && prop[0] === "$") {
            continue;
        }
        if (!opt.serializeAll && (/\$options$/.test(prop) || prop === "$validationErrors")) {
            continue;
        }
        if (typeof (value) === "undefined") {
            continue;
        }
        if (!ko.isObservable(value) && typeof (value) === "function") {
            continue;
        }

        const options = viewModel[prop + "$options"];
        if (!opt.serializeAll && options && options.doNotPost) {
            // continue
        } else if (opt.oneLevel) {
            result[prop] = ko.unwrap(value);
        } else if (!opt.serializeAll && options && options.pathOnly && opt.pathMatcher) {
            let path = options.pathOnly;
            if (!(path instanceof Array)) {
                path = opt.path || findObject(value, opt.pathMatcher);
            }
            if (path) {
                if (path.length === 0) {
                    result[prop] = serialize(value, opt);
                } else {
                    result[prop] = serialize(value, { ignoreSpecialProperties: opt.ignoreSpecialProperties, serializeAll: opt.serializeAll, path: path, pathOnly: true });
                }
            }
        } else {
            result[prop] = serialize(value, opt);
        }

        if (options && options.type && !validateType(result[prop], options.type)) {
            delete result[prop];
            options.wasInvalid = true;
        }
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
    obj = ko.unwrap(obj);
    if (matcher(obj)) {
        return [];
    }
    if (typeof obj != "object") {
        return null;
    }
    for (const p of keys(obj)) {
        const match = findObject(obj[p], matcher);
        if (match) {
            match.push(p);
            return match;
        }
    }
    return null;
}
