import { DotVVM } from "./dotvvm-base"
import addPolyfills from './DotVVM.Polyfills'
import { events } from './DotVVM.Events'

if (compileConstants.nomodules) {
    addPolyfills()
}

if (window["dotvvm"]) {
    throw 'DotVVM is already loaded!';
}
var dotvvm: any = new DotVVM();
window["dotvvm"] = dotvvm;

export { events }


