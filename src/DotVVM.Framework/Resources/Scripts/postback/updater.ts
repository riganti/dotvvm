import { getElementByDotvvmId } from '../utils/dom'
import { getViewModel, replaceViewModel, updateViewModelCache, clearViewModelCache } from '../dotvvm-base'
import { deserialize } from '../serialization/deserialize'
import { keys } from '../utils/objects';

const diffEqual = {};
let isViewModelUpdating: boolean = false;

export function getIsViewModelUpdating() {
    return isViewModelUpdating;
}

export function cleanUpdatedControls(resultObject: any, updatedControls: any = {}) {
    for (const id of keys(resultObject.updatedControls)) {
        const control = getElementByDotvvmId(id);
        if (control) {
            const dataContext = ko.contextFor(control);
            const nextSibling = control.nextSibling;
            const parent = control.parentNode;
            ko.removeNode(control);
            updatedControls[id] = { control: control, nextSibling: nextSibling, parent: parent, dataContext: dataContext };
        }
    }
    return updatedControls;
}

export function restoreUpdatedControls(resultObject: any, updatedControls: any, applyBindingsOnEachControl: boolean) {
    for (const id of keys(resultObject.updatedControls)) {
        const updatedControl = updatedControls[id];
        if (updatedControl) {
            const wrapper = document.createElement(updatedControls[id].parent.tagName || "div");
            wrapper.innerHTML = resultObject.updatedControls[id];
            if (wrapper.childElementCount > 1) {
                throw new Error("Postback.Update control can not render more than one element");
            }
            const element = wrapper.firstElementChild;
            if (element.id == null) {
                throw new Error("Postback.Update control always has to render id attribute.");
            }
            if (element.id !== updatedControls[id].control.id) {
                console.log(`Postback.Update control changed id from '${updatedControls[id].control.id}' to '${element.id}'`);
            }
            wrapper.removeChild(element);
            if (updatedControl.nextSibling) {
                updatedControl.parent.insertBefore(element, updatedControl.nextSibling);
            } else {
                updatedControl.parent.appendChild(element);
            }
        }
    }

    if (applyBindingsOnEachControl) {
        Promise.resolve(0).then(_ => {
            for (const id of keys(resultObject.updatedControls)) {
                const updatedControl = getElementByDotvvmId(id);
                if (updatedControl) {
                    ko.applyBindings(updatedControls[id].dataContext, updatedControl);
                }
            }
        });
    }
}

export function updateViewModelAndControls(resultObject: any) {
    try {
        isViewModelUpdating = true;

        // store server-side cached viewmodel
        if (resultObject.viewModelCacheId) {
            updateViewModelCache(resultObject.viewModelCacheId, resultObject.viewModel);
        } else {
            clearViewModelCache();
        }

        // remove updated controls
        const updatedControls = cleanUpdatedControls(resultObject);

        // update viewmodel
        replaceViewModel(resultObject.viewModel);

        // remove updated controls which were previously removed from DOM
        cleanUpdatedControls(resultObject, updatedControls);

        // add new updated controls
        restoreUpdatedControls(resultObject, updatedControls, true);
    }
    finally {
        isViewModelUpdating = false;
    }
}

export function patchViewModel(source: any, patch: any): any {
    if (source instanceof Array && patch instanceof Array) {
        return patch.map((val, i) => patchViewModel(source[i], val));
    }
    else if (source instanceof Array || patch instanceof Array) {
        return patch;
    }
    else if (typeof source == "object" && typeof patch == "object" && source && patch) {
        for (const p of keys(patch)) {
            if (patch[p] == null) {
                source[p] = null;
            } else if (source[p] == null) {
                source[p] = patch[p];
            } else {
                source[p] = patchViewModel(source[p], patch[p]);
            }
        }
        return source;
    }
    else {
        return patch;
    }
}

export function diffViewModel(source: any, modified: any): any {
    if (source instanceof Array && modified instanceof Array) {
        const diffArray = modified.map((el, index) => diffViewModel(source[index], el));
        if (source.length === modified.length
            && diffArray.every((el, index) => el === diffEqual || source[index] === modified[index])) {
            return diffEqual;
        } else {
            return diffArray;
        }
    }
    else if (source instanceof Array || modified instanceof Array) {
        return modified;
    }
    else if (typeof source == "object" && typeof modified == "object" && source && modified) {
        let result: any = diffEqual;
        for (const p in modified) {
            const propertyDiff = diffViewModel(source[p], modified[p]);
            if (propertyDiff !== diffEqual && source[p] !== modified[p]) {
                if (result === diffEqual) {
                    result = {};
                }
                result[p] = propertyDiff;
            } else if (p[0] === "$") {
                if (result == diffEqual) {
                    result = {};
                }
                result[p] = modified[p];
            }
        }
        return result;
    }
    else if (source === modified) {
        if (typeof source == "object") {
            return diffEqual;
        } else {
            return source;
        }
    } else {
        return modified;
    }
}
