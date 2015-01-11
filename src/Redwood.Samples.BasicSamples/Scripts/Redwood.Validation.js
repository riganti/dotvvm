var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
if (!redwood) {
    throw "Redwood.js is required!";
}
redwood.extensions.validation = redwood.extensions.validation || {
    rules: [
    ]
};
var RedwoodValidationContext = (function () {
    function RedwoodValidationContext() {
    }
    return RedwoodValidationContext;
})();
var RedwoodValidator = (function () {
    function RedwoodValidator(context) {
        var _this = this;
        this.context = context;
        this.isValid = ko.computed(function () { return redwood.extensions.validation.rules[_this.ruleName].isValid(_this.context.observable); });
        this.errorMessage = ko.observable();
    }
    return RedwoodValidator;
})();
var RedwoodValidationRuleBase = (function () {
    function RedwoodValidationRuleBase() {
    }
    RedwoodValidationRuleBase.prototype.isValid = function (context) {
        return false;
    };
    RedwoodValidationRuleBase.prototype.isEmpty = function (value) {
        return value || !/^\s*/.test(value);
    };
    return RedwoodValidationRuleBase;
})();
var RedwoodRequiredValidator = (function (_super) {
    __extends(RedwoodRequiredValidator, _super);
    function RedwoodRequiredValidator() {
        _super.apply(this, arguments);
    }
    RedwoodRequiredValidator.prototype.isValid = function (context) {
        var value = context.observable();
        return !this.isEmpty(value);
    };
    return RedwoodRequiredValidator;
})(RedwoodValidationRuleBase);
var RedwoodRegularExpressionValidator = (function (_super) {
    __extends(RedwoodRegularExpressionValidator, _super);
    function RedwoodRegularExpressionValidator() {
        _super.apply(this, arguments);
    }
    RedwoodRegularExpressionValidator.prototype.isValid = function (context) {
        var value = context.observable();
        var expr = context.arguments[0];
        return this.isEmpty(value) || new RegExp(expr).test(value);
    };
    return RedwoodRegularExpressionValidator;
})(RedwoodValidationRuleBase);
//# sourceMappingURL=Redwood.Validation.js.map