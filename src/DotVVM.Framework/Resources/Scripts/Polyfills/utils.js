if (!Math.trunc) {
    Math.trunc = function (v) {
        return v < 0 ? Math.ceil(v) : Math.floor(v);
    };
}
if (!String.prototype.includes) {
    String.prototype.includes = function (search, start) {
        'use strict';

        if (search instanceof RegExp) {
            throw TypeError('first argument must not be a RegExp');
        }
        if (start === undefined) { start = 0; }
        return this.indexOf(search, start) !== -1;
    };
}
if (!String.prototype.endsWith) {
    String.prototype.endsWith = function (search, this_len) {
        if (this_len === undefined || this_len > this.length) {
            this_len = this.length;
        }
        return this.substring(this_len - search.length, this_len) === search;
    };
}
if (!String.prototype.startsWith) {
    String.prototype.startsWith = function (search, rawPos) {
        var pos = rawPos > 0 ? rawPos | 0 : 0;
        return this.substring(pos, pos + search.length) === search;
    }
}
if (!String.prototype.trimStart) {
    String.prototype.trimStart = function () {
        if (this == null) {
            throw new TypeError('"this" is null or not defined');
        }
        return String(this).replace(/^\s*/, "");
    }
}
if (!String.prototype.trimEnd) {
    String.prototype.trimEnd = function () {
        if (this == null) {
            throw new TypeError('"this" is null or not defined');
        }
        return String(this).replace(/\s*$/, "");
    }
}
if (!String.prototype.padStart) {
    String.prototype.padStart = function (length, char) {
        if (this == null) {
            throw new TypeError('"this" is null or not defined');
        }
        var string = String(this);
        if (length == null || length <= string.length) {
            return string;
        }
        if (char == null) {
            char = " ";
        }
        return Array(length - string.length + 1).join(char) + string;
    }
}
if (!String.prototype.padEnd) {
    String.prototype.padEnd = function (length, char) {
        if (this == null) {
            throw new TypeError('"this" is null or not defined');
        }
        var string = String(this);
        if (length == null || length <= string.length) {
            return string;
        }
        if (char == null) {
            char = " ";
        }
        return string + Array(length - string.length + 1).join(char);
    }
}
if (!Array.prototype.includes) {
    Object.defineProperty(Array.prototype, 'includes', {
        value: function (searchElement, fromIndex) {

            // 1. Let O be ? ToObject(this value).
            if (this == null) {
                throw new TypeError('"this" is null or not defined');
            }

            var o = Object(this);

            // 2. Let len be ? ToLength(? Get(O, "length")).
            var len = o.length >>> 0;

            // 3. If len is 0, return false.
            if (len === 0) {
                return false;
            }

            // 4. Let n be ? ToInteger(fromIndex).
            //    (If fromIndex is undefined, this step produces the value 0.)
            var n = fromIndex | 0;

            // 5. If n ≥ 0, then
            //  a. Let k be n.
            // 6. Else n < 0,
            //  a. Let k be len + n.
            //  b. If k < 0, let k be 0.
            var k = Math.max(n >= 0 ? n : len - Math.abs(n), 0);

            function sameValueZero(x, y) {
                return x === y || (typeof x === 'number' && typeof y === 'number' && isNaN(x) && isNaN(y));
            }

            // 7. Repeat, while k < len
            while (k < len) {
                // a. Let elementK be the result of ? Get(O, ! ToString(k)).
                // b. If SameValueZero(searchElement, elementK) is true, return true.
                // c. Increase k by 1.
                if (sameValueZero(o[k], searchElement)) {
                    return true;
                }
                k++;
            }

            // 8. Return false
            return false;
        }
    });
}
