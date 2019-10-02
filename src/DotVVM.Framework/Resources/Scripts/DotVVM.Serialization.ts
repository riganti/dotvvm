interface ISerializationOptions {
    serializeAll?: boolean;
    oneLevel?: boolean;
    ignoreSpecialProperties?: boolean;
    pathMatcher?: (vm: any) => boolean;
    path?: string[];
    pathOnly?: boolean;
    restApiTarget?: boolean;    // convert string dates to Date objects
}

class DotvvmSerialization {

    public deserializeNullOrUndefined(viewModel: any, target?: any): any {
        if (ko.isObservable(target)) {
            target(viewModel);
            return target;
        }
        return viewModel;
    }

    public deserializePrimitive(viewModel: any, target?: any): any {
        if (ko.isObservable(target)) {
            target(viewModel);
            return target;
        }
        return viewModel;
    }

    public deserializeDate(viewModel: any, target?: any): any {
        viewModel = dotvvm.serialization.serializeDate(viewModel);
        if (ko.isObservable(target)) {
            target(viewModel);
            return target;
        }
        return viewModel;
    }

    public deserializeArray(viewModel: any, target?: any, deserializeAll: boolean = false): any {
        if (ko.isObservable(target) && "removeAll" in target && target() != null && target().length === viewModel.length) {
            this.updateArrayItems(target, viewModel, deserializeAll);
        } else {
            target = this.rebuildArrayFromScratch(viewModel, deserializeAll, target);
        }
        return target;
    }

    private rebuildArrayFromScratch(viewModel: any, deserializeAll: boolean, target: any) {
        var array: KnockoutObservable<any>[] = [];
        for (var i = 0; i < viewModel.length; i++)
        {
            array.push(this.wrapObservable(this.deserialize(ko.unwrap(viewModel[i]), {}, deserializeAll)));
        }
        if (ko.isObservable(target)) {
            if (!("removeAll" in target)) {
                // if the previous value was null, the property is not an observable array - make it
                ko.utils.extend(target, ko.observableArray['fn']);
                target = target.extend({ 'trackArrayChanges': true });
            }
            target(array);
        }
        else {
            target = ko.observableArray(array);
        }
        return target;
    }

    private updateArrayItems(target: KnockoutObservable<any>, viewModel: any, deserializeAll: boolean) {
        var targetArray = target();
        for (var i = 0; i < viewModel.length; i++) {
            var targetItem = targetArray[i]();
            var deserialized = this.deserialize(ko.unwrap(viewModel[i]), targetItem, deserializeAll);
            //It should be fine that we do not unwrap deserialized because target is unwrapped and viewmodel is unwrapped so the result should be unwrapped
            if (targetItem !== deserialized) {
                // update the observable only if the item has changed
                targetArray[i](deserialized);
            }
        }
    }

    deserializeObject(viewModel: any, target: any, deserializeAll: boolean): any {
        //If we stepped into target and called deserialize on target.A and A did not exists but A existed in viewmodel,
        //we simply create the object and rely on caller to set the property
        if (typeof (target) === "undefined") {
            target = {};
        }
        //Ok this is probably so that we can construct a copy of viewmodel and set it to initialy empty observable later
        var unwrappedTarget = ko.unwrap(target);
        var updateTarget = false;
        if (unwrappedTarget == null) {
            unwrappedTarget = {};
            if (ko.isObservable(target)) {
                updateTarget = true;
            } else {
                target = unwrappedTarget;
            }
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
                var deserialized = this.deserialize(ko.unwrap(value), unwrappedTarget[prop], deserializeAll);
                if (value instanceof Date) {
                    // if we get Date value from API, it was converted to string, but we should note that it was date to convert it back
                    unwrappedTarget[prop + "$options"] = { ...unwrappedTarget[prop + "$options"], isDate: true };
                }

                // update the property
                if (ko.isObservable(deserialized)) {
                    if (ko.isObservable(unwrappedTarget[prop])) {
                        if (deserialized() !== unwrappedTarget[prop]()) {
                            unwrappedTarget[prop](deserialized());
                        }
                    } else {
                        const unwrapped = ko.unwrap(deserialized);
                        unwrappedTarget[prop] = Array.isArray(unwrapped) ? ko.observableArray(unwrapped) : ko.observable(unwrapped);      // don't reuse the same observable from the source
                    }
                } else {
                    if (ko.isObservable(unwrappedTarget[prop])) {
                        if (deserialized !== unwrappedTarget[prop]()) unwrappedTarget[prop](deserialized);
                    } else {
                        unwrappedTarget[prop] = ko.observable(deserialized);
                    }
                }

                if (options && options.clientExtenders && ko.isObservable(unwrappedTarget[prop])) {
                    for (var j = 0; j < options.clientExtenders.length; j++) {
                        var extenderOptions = {};
                        var extenderInfo = options.clientExtenders[j];
                        extenderOptions[extenderInfo.name] = extenderInfo.parameter;
                        unwrappedTarget[prop].extend(extenderOptions);
                    }
                }
            }
        }

