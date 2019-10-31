import { DotVVM } from "./DotVVM"

if (window["dotvvm"]) {
    throw 'DotVVM is already loaded!';
}
window["dotvvm"] = new DotVVM();

