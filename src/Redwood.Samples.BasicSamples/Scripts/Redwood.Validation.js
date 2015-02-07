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
var RedwoodPropertyValidator = (function (_super) {
    __extends(RedwoodPropertyValidator, _super);
    function RedwoodPropertyValidator(validation) {
        _super.call(this);
        this.validation = validation;
    }
    RedwoodPropertyValidator.prototype.isValid = function (context) {
        var val = context.valueToValidate;
        var type = val["$type"];
        if (type == null)
            return true;
        return this.validation.validateTypedObject(val, type);
    };
    return RedwoodPropertyValidator;
})(RedwoodValidatorBase);
var RedwoodCollectionValidator = (function (_super) {
    __extends(RedwoodCollectionValidator, _super);
    function RedwoodCollectionValidator(validation) {
        _super.call(this);
        this.validation = validation;
    }
    RedwoodCollectionValidator.prototype.isValid = function (context) {
        var _this = this;
        var col = context.valueToValidate;
        var type = context.parameters[0];
        if (type == null)
            return true;
        return col.every(function (i) { return _this.validation.validateTypedObject(i, type); });
    };
    return RedwoodCollectionValidator;
})(RedwoodValidatorBase);
var ValidationError = (function () {
    function ValidationError(targetObservable) {
        var _this = this;
        this.targetObservable = targetObservable;
        this.errorMessage = ko.observable("");
        this.isValid = ko.computed(function () { return !!_this.errorMessage(); });
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
            "regularExpression": new RedwoodRegularExpressionValidator(),
            //"numeric": new RedwoodNumericValidator(),
            //"datetime": new RedwoodDateTimeValidator(),
            //"range": new RedwoodRangeValidator()
            "validate": new RedwoodPropertyValidator(this),
            "collection": new RedwoodCollectionValidator(this),
        };
        this.errors = ko.observableArray([]);
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
    }
    /// Validates the specified view model
    RedwoodValidation.prototype.validateViewModel = function (viewModel, viewModelName) {
        var _this = this;
        if (viewModelName === void 0) { viewModelName = "root"; }
        var rules = redwood.viewModels.root.validationRules[viewModelName];
        rules.forEach(function (rule) {
            var viewModelProperty = viewModel[rule.propertyName];
            if (!viewModelProperty || !ko.isObservable(viewModelProperty))
                return;
            var value = viewModelProperty();
            _this.validateProperty(viewModel, viewModelProperty, value, rule);
        });
    };
    RedwoodValidation.prototype.validateTypedObject = function (obj, type) {
        var _this = this;
        if (!obj)
            return;
        if (!type)
            return;
        var ecount = this.errors.length;
        var rulesForType = redwood.viewModels.root.validationRules.types[type];
        if (!rulesForType)
            return;
        for (var property in obj) {
            if (!obj.hasOwnProperty(property) || property.indexOf("$") >= 0)
                continue;
            var viewModelProperty = obj[property];
            if (!viewModelProperty || !ko.isObservable(viewModelProperty))
                continue;
            var value = obj[property]();
            // run validation rules
            if (rulesForType.hasOwnProperty(property)) {
                rulesForType[property].forEach(function (r) { return _this.validateProperty(obj, viewModelProperty, value, r); });
            }
        }
        return ecount == this.errors.length;
    };
    /// Validates the specified property in the viewModel
    RedwoodValidation.prototype.validateProperty = function (viewModel, property, value, rule) {
        var ruleTemplate = this.rules[rule.ruleName];
        var context = new RedwoodValidationContext(value, viewModel, rule.parameters);
        var validationError = ValidationError.getOrCreate(property);
        viewModel.$validationErrors.remove(validationError);
        this.errors.remove(validationError);
        if (!ruleTemplate.isValid(context)) {
            // add error message
            validationError.errorMessage(rule.errorMessage);
            viewModel.$validationErrors.push(validationError);
            this.errors.push(validationError);
        }
        else {
            // remove
            validationError.errorMessage("");
        }
    };
    // clears validation errors
    RedwoodValidation.prototype.clearValidationErrors = function (validationTarget) {
        if (!validationTarget.$validationErrors || !ko.isObservable(validationTarget.$validationErrors))
            return;
        var errors = validationTarget.$validationErrors();
        for (var i = 0; i < errors.length; i++) {
            errors[i].errorMessage("");
        }
        this.errors.removeAll(errors);
    };
    // merge validation rules
    RedwoodValidation.prototype.mergeValidationRules = function (args) {
        this.clearValidationErrors(args.viewModel);
        if (args.serverResponseObject.validationRules) {
            var existingRules = redwood.viewModels[args.viewModelName].validationRules;
            for (var type in args.serverResponseObject) {
                if (!args.serverResponseObject.hasOwnProperty(type))
                    continue;
                existingRules[type] = args.serverResponseObject[type];
            }
        }
    };
    // shows the validation errors from server
    RedwoodValidation.prototype.showValidationErrorsFromServer = function (args) {
        // resolve validation target
        var context = ko.contextFor(args.sender);
        var validationTarget = redwood.evaluateOnViewModel(context, args.validationTargetPath);
        // add validation errors
        this.clearValidationErrors(redwood.evaluateOnViewModel(ko.contextFor(args.sender), args.validationTargetPath));
        var modelState = args.serverResponseObject.modelState;
        for (var i = 0; i < modelState.length; i++) {
            // find the observable property
            var propertyPath = modelState[i].propertyPath;
            var observable = redwood.evaluateOnViewModel(validationTarget, propertyPath);
            var parent = redwood.evaluateOnViewModel(context, propertyPath.substring(0, propertyPath.lastIndexOf(".")) || "$data");
            if (!ko.isObservable(observable) || !parent || !parent.$validationErrors) {
                throw "Invalid validation path!";
            }
            // add the error to appropriate collections
            var error = ValidationError.getOrCreate(observable);
            error.errorMessage(modelState[i].errorMessage);
            if (parent.$validationErrors.indexOf(error) < 0) {
                parent.$validationErrors.push(error);
            }
            this.errors.push(error);
        }
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
        var validationTarget = redwood.evaluateOnViewModel(context, args.validationTargetPath);
        // validate the object
        redwood.extensions.validation.clearValidationErrors(validationTarget);
        redwood.extensions.validation.validateViewModel(validationTarget);
        if (redwood.extensions.validation.errors().length > 0) {
            args.cancel = true;
            return true;
        }
    }
    return false;
});
redwood.events.afterPostback.subscribe(function (args) {
    if (args.serverResponseObject.action === "successfulCommand") {
        // merge validation rules from postback with those we already have (required when a new type appears in the view model)
        redwood.extensions.validation.mergeValidationRules(args);
        return false;
    }
    else if (args.serverResponseObject.action === "validationErrors") {
        // apply validation errors from server
        redwood.extensions.validation.showValidationErrorsFromServer(args);
        return true;
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