        // copy the property options metadata
        for (var prop in viewModel) {
            if (viewModel.hasOwnProperty(prop) && /\$options$/.test(prop)) {

                unwrappedTarget[prop] = unwrappedTarget[prop] || {};
                for (var optProp in viewModel[prop]) {
                    if (viewModel[prop].hasOwnProperty(optProp)) {
                        unwrappedTarget[prop][optProp] = viewModel[prop][optProp];
                    }
                }

                var originalName = prop.substring(0, prop.length - "$options".length);
                if (typeof unwrappedTarget[originalName] === "undefined") {
                    unwrappedTarget[originalName] = ko.observable();
                }
            }
        }

        //CHECK: and if target wasnt null but still is observable no need to update it?
        if (updateTarget) {
            target(unwrappedTarget);
        }
        return target;
    }

    public deserialize(viewModel: any, target?: any, deserializeAll: boolean = false): any {

        if (typeof (viewModel) == "undefined" || viewModel == null) {
            return this.deserializeNullOrUndefined(viewModel, target);
        }
        if (typeof (viewModel) == "string" || typeof (viewModel) == "number" || typeof (viewModel) == "boolean") {
            return this.deserializePrimitive(viewModel, target);
        }
        if (viewModel instanceof Date) {
            return this.deserializeDate(viewModel, target);
        }

        if (viewModel instanceof Array) {
            return this.deserializeArray(viewModel, target, deserializeAll)
        }

        return this.deserializeObject(viewModel, target, deserializeAll)
    }

    public wrapObservable<T>(obj: T): KnockoutObservable<T> {
        if (!ko.isObservable(obj)) return ko.observable(obj);
        return <KnockoutObservable<T>><any>obj;
    }

    public serialize(viewModel: any, opt: ISerializationOptions = {}): any {
        opt = ko.utils.extend({}, opt);

        if (opt.pathOnly && opt.path && opt.path.length === 0) opt.pathOnly = false;

        if (viewModel == null) {
            return null;
        }

        if (typeof (viewModel) === "string" || typeof (viewModel) === "number" || typeof (viewModel) === "boolean") {
            return viewModel;
        }

        if (ko.isObservable(viewModel)) {
            return this.serialize(viewModel(), opt);
        }

        if (typeof (viewModel) === "function") {
            return null;
        }

        if (viewModel instanceof Array) {
            if (opt.pathOnly && opt.path) {
                var index = parseInt(<string>opt.path.pop());
                let array = new Array(index + 1);
                array[index] = this.serialize(viewModel[index], opt);
                opt.path.push(index.toString());
                return array;
            } else {

                let array: any[] = [];
                for (var i = 0; i < viewModel.length; i++) {
                    array.push(this.serialize(viewModel[i], opt));
                }
                return array;
            }
        }

        if (viewModel instanceof Date) {
            if (opt.restApiTarget) {
                return viewModel;
            } else {
                return this.serializeDate(viewModel);
            }
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
                        path = opt.path || this.findObject(value, opt.pathMatcher);
                    }
                    if (path) {
                        if (path.length === 0) {
                            result[prop] = this.serialize(value, opt);
                        }
                        else {
                            result[prop] = this.serialize(value, { ignoreSpecialProperties: opt.ignoreSpecialProperties, serializeAll: opt.serializeAll, path: path, pathOnly: true });
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
        if (pathProp && opt.path) opt.path.push(pathProp);
        return result;
    }

    public validateType(value, type: string) {
        var nullable = type[type.length - 1] === "?";
        if (nullable) {
            type = type.substr(0, type.length - 1);
        }
        if (nullable && (value == null || value == "")) {
            return true;
        }
        if (!nullable && (value === null || typeof value === "undefined")) {
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
        if (type === "number" || type === "single" || type === "double" || type === "decimal") {
            // should check if the value is numeric or number in a string
            return +value === value || (!isNaN(+value) && typeof value === "string");
        }
        return true;
    }

    private findObject(obj: any, matcher: (o) => boolean): string[] | null {
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

    public serializeDate(date: string | Date | null, convertToUtc: boolean = true): string | null {
        if (date == null) {
            return null;
        } else if (typeof date == "string") {
            // just print in the console if it's invalid
            if (dotvvm.globalize.parseDotvvmDate(date) == null)
                console.error(new Error(`Date ${date} is invalid.`));
            return date;
        }
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
