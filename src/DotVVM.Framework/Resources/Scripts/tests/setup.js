global.compileConstants = { isSpa: false, nomodules: false }
global.ko = require("../knockout-latest.debug")
global.dotvvm_Globalize = require("../Globalize/globalize")

const expect = require("expect")

expect.extend({
    observable(obj) {
        return { pass: ko.isObservable(obj), message: () => "Object was expected to be an observable." }
    },
    observableArray(obj) {
        return { pass: ko.isObservable(obj) && "removeAll" in obj, message: () => "Object was expected to be an observable array." }
    }
})
