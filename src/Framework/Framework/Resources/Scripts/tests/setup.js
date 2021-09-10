// use polyfill promise, so Jest fake timers work
// see https://github.com/facebook/jest/issues/7151#issuecomment-429377276
// this PR should solve this in the future: https://github.com/facebook/jest/pull/6876
// global.Promise = require('promise');

global.compileConstants = { isSpa: true, nomodules: false }
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
