(function (factory) {
    if (typeof require === "function" && typeof exports === "object" && typeof module === "object") {
        // CommonJS or Node: hard-coded dependency on "knockout"
        factory(require("knockout"), exports);
    } else if (typeof define === "function" && define["amd"]) {
        // AMD anonymous module with hard-coded dependency on "knockout"
        define(["knockout", "exports"], factory);
    } else {
        // <script> tag: use the global `ko` object, attaching a `mapper` property
        factory(ko, ko.mapper = {});
    }
})(function (ko, exports) {
    exports.fromJS = function (value, options, target, wrap) {
        var handler = "auto";

        if (options && options.$fromJS)
            options = options.$fromJS;

        if (options) {
            if (options.$handler) {
                handler = options.$handler.fromJS || options.$handler;
            } else if (getType(options) == "string") {
                handler = options;
            }
        } else {
            options = {};
        }

        if (typeof (handler) == 'function')
            return handler(value, options, target, wrap);
        else
            return exports.handlers[handler].fromJS(value, options, target, wrap);
    };

    exports.toJS = function (value, options) {
        var handler = "auto";

        if (options && options.$toJS)
            options = options.$toJS;

        if (options) {
            if (options.$handler) {
                handler = options.$handler.toJS || options.$handler;
            } else if (getType(options) == "string") {
                handler = options;
            }
        } else {
            options = {};
        }

        if (typeof (handler) == 'function')
            return handler(value, options);
        else
            return exports.handlers[handler].toJS(value, options);
    };

    exports.fromJSON = function (value, options, target, wrap) {
        return exports.fromJS(ko.utils.parseJson(value), options, target, wrap);
    };

    exports.toJSON = function (value, options) {
        return ko.utils.stringifyJson(exports.toJS(value, options));
    };

    exports.resolveFromJSHandler = function (value, options, target, wrap) {
        var type = getType(value);
        if (type == "array") return 'array';
        if (type == "object") return 'object';

        return 'value';
    };

    exports.resolveToJSHandler = function (observable, options) {
        var value = ko.utils.unwrapObservable(observable);

        var type = getType(value);
        if (type == "array") return 'array';
        if (type == "object") return 'object';

        return 'value';
    };

    exports.handlers = {};

    exports.handlers.auto = {
        fromJS: function (value, options, target, wrap) {
            var handler = exports.resolveFromJSHandler(value, options, target, wrap);
            return exports.handlers[handler].fromJS(value, options, target, wrap);
        },
        toJS: function (observable, options) {
            var handler = exports.resolveToJSHandler(observable, options);
            return exports.handlers[handler].toJS(observable, options);
        }
    };

    exports.ignore = {};

    exports.handlers.ignore = {
        fromJS: function (value, options, target, wrap) {
            return exports.ignore;
        },
        toJS: function (observable, options) {
            return exports.ignore;
        }
    };

    exports.handlers.copy = {
        fromJS: function (value, options, target, wrap) {
            return value;
        },
        toJS: function (value, options) {
            return value;
        }
    };

    exports.handlers.value = {
        fromJS: function (value, options, target, wrap) {
            if (ko.isObservable(target) && (wrap || wrap == undefined || wrap == null)) {
                target(value);
                return target;
            } else if (wrap) {
                return ko.observable(value);
            } else {
                return value;
            }
        },
        toJS: function (observable, options) {
            return ko.utils.unwrapObservable(observable);
        }
    };
    exports.handlers.array = {
        fromJS: function (value, options, target, wrap) {
            var targetArray = ko.utils.unwrapObservable(target);

            var array;

            var findItems = options.$key && targetArray;

            var itemOptions = options.$itemOptions;
            if (typeof itemOptions == 'function') itemOptions = itemOptions();

            if (options.$merge) {
                array = targetArray || [];
                for (var i = 0; i < value.length; i++) {
                    var item = findItems ? find(targetArray, options.$key, value[i]) : null;

                    var val = exports.fromJS(value[i], itemOptions, item);
                    if (val !== exports.ignore && !item) {
                        array.push(val);
                    }
                }
            } else {
                array = [];
                for (var i = 0; i < value.length; i++) {
                    var item = findItems ? find(targetArray, options.$key, value[i]) : null;

                    var val = exports.fromJS(value[i], itemOptions, item);
                    if (val !== exports.ignore) {
                        array.push(val);
                    }
                }
            }

            if (wrap || wrap == undefined || wrap == null) {
                if (ko.isObservable(target)) {
					target(array);
					return target;
				} else {
					return ko.observableArray(array);
				}
            } else {
                return array;
            }
        },
        toJS: function (observable, options) {
            var value = ko.utils.unwrapObservable(observable);
            var arr = [];
            for (var i = 0; i < value.length; i++) {
                var itemOptions = options.$itemOptions;
                if (typeof itemOptions == 'function') itemOptions = itemOptions(observable, options);

                var val = exports.toJS(value[i], itemOptions);
                if (val !== exports.ignore) {
                    arr.push(val);
                }
            }
            return arr;
        }
    };
    exports.handlers.object = {
        fromJS: function (value, options, target, wrap) {
            var obj = ko.utils.unwrapObservable(target);

            if (!obj) {
                if (options.$type) obj = new options.$type;
                else obj = {};
            }
            for (var p in value) {
                var val = exports.fromJS(value[p], options[p] || options.$default, obj[p], true);
                if (val !== exports.ignore && obj[p] != val) {
                    obj[p] = val;
                }
            }
            if (ko.isObservable(target) && (wrap || wrap == undefined || wrap == null)) {
                target(obj);
                return target;
            } else if (wrap) {
                return ko.observable(obj);
            } else {
                return obj;
            }
        },
        toJS: function (observable, options) {
            var value = ko.utils.unwrapObservable(observable);
            var obj = {};
            for (var p in value) {
                var val = exports.toJS(value[p], options[p] || options.$default);
                if (val !== exports.ignore) {
                    obj[p] = val;
                }
            }
            return obj;
        }
    };

    function find(array, key, data) {
        if (typeof key === 'function') {
            var value = key(data);
            for (var i = 0; i < array.length; i++) {
                var itemValue = key(array[i]);
                if (itemValue == value) {
                    return array[i];
                }
            }
        } else {
            var value = data[key];
            for (var i = 0; i < array.length; i++) {
                var itemValue = ko.utils.unwrapObservable(array[i][key]);
                if (itemValue == value) {
                    return array[i];
                }
            }
        }
        return null;
    }

    function getType(x) {
        if (x == null) return null;
        if (x instanceof Array) return "array";
        if (x instanceof Date) return "date";
        if (x instanceof RegExp) return "regex";
        return typeof x;
    }
});