import { wrapObservable } from '../utils/knockout'
import { serializeDate } from './date'
import { isPrimitive } from '../utils/objects'

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

    if (opt.pathOnly && opt.path && opt.path.length === 0) opt.pathOnly = false;

    if (isPrimitive(viewModel)) {
        return viewModel;
    }

    if (typeof (viewModel) == "function") {
        return null;
    }

    if (viewModel instanceof Array) {
        if (opt.pathOnly && opt.path) {
            var index = parseInt(<string>opt.path.pop());
            let array = new Array(index + 1);
            array[index] = serialize(viewModel[index], opt);
            opt.path.push(index.toString());
            return array;
        } else {

            let array: any[] = [];
            for (var i = 0; i < viewModel.length; i++) {
                array.push(serialize(viewModel[i], opt));
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

    var pathProp = opt.path && opt.path.pop();

    var result: any = {};
    for (var prop of Object.keys(viewModel)) {
        if (opt.pathOnly && prop !== pathProp) {
            continue;
        }

        var value = viewModel[prop];

        if (opt.ignoreSpecialProperties && prop[0] === "$") continue;
        if (!opt.serializeAll && (/\$options$/.test(prop) || prop === "$validationErrors")) {
            continue;
        }
        if (typeof (value) === "undefined") {
            continue;
        }
        if (!ko.isObservable(value) && typeof (value) === "function") {
            continue;
        }

        var options = viewModel[prop + "$options"];
        if (!opt.serializeAll && options && options.doNotPost) {
            // continue
        }
        else if (opt.oneLevel) {
            result[prop] = ko.unwrap(value);
        }
        else if (!opt.serializeAll && options && options.pathOnly && opt.pathMatcher) {
            var path = options.pathOnly;
            if (!(path instanceof Array)) {
                path = opt.path || findObject(value, opt.pathMatcher);
            }
            if (path) {
                if (path.length === 0) {
                    result[prop] = serialize(value, opt);
                }
                else {
                    result[prop] = serialize(value, { ignoreSpecialProperties: opt.ignoreSpecialProperties, serializeAll: opt.serializeAll, path: path, pathOnly: true });
                }
            }
        }
        else {
            result[prop] = serialize(value, opt);
        }
        if (options && options.type && !validateType(result[prop], options.type)) {
            delete result[prop];
            options.wasInvalid = true;
        }
    }
    if (pathProp && opt.path) opt.path.push(pathProp);
    return result;
}

function validateType(value: any, type: string) {
    const nullable = type[type.length - 1] == "?";
    if (nullable) {
        type = type.substr(0, type.length - 1);
    }
    if (nullable && (value == null || value == "")) {
        return true;
    }
    if (!nullable && (value == null)) {
        return false;
    }

    var intmatch = /(u?)int(\d*)/.exec(type);
    if (intmatch) {
        if (!/^-?\d*$/.test(value)) return false;

        var unsigned = intmatch[1] === "u";
        var bits = parseInt(intmatch[2]);
        var minValue = 0;
        var maxValue = Math.pow(2, bits) - 1;
        if (!unsigned) {
            minValue = -Math.floor(maxValue / 2);
            maxValue = maxValue + minValue;
        }
        var int = parseInt(value);
        return int >= minValue && int <= maxValue && int === parseFloat(value);
    }
    if (type == "number" || type == "single" || type == "double" || type == "decimal") {
        // should check if the value is numeric or number in a string
        return +value === value || (!isNaN(+value) && typeof value == "string");
    }
    return true;
}

function findObject(obj: any, matcher: (o: any) => boolean): string[] | null {
    if (matcher(obj)) return [];
    obj = ko.unwrap(obj);
    if (matcher(obj)) return [];
    if (typeof obj != "object") return null;
    for (var p of Object.keys(obj)) {
        var match = findObject(obj[p], matcher);
        if (match) {
            match.push(p);
            return match;
        }
    }
    return null;
}
