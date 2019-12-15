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

    public wrapObservable<T>(obj: T): KnockoutObservable<T> {
        if (!ko.isObservable(obj)) return ko.observable(obj);
        return <KnockoutObservable<T>><any>obj;
    }

    public deserialize(viewModel: any, target?: any, deserializeAll: boolean = false): any {
        if (ko.isObservable(viewModel)) {
            throw new Error("Parameter viewModel should not be an observable. Maybe you forget to invoke the observable you are passing as a viewModel parameter.");
        }

        if (this.isPrimitive(viewModel)) {
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
        if (this.isObservableArray(target) && target() != null && target().length === viewModel.length) {
            this.updateArrayItems(viewModel, target, deserializeAll);
        } else {
            target = this.rebuildArrayFromScratch(viewModel, target, deserializeAll);
        }
        return target;
    }

    private rebuildArrayFromScratch(viewModel: any, target: any, deserializeAll: boolean) {
        const array: KnockoutObservable<any>[] = [];
        for (let i = 0; i < viewModel.length; i++) {
            array.push(this.wrapObservableObjectOrArray(this.deserialize(ko.unwrap(viewModel[i]), {}, deserializeAll)));
        }
        if (ko.isObservable(target)) {
            target = this.extendToObservableArrayIfRequired(target);
            target(array);
        }
        else {
            target = array;
        }
        return target;
    }

    private updateArrayItems(viewModel: any, target: KnockoutObservable<any>, deserializeAll: boolean) {
        const targetArray = target();
        for (let i = 0; i < viewModel.length; i++) {
            const targetItem = ko.unwrap(targetArray[i]);
            const deserialized = this.deserialize(ko.unwrap(viewModel[i]), targetItem, deserializeAll);

            if (targetItem !== deserialized) {
                //update the item
                if (ko.isObservable(targetArray[i])) {
                    if (targetArray[i]() !== deserialized) {
                        targetArray[i] = this.extendToObservableArrayIfRequired(targetArray[i]);
                        targetArray[i](deserialized);
                    }
                }
                else {
                    targetArray[i] = this.wrapObservableObjectOrArray(deserialized);
                }
            }
        }
    }

    deserializeObject(viewModel: any, target: any, deserializeAll: boolean): any {
        let unwrappedTarget = ko.unwrap(target);

        if (this.isPrimitive(unwrappedTarget)) {
            unwrappedTarget = {};
        }

        for (const prop of Object.getOwnPropertyNames(viewModel)) {
            if (this.isOptionsProperty(prop)) {
                continue;
            }

            const value = viewModel[prop];
            if (typeof (value) == "undefined") {
                continue;
            }
            if (!ko.isObservable(value) && typeof (value) === "function") {
                continue;
            }
            const options = viewModel[prop + "$options"];
            if (!deserializeAll && options && options.doNotUpdate) {
                continue;
            }

            this.copyProperty(value, unwrappedTarget, prop, deserializeAll, options);
        }

        // copy the property options metadata
        for (const prop of Object.getOwnPropertyNames(viewModel)) {
            if (!this.isOptionsProperty(prop)) {
                continue;
            }

            this.copyPropertyMetadata(unwrappedTarget, prop, viewModel);
        }

        if (ko.isObservable(target)) {
            //This is so that if we have already updated the instance inside target observable
            //there's no need to force update 
            if (unwrappedTarget !== target()) {
                target(unwrappedTarget);
            }
        }
        else {
            target = unwrappedTarget;
        }
        return target;
    }

    private copyProperty(value: any, unwrappedTarget: any, prop: string, deserializeAll: boolean, options: any) {
        const deserialized = this.deserialize(ko.unwrap(value), unwrappedTarget[prop], deserializeAll);
        if (value instanceof Date) {
            // if we get Date value from API, it was converted to string, but we should note that it was date to convert it back
            unwrappedTarget[prop + "$options"] = {
                ...unwrappedTarget[prop + "$options"],
                isDate: true
            };
        }

        // update the property
        if (ko.isObservable(deserialized)) { //deserialized is observable <=> its input target is observable
            if (deserialized() !== unwrappedTarget[prop]()) {
                unwrappedTarget[prop] = this.extendToObservableArrayIfRequired(unwrappedTarget[prop]);
                unwrappedTarget[prop](deserialized());
            }
        }
        else {
            unwrappedTarget[prop] = this.wrapObservableObjectOrArray(deserialized);
        }

        if (options && options.clientExtenders && ko.isObservable(unwrappedTarget[prop])) {
            for (let j = 0; j < options.clientExtenders.length; j++) {
                const extenderOptions = {};
                const extenderInfo = options.clientExtenders[j];
                extenderOptions[extenderInfo.name] = extenderInfo.parameter;
                unwrappedTarget[prop].extend(extenderOptions);
            }
        }
    }

    private copyPropertyMetadata(unwrappedTarget: any, prop: string, viewModel: any) {
        unwrappedTarget[prop] = {
            ...unwrappedTarget[prop],
            ...viewModel[prop]
        }
        const originalName = prop.substring(0, prop.length - "$options".length);
        if (typeof unwrappedTarget[originalName] === "undefined") {
            unwrappedTarget[originalName] = ko.observable();
        }
    }

    private extendToObservableArrayIfRequired(observable: any) {
        if (!ko.isObservable(observable)) {
            throw new Error("Trying to extend a non-observable to an observable array.");
        }

        if (!this.isObservableArray(observable)) {
            ko.utils.extend(observable, ko.observableArray['fn']);
            observable = observable.extend({ 'trackArrayChanges': true });
        }
        return observable;
    }

    private wrapObservableObjectOrArray<T>(obj: T): KnockoutObservable<T> | KnockoutObservableArray<T> {
        return Array.isArray(obj)
            ? ko.observableArray(obj)
            : ko.observable(obj);
    }

    private isPrimitive(viewModel: any) {
        return viewModel == null
            || typeof (viewModel) == "string"
            || typeof (viewModel) == "number"
            || typeof (viewModel) == "boolean";
    }

    private isOptionsProperty(prop: string) {
        return /\$options$/.test(prop);
    }

    private isObservableArray(target: any): boolean {
        return ko.isObservable(target) && "removeAll" in target;
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
