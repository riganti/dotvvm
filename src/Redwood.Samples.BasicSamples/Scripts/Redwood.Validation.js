var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var RedwoodValidationContext = (function () {
    function RedwoodValidationContext(valueToValidate, parentViewModel, parameters) {
        this.valueToValidate = valueToValidate;
        this.parentViewModel = parentViewModel;
        this.parameters = parameters;
    }
    return RedwoodValidationContext;
})();
var RedwoodValidatorBase = (function () {
    function RedwoodValidatorBase() {
    }
    RedwoodValidatorBase.prototype.isValid = function (context) {
        return false;
    };
    RedwoodValidatorBase.prototype.isEmpty = function (value) {
        return value == null || /^\s*$/.test(value);
    };
    return RedwoodValidatorBase;
})();
var RedwoodRequiredValidator = (function (_super) {
    __extends(RedwoodRequiredValidator, _super);
    function RedwoodRequiredValidator() {
        _super.apply(this, arguments);
    }
    RedwoodRequiredValidator.prototype.isValid = function (context) {
        var value = context.valueToValidate;
        return !this.isEmpty(value);
    };
    return RedwoodRequiredValidator;
})(RedwoodValidatorBase);
var RedwoodRegularExpressionValidator = (function (_super) {
    __extends(RedwoodRegularExpressionValidator, _super);
    function RedwoodRegularExpressionValidator() {
        _super.apply(this, arguments);
    }
    RedwoodRegularExpressionValidator.prototype.isValid = function (context) {
        var value = context.valueToValidate;
        var expr = context.parameters[0];
        return this.isEmpty(value) || new RegExp(expr).test(value);
    };
    return RedwoodRegularExpressionValidator;
})(RedwoodValidatorBase);
var ValidationError = (function () {
    function ValidationError(targetObservable) {
        var _this = this;
        this.targetObservable = targetObservable;
        this.errorMessage = ko.observable("");
        this.isValid = ko.computed(function () { return _this.errorMessage(); });
    }
    ValidationError.getOrCreate = function (targetObservable) {
        if (!targetObservable["validationError"]) {
            targetObservable["validationError"] = new ValidationError(targetObservable);
        }
        return targetObservable["validationError"];
    };
    return ValidationError;
})();
var RedwoodValidation = (function () {
    function RedwoodValidation() {
        this.rules = {
            "required": new RedwoodRequiredValidator(),
            "regularExpression": new RedwoodRegularExpressionValidator()
        };
        this.elementUpdateFunctions = {
            // shows the element when it is not valid
            hideWhenValid: function (element, errorMessage, options) {
                if (errorMessage) {
                    element.style.display = "";
                    element.title = "";
                }
                else {
                    element.style.display = "none";
                    element.title = errorMessage;
                }
            },
            // adds a CSS class when the element is not valid
            addCssClass: function (element, errorMessage, options) {
                var cssClass = (options && options.cssClass) ? options.cssClass : "field-validation-error";
                if (errorMessage) {
                    element.className += " " + cssClass;
                }
                else {
                    element.className = element.className.replace(new RegExp("\\b" + cssClass + "\\b", "g"), "");
                }
            },
            // displays the error message
            displayErrorMessage: function (element, errorMessage, options) {
                element[element.innerText ? "innerText" : "textContent"] = errorMessage;
            }
        };
        this.errors = ko.observableArray([]);
    }
    /// Validates the specified view model
    RedwoodValidation.prototype.validateViewModel = function (viewModel) {
        if (!viewModel || !viewModel.$type)
            return;
        // find validation rules
        var type = ko.unwrap(viewModel.$type);
        if (!type)
            return;
        var rulesForType = redwood.viewModels.root.validationRules[type];
        if (!rulesForType)
            return;
        for (var property in viewModel) {
            if (!viewModel.hasOwnProperty(property) || property.indexOf("$") >= 0)
                continue;
            var viewModelProperty = viewModel[property];
            if (!viewModelProperty || !ko.isObservable(viewModelProperty))
                continue;
            var value = viewModel[property]();
            // run validation rules
            if (rulesForType.hasOwnProperty(property)) {
                this.validateProperty(viewModel, viewModelProperty, value, rulesForType[property]);
            }
            if (value) {
                if (Array.isArray(value)) {
                    for (var i = 0; i < value.length; i++) {
                        this.validateViewModel(value[i]);
                    }
                }
                else if (value.$type) {
                    // handle nested objects
                    this.validateViewModel(value);
                }
            }
        }
    };
    /// Validates the specified property in the viewModel
    RedwoodValidation.prototype.validateProperty = function (viewModel, property, value, rulesForProperty) {
        for (var i = 0; i < rulesForProperty.length; i++) {
            // validate the rules
            var rule = rulesForProperty[i];
            var ruleTemplate = this.rules[rule.ruleName];
            var context = new RedwoodValidationContext(value, viewModel, rule.parameters);
            var validationError = ValidationError.getOrCreate(property);
            if (!ruleTemplate.isValid(context)) {
                // add error message
                validationError.errorMessage(rule.errorMessage);
                this.errors.push(validationError);
            }
        }
    };
    // clears validation errors
    RedwoodValidation.prototype.clearValidationErrors = function () {
        var errors = this.errors();
        for (var i = 0; i < errors.length; i++) {
            errors[i].errorMessage("");
        }
        this.errors.removeAll();
    };
    return RedwoodValidation;
})();
;
if (!redwood) {
    throw "Redwood.js is required!";
}
redwood.extensions.validation = redwood.extensions.validation || new RedwoodValidation();
// perform the validation before postback
redwood.events.beforePostback.subscribe(function (args) {
    if (args.validationTargetPath) {
        // resolve target
        var context = ko.contextFor(args.sender);
        var validationTarget = eval("(function (c) { return c." + args.validationTargetPath + "; })")(context);
        // validate the object
        redwood.extensions.validation.clearValidationErrors();
        redwood.extensions.validation.validateViewModel(validationTarget);
        if (redwood.extensions.validation.errors().length > 0) {
            //args.cancel = true;
            return true;
        }
    }
    return false;
});
// merge validation rules from postback with those we already have (required when a new type appears in the view model)
redwood.events.afterPostback.subscribe(function (args) {
    if (args.serverResponseObject.validationRules) {
        var existingRules = redwood.viewModels[args.viewModelName].validationRules;
        for (var type in args.serverResponseObject) {
            if (!args.serverResponseObject.hasOwnProperty(type))
                continue;
            existingRules[type] = args.serverResponseObject[type];
        }
    }
    return false;
});
// add knockout binding handler
ko.bindingHandlers["redwoodValidation"] = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var observableProperty = valueAccessor();
        if (ko.isObservable(observableProperty)) {
            // try to get the options
            var options = allBindingsAccessor.get("redwoodValidationOptions");
            var mode = (options && options.mode) ? options.mode : "addCssClass";
            var updateFunction = redwood.extensions.validation.elementUpdateFunctions[mode];
            // subscribe to the observable property changes
            var validationError = ValidationError.getOrCreate(observableProperty);
            validationError.errorMessage.subscribe(function (newValue) { return updateFunction(element, newValue, options); });
            updateFunction(element, validationError.errorMessage(), options);
        }
    }
};
//# sourceMappingURL=Redwood.Validation.js.map