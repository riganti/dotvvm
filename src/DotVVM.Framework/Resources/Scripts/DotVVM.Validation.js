/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout.mapper/knockout.mapper.d.ts" />
var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var DotvvmValidationContext = (function () {
    function DotvvmValidationContext(valueToValidate, parentViewModel, parameters) {
        this.valueToValidate = valueToValidate;
        this.parentViewModel = parentViewModel;
        this.parameters = parameters;
    }
    return DotvvmValidationContext;
})();
var DotvvmValidatorBase = (function () {
    function DotvvmValidatorBase() {
    }
    DotvvmValidatorBase.prototype.isValid = function (context) {
        return false;
    };
    DotvvmValidatorBase.prototype.isEmpty = function (value) {
        return value == null || value.trim() == "";
    };
    return DotvvmValidatorBase;
})();
var DotvvmRequiredValidator = (function (_super) {
    __extends(DotvvmRequiredValidator, _super);
    function DotvvmRequiredValidator() {
        _super.apply(this, arguments);
    }
    DotvvmRequiredValidator.prototype.isValid = function (context) {
        var value = context.valueToValidate;
        return !this.isEmpty(value);
    };
    return DotvvmRequiredValidator;
})(DotvvmValidatorBase);
var DotvvmRegularExpressionValidator = (function (_super) {
    __extends(DotvvmRegularExpressionValidator, _super);
    function DotvvmRegularExpressionValidator() {
        _super.apply(this, arguments);
    }
    DotvvmRegularExpressionValidator.prototype.isValid = function (context) {
        var value = context.valueToValidate;
        var expr = context.parameters[0];
        return this.isEmpty(value) || new RegExp(expr).test(value);
    };
    return DotvvmRegularExpressionValidator;
})(DotvvmValidatorBase);
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
var DotvvmValidation = (function () {
    function DotvvmValidation() {
        this.rules = {
            "required": new DotvvmRequiredValidator(),
            "regularExpression": new DotvvmRegularExpressionValidator()
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
                    element.className = element.className.split(' ').filter(function (c) { return c != cssClass; }).join(' ');
                }
            },
            // displays the error message
            displayErrorMessage: function (element, errorMessage, options) {
                element[element.innerText ? "innerText" : "textContent"] = errorMessage;
            }
        };
    }
    /// Validates the specified view model
    DotvvmValidation.prototype.validateViewModel = function (viewModel) {
        if (!viewModel || !viewModel.$type || !dotvvm.viewModels.root.validationRules)
            return;
        // find validation rules
        var type = ko.unwrap(viewModel.$type);
        if (!type)
            return;
        var rulesForType = dotvvm.viewModels.root.validationRules[type];
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
    DotvvmValidation.prototype.validateProperty = function (viewModel, property, value, rulesForProperty) {
        for (var i = 0; i < rulesForProperty.length; i++) {
            // validate the rules
            var rule = rulesForProperty[i];
            var ruleTemplate = this.rules[rule.ruleName];
            var context = new DotvvmValidationContext(value, viewModel, rule.parameters);
            var validationError = ValidationError.getOrCreate(property);
            if (!ruleTemplate.isValid(context)) {
                // add error message
                validationError.errorMessage(rule.errorMessage);
                this.addValidationError(viewModel, validationError);
            }
            else {
                // remove
                this.removeValidationError(viewModel, validationError);
                validationError.errorMessage("");
            }
        }
    };
    // clears validation errors
    DotvvmValidation.prototype.clearValidationErrors = function () {
        var errors = this.errors();
        for (var i = 0; i < errors.length; i++) {
            errors[i].errorMessage("");
        }
        this.errors.removeAll();
    };
    // merge validation rules
    DotvvmValidation.prototype.mergeValidationRules = function (args) {
        if (args.serverResponseObject.validationRules) {
            var existingRules = dotvvm.viewModels[args.viewModelName].validationRules;
            for (var type in args.serverResponseObject) {
                if (!args.serverResponseObject.hasOwnProperty(type))
                    continue;
                existingRules[type] = args.serverResponseObject[type];
            }
        }
    };
    // shows the validation errors from server
    DotvvmValidation.prototype.showValidationErrorsFromServer = function (args) {
        // resolve validation target
        var context = ko.contextFor(args.sender);
        var validationTarget = dotvvm.evaluateOnViewModel(context, args.validationTargetPath);
        // add validation errors
        this.clearValidationErrors();
        var modelState = args.serverResponseObject.modelState;
        for (var i = 0; i < modelState.length; i++) {
            // find the observable property
            var propertyPath = modelState[i].propertyPath;
            var observable = dotvvm.evaluateOnViewModel(validationTarget, propertyPath);
            var parentPath = propertyPath.substring(0, propertyPath.lastIndexOf("."));
            var parent = parentPath ? dotvvm.evaluateOnViewModel(validationTarget, parentPath) : validationTarget;
            if (!ko.isObservable(observable) || !parent || !parent.$validationErrors) {
                throw "Invalid validation path!";
            }
            // add the error to appropriate collections
            var error = ValidationError.getOrCreate(observable);
            error.errorMessage(modelState[i].errorMessage);
            this.addValidationError(parent, error);
        }
    };
    DotvvmValidation.prototype.addValidationError = function (viewModel, error) {
        this.removeValidationError(viewModel, error);
        viewModel.$validationErrors.push(error);
        this.errors.push(error);
    };
    DotvvmValidation.prototype.removeValidationError = function (viewModel, error) {
        var errorMessage = error.errorMessage();
        viewModel.$validationErrors.remove(function (e) { return e.errorMessage() === errorMessage; });
        this.errors.remove(error);
    };
    return DotvvmValidation;
})();
;
if (!dotvvm) {
    throw "DotVVM.js is required!";
}
dotvvm.extensions.validation = dotvvm.extensions.validation || new DotvvmValidation();
// perform the validation before postback
dotvvm.events.beforePostback.subscribe(function (args) {
    if (args.validationTargetPath) {
        // resolve target
        var context = ko.contextFor(args.sender);
        var validationTarget = dotvvm.evaluateOnViewModel(context, args.validationTargetPath);
        // validate the object
        dotvvm.extensions.validation.clearValidationErrors();
        dotvvm.extensions.validation.validateViewModel(validationTarget);
        if (dotvvm.extensions.validation.errors().length > 0) {
            args.cancel = true;
            args.clientValidationFailed = true;
        }
    }
});
dotvvm.events.afterPostback.subscribe(function (args) {
    if (!args.wasInterrupted && args.serverResponseObject) {
        if (args.serverResponseObject.action === "successfulCommand") {
            // merge validation rules from postback with those we already have (required when a new type appears in the view model)
            dotvvm.extensions.validation.mergeValidationRules(args);
            args.isHandled = true;
        }
        else if (args.serverResponseObject.action === "validationErrors") {
            // apply validation errors from server
            dotvvm.extensions.validation.showValidationErrorsFromServer(args);
            args.isHandled = true;
        }
    }
});
// add knockout binding handler
ko.bindingHandlers["dotvvmalidation"] = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var observableProperty = valueAccessor();
        if (ko.isObservable(observableProperty)) {
            // try to get the options
            var options = allBindingsAccessor.get("dotvvmalidationOptions");
            var mode = (options && options.mode) ? options.mode : "addCssClass";
            var updateFunction = dotvvm.extensions.validation.elementUpdateFunctions[mode];
            // subscribe to the observable property changes
            var validationError = ValidationError.getOrCreate(observableProperty);
            validationError.errorMessage.subscribe(function (newValue) { return updateFunction(element, newValue, options); });
            updateFunction(element, validationError.errorMessage(), options);
        }
    }
};
//# sourceMappingURL=DotVVM.Validation.js.map