interface ISerializationOptions {
    serializeAll?: boolean;
    oneLevel?: boolean;
    ignoreSpecialProperties?: boolean;
    pathMatcher?: (vm: any) => boolean;
    path?: string[];
    pathOnly?: boolean;
}

class DotvvmSerialization {

    public deserialize(viewModel: any, target?: any, deserializeAll: boolean = false) {

        if (typeof (viewModel) == "undefined" || viewModel == null) {
            return viewModel;
        }
        if (typeof (viewModel) == "string" || typeof (viewModel) == "number" || typeof (viewModel) == "boolean") {
            return viewModel;
        }
        if (viewModel instanceof Date) {
            return viewModel;
        }

        // handle arrays
        if (viewModel instanceof Array) {
            if (ko.isObservable(target) && target.removeAll && target().length === viewModel.length) {
                // the array has the same number of items, update it
                var targetArray = target();
                for (var i = 0; i < viewModel.length; i++) {
                    var targetItem = targetArray[i]();
                    var deserialized = this.deserialize(viewModel[i], targetItem, deserializeAll);
                    if (targetItem !== deserialized) {
                        // update the observable only if the item has changed
                        targetArray[i](deserialized);
                    }
                }

            } else {
                // rebuild the array because it is different
                var array = [];
                for (var i = 0; i < viewModel.length; i++) {
                    array.push(this.wrapObservable(this.deserialize(viewModel[i], {}, deserializeAll)));
                }

                if (ko.isObservable(target)) {
                    if (!target.removeAll) {
                        // if the previous value was null, the property is not an observable array - make it
                        ko.utils.extend(target, ko.observableArray['fn']);
                        target = target.extend({ 'trackArrayChanges': true });
                    }
                    target(array);
                } else {
                    target = ko.observableArray(array);
                }
            }
            return target;
        }

        // handle objects
        if (typeof (target) === "undefined") {
            target = {};
        }
        var result = ko.unwrap(target);
        if (result == null) {
            target = result = {};
        }
        for (var prop in viewModel) {
            if (viewModel.hasOwnProperty(prop) && !/\$options$/.test(prop)) {
                var value = viewModel[prop];
                if (typeof (value) === "undefined") {
                    continue;
                }
                if (!ko.isObservable(value) && typeof (value) === "function") {
                    continue;
                }
                var options = viewModel[prop + "$options"];
                if (!deserializeAll && options && options.doNotUpdate) {
                    continue;
                }

                // deserialize value
                var deserialized = ko.isObservable(value) ? value : this.deserialize(value, result[prop], deserializeAll);
                
                // update the property
                if (ko.isObservable(deserialized)) {
                    if (ko.isObservable(result[prop])) {
                        if (deserialized() !== result[prop]()) {
                            result[prop](deserialized());
                        }
                    } else {
                        result[prop] = deserialized;
                    }
                } else {
                    if (ko.isObservable(result[prop])) {
                        if (deserialized !== result[prop]()) result[prop](deserialized);
                    } else {
                        result[prop] = ko.observable(deserialized);
                    }
                }
            }
        }

        // copy the property options metadata
        for (var prop in viewModel) {
            if (viewModel.hasOwnProperty(prop) && /\$options$/.test(prop)) {
                result[prop] = viewModel[prop];
                var originalName = prop.substring(0, prop.length - "$options".length);
                if (typeof result[originalName] === "undefined") {
                    result[originalName] = ko.observable();
                }
            }
        }

        return target;
    }

    public wrapObservable<T>(obj: T): KnockoutObservable<T> {
        if (!ko.isObservable(obj)) return ko.observable(obj);
        return <KnockoutObservable<T>><any>obj;
    }

