import { serializeDate } from './date'
import { isObservableArray, wrapObservableObjectOrArray } from '../utils/knockout'
import { isPrimitive } from '../utils/objects'

export function deserialize(viewModel: any, target?: any, deserializeAll: boolean = false): any {
    if (ko.isObservable(viewModel)) {
        throw new Error("Parameter viewModel should not be an observable. Maybe you forget to invoke the observable you are passing as a viewModel parameter.");
    }

    if (isPrimitive(viewModel)) {
        return deserializePrimitive(viewModel, target);
    }

    if (viewModel instanceof Date) {
        return deserializeDate(viewModel, target);
    }

    if (viewModel instanceof Array) {
        return deserializeArray(viewModel, target, deserializeAll)
    }

    return deserializeObject(viewModel, target, deserializeAll)
}

export function deserializePrimitive(viewModel: any, target?: any): any {
    if (ko.isObservable(target)) {
        target(viewModel);
        return target;
    }
    return viewModel;
}

export function deserializeDate(viewModel: any, target?: any): any {
    viewModel = serializeDate(viewModel);
    if (ko.isObservable(target)) {
        target(viewModel);
        return target;
    }
    return viewModel;
}

export function deserializeArray(viewModel: any, target?: any, deserializeAll: boolean = false): any {
    if (isObservableArray(target) && target() != null && target().length === viewModel.length) {
        updateArrayItems(viewModel, target, deserializeAll);
    } else {
        target = rebuildArrayFromScratch(viewModel, target, deserializeAll);
    }
    return target;
}

function rebuildArrayFromScratch(viewModel: any, target: any, deserializeAll: boolean) {
    const array: Array<KnockoutObservable<any>> = [];
    for (let i = 0; i < viewModel.length; i++) {
        array.push(wrapObservableObjectOrArray(deserialize(ko.unwrap(viewModel[i]), {}, deserializeAll)));
    }
    if (ko.isObservable(target)) {
        target = extendToObservableArrayIfRequired(target);
        target(array);
    } else {
        target = array;
    }
    return target;
}

function updateArrayItems(viewModel: any, target: KnockoutObservable<any>, deserializeAll: boolean) {
    const targetArray = target();
    for (let i = 0; i < viewModel.length; i++) {
        const targetItem = ko.unwrap(targetArray[i]);
        const deserialized = deserialize(ko.unwrap(viewModel[i]), targetItem, deserializeAll);

        if (targetItem !== deserialized) {
            // update the item
            if (ko.isObservable(targetArray[i])) {
                if (targetArray[i]() !== deserialized) {
                    targetArray[i] = extendToObservableArrayIfRequired(targetArray[i]);
                    targetArray[i](deserialized);
                }
            } else {
                targetArray[i] = wrapObservableObjectOrArray(deserialized);
            }
        }
    }
}

export function deserializeObject(viewModel: any, target: any, deserializeAll: boolean): any {
    let unwrappedTarget = ko.unwrap(target);

    if (isPrimitive(unwrappedTarget)) {
        unwrappedTarget = {};
    }

    for (const prop of Object.getOwnPropertyNames(viewModel)) {
        if (isOptionsProperty(prop)) {
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

        copyProperty(value, unwrappedTarget, prop, deserializeAll, options);
    }

    // copy the property options metadata
    for (const prop of Object.getOwnPropertyNames(viewModel)) {
        if (!isOptionsProperty(prop)) {
            continue;
        }

        copyPropertyMetadata(unwrappedTarget, prop, viewModel);
    }

    if (ko.isObservable(target)) {
        // this is so that if we have already updated the instance inside target observable
        // there's no need to force update.
        if (unwrappedTarget !== target()) {
            target(unwrappedTarget);
        }
    } else {
        target = unwrappedTarget;
    }
    return target;
}

function copyProperty(value: any, unwrappedTarget: any, prop: string, deserializeAll: boolean, options: any) {
    const deserialized = deserialize(ko.unwrap(value), unwrappedTarget[prop], deserializeAll);
    if (value instanceof Date) {
        // if we get Date value from API, it was converted to string, but we should note that it was date to convert it back
        unwrappedTarget[prop + "$options"] = {
            ...unwrappedTarget[prop + "$options"],
            isDate: true
        };
    }

    // update the property
    if (ko.isObservable(deserialized)) { // deserialized is observable <=> its input target is observable
        if (deserialized() !== unwrappedTarget[prop]()) {
            unwrappedTarget[prop] = extendToObservableArrayIfRequired(unwrappedTarget[prop]);
            unwrappedTarget[prop](deserialized());
        }
    } else {
        unwrappedTarget[prop] = wrapObservableObjectOrArray(deserialized);
    }

    if (options && options.clientExtenders && ko.isObservable(unwrappedTarget[prop])) {
        for (let j = 0; j < options.clientExtenders.length; j++) {
            const extenderOptions: any = {};
            const extenderInfo = options.clientExtenders[j];
            extenderOptions[extenderInfo.name] = extenderInfo.parameter;
            unwrappedTarget[prop].extend(extenderOptions);
        }
    }
}

function copyPropertyMetadata(unwrappedTarget: any, prop: string, viewModel: any) {
    unwrappedTarget[prop] = {
        ...unwrappedTarget[prop],
        ...viewModel[prop]
    }
    const originalName = prop.substring(0, prop.length - "$options".length);
    if (unwrappedTarget[originalName] === undefined) {
        unwrappedTarget[originalName] = ko.observable();
    }
}

function extendToObservableArrayIfRequired(observable: any) {
    if (!ko.isObservable(observable)) {
        throw new Error("Trying to extend a non-observable to an observable array.");
    }

    if (!isObservableArray(observable)) {
        ko.utils.extend(observable, ko.observableArray['fn']);
        observable = observable.extend({ trackArrayChanges: true });
    }
    return observable;
}

function isOptionsProperty(prop: string) {
    return /\$options$/.test(prop);
}
