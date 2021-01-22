import { serializeDate } from './date'
import { isObservableArray, wrapObservableObjectOrArray } from '../utils/knockout'
import { isPrimitive, keys } from '../utils/objects'
import { getObjectTypeInfo } from '../metadata/typeMap';

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

    let typeId = ko.unwrap(viewModel["$type"]);
    if (!typeId && unwrappedTarget)  {
        typeId = ko.unwrap(unwrappedTarget["$type"]);
    }

    if (isPrimitive(unwrappedTarget)) {
        unwrappedTarget = {};
    }

    let typeInfo;
    if (typeId) {
        typeInfo = getObjectTypeInfo(typeId);

        if (!ko.isObservable(unwrappedTarget["$type"])) {
            unwrappedTarget["$type"] = ko.observable(typeId);
        } else {
            unwrappedTarget["$type"](typeId);
        }
    } 

    for (const prop of keys(viewModel)) {
        if (isTypeIdProperty(prop)) {
            continue;
        }

        if (typeof unwrappedTarget[prop] === "undefined") {
            unwrappedTarget[prop] = ko.observable();
        }

        const value = viewModel[prop];
        if (typeof (value) == "undefined") {
            continue;
        }
        if (!ko.isObservable(value) && typeof (value) === "function") {
            continue;
        }

        const propInfo = typeInfo?.properties[prop];
        if (!deserializeAll && propInfo && propInfo.update == "no") {
            continue;
        }

        copyProperty(value, unwrappedTarget, prop, deserializeAll, propInfo);
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

function copyProperty(value: any, unwrappedTarget: any, prop: string, deserializeAll: boolean, propInfo?: PropertyMetadata) {
    const deserialized = deserialize(ko.unwrap(value), unwrappedTarget[prop], deserializeAll);
    
    if (ko.isObservable(deserialized)) { // deserialized is observable <=> its input target is observable
        if (deserialized() !== unwrappedTarget[prop]()) {
            unwrappedTarget[prop] = extendToObservableArrayIfRequired(unwrappedTarget[prop]);
            unwrappedTarget[prop](deserialized());
        }
    } else {
        unwrappedTarget[prop] = wrapObservableObjectOrArray(deserialized);
    }

    if (propInfo && propInfo.clientExtenders && ko.isObservable(unwrappedTarget[prop])) {
        for (let j = 0; j < propInfo.clientExtenders.length; j++) {
            const extenderOptions: any = {};
            const extenderInfo = propInfo.clientExtenders[j];
            extenderOptions[extenderInfo.name] = extenderInfo.parameter;
            unwrappedTarget[prop].extend(extenderOptions);
        }
    }
}

export function extendToObservableArrayIfRequired(observable: any) {
    if (!ko.isObservable(observable)) {
        throw new Error("Trying to extend a non-observable to an observable array.");
    }

    if (!isObservableArray(observable)) {
        ko.utils.extend(observable, ko.observableArray['fn']);
        observable = observable.extend({ trackArrayChanges: true });
    }
    return observable;
}

function isTypeIdProperty(prop: string) {
    return prop == "$type";
}