    public serialize(viewModel: any, opt: ISerializationOptions = {}): any {
        opt = ko.utils.extend({}, opt);

        if (opt.pathOnly && opt.path && opt.path.length === 0) opt.pathOnly = false;

        if (typeof (viewModel) === "undefined" || viewModel == null) {
            return viewModel;
        }

        if (typeof (viewModel) === "string" || typeof (viewModel) === "number" || typeof (viewModel) === "boolean") {
            return viewModel;
        }

        if (ko.isObservable(viewModel)) {
            return this.serialize(ko.unwrap(viewModel), opt);
        }

        if (typeof (viewModel) === "function") {
            return null;
        }

        if (viewModel instanceof Array) {
            if (opt.pathOnly) {
                var index = parseInt(opt.path.pop());
                var array = new Array(index + 1);
                array[index] = this.serialize(viewModel[index], opt);
                opt.path.push(index.toString());
                return array;
            } else {

                var array = [];
                for (var i = 0; i < viewModel.length; i++) {
                    array.push(this.serialize(viewModel[i], opt));
                }
                return array;
            }
        }

        if (viewModel instanceof Date) {
            return this.serializeDate(viewModel);
        }

        var pathProp = opt.path && opt.path.pop();

        var result = {};
        for (var prop in viewModel) {
            if (viewModel.hasOwnProperty(prop)) {
                if (opt.pathOnly && prop !== pathProp) {
                    continue;
                }

                var value = viewModel[prop];

                if (opt.ignoreSpecialProperties && prop[0] === "$") continue;
                if (!opt.serializeAll && /\$options$/.test(prop)) {
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
                else if (!opt.serializeAll && options && options.pathOnly) {
                    var path = options.pathOnly;
                    if (!(path instanceof Array)) {
                        path = opt.path || this.findObject(value, opt.pathMatcher);
                    }
                    if (path) {
                        if (path.length === 0) {
                            result[prop] = this.serialize(value, opt);
                        }
                        else {
                            result[prop] = this.serialize(value, { ignoreSpecialProperties: opt.ignoreSpecialProperties, serializeAll: opt.serializeAll, path: path, pathOnly: true })
                        }
                    }
                }
                else {
                    result[prop] = this.serialize(value, opt);
                }
                if (options && options.type && !this.validateType(result[prop], options.type)) {
                    delete result[prop];
                    options.wasInvalid = true;
                }
            }
        }
        if (pathProp) opt.path.push(pathProp);
        return result;
    }

    public validateType(value, type: string) {
        var nullable = type[type.length - 1] === "?";
        if (nullable) {
            type = type.substr(0, type.length - 1);
        }
        if (nullable && (typeof(value) === "undefined" || value == null)) {
            return true;
        }

        var intmatch = /(u?)int(\d*)/.exec(type);
        if (intmatch) {
            if (!/^-?\d*$/.test(value)) return false;

            var unsigned = intmatch[1] === "u";
            var bits = parseInt(intmatch[2]);
            var minValue = 0;
            var maxValue = Math.pow(2, bits) - 1;
            if (!unsigned) {
                minValue = -((maxValue / 2) | 0);
                maxValue = maxValue + minValue;
            }
            var int = parseInt(value);
            return int >= minValue && int <= maxValue && int === parseFloat(value);
        }
        if (type === "number" || type === "single" || type === "double" || type === "decimal") {
            return !isNaN(value) || value === NaN;
        }
        return true;
    }

    private findObject(obj: any, matcher: (o) => boolean): string[] {
        if (matcher(obj)) return [];
        obj = ko.unwrap(obj);
        if (matcher(obj)) return [];
        if (typeof obj != "object") return null;
        for (var p in obj) {
            if (obj.hasOwnProperty(p)) {
                var match = this.findObject(obj[p], matcher);
                if (match) {
                    match.push(p);
                    return match;
                }
            }
        }
        return null;
    }

    public flatSerialize(viewModel: any) {
        return this.serialize(viewModel, { ignoreSpecialProperties: true, oneLevel: true, serializeAll: true });
    }

    public getPureObject(viewModel: any) {
        viewModel = ko.unwrap(viewModel);
        if (viewModel instanceof Array) return viewModel.map(this.getPureObject.bind(this));
        var result = {};
        for (var prop in viewModel) {
            if (prop[0] != '$') result[prop] = viewModel[prop];
        }
        return result;
    }

    private pad(value: string, digits: number) {
        while (value.length < digits) {
            value = "0" + value;
        }
        return value;
    }

    public serializeDate(date: Date, convertToUtc: boolean = true): string {
        var date2 = new Date(date.getTime());
        if (convertToUtc) {
            date2.setMinutes(date.getMinutes() + date.getTimezoneOffset());
        } else {
            date2 = date;
        }

        var y = this.pad(date2.getFullYear().toString(), 4);
        var m = this.pad((date2.getMonth() + 1).toString(), 2);
        var d = this.pad(date2.getDate().toString(), 2);
        var h = this.pad(date2.getHours().toString(), 2);
        var mi = this.pad(date2.getMinutes().toString(), 2);
        var s = this.pad(date2.getSeconds().toString(), 2);
        var ms = this.pad(date2.getMilliseconds().toString(), 3);
        return y + "-" + m + "-" + d + "T" + h + ":" + mi + ":" + s + "." + ms + "0000";
    }
}