import { DotVVM } from "./DotVVM"
import addPolyfills from './DotVVM.Polyfills'

if (compileConstants.nomodules) {
    addPolyfills()
}

if (window["dotvvm"]) {
    throw 'DotVVM is already loaded!';
}
window["dotvvm"] = new DotVVM();

export var virtualDirectory = window["dotvvm"].viewModels["root"].virtualDirectory || "";
export var viewModel = window["dotvvm"].viewModels["root"].viewModel;
export var currentUrl = <string>window["dotvvm"].viewModels[viewModelName].url;
export var renderedResources = window["dotvvm"].viewModels["root"].renderedResources;
