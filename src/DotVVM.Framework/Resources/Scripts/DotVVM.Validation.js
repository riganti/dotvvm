/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout.mapper/knockout.mapper.d.ts" />
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
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
        return value == null || (typeof value == "string" && value.trim() == "");
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
var DotvvmIntRangeValidator = (function (_super) {
    __extends(DotvvmIntRangeValidator, _super);
    function DotvvmIntRangeValidator() {
        _super.apply(this, arguments);
    }
    DotvvmIntRangeValidator.prototype.isValid = function (context) {
        var val = context.valueToValidate;
        var from = context.parameters[0];
        var to = context.parameters[1];
        return val % 1 === 0 && val >= from && val <= to;
    };
    return DotvvmIntRangeValidator;
})(DotvvmValidatorBase);
var DotvvmRangeValidator = (function (_super) {
    __extends(DotvvmRangeValidator, _super);
    function DotvvmRangeValidator() {
        _super.apply(this, arguments);
    }
    DotvvmRangeValidator.prototype.isValid = function (context) {
        var val = context.valueToValidate;
        var from = context.parameters[0];
        var to = context.parameters[1];
        return val >= from && val <= to;
    };
    return DotvvmRangeValidator;
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
            "regularExpression": new DotvvmRegularExpressionValidator(),
            "intrange": new DotvvmIntRangeValidator(),
            "range": new DotvvmRangeValidator(),
        };
        this.errors = ko.observableArray([]);
        this.events = {
            validationErrorsChanged: new DotvvmEvent("dotvvm.extensions.validation.events.validationErrorsChanged")
        };
        this.elementUpdateFunctions = {
            // shows the element when it is valid
            hideWhenValid: function (element, errorMessage, param) {
                if (errorMessage) {
                    element.style.display = "";
                }
                else {
                    element.style.display = "none";
                }
            },
            // adds a CSS class when the element is not valid
            invalidCssClass: function (element, errorMessage, param) {
                if (errorMessage) {
                    element.className += " " + param;
                }
                else {
                    element.className = element.className.split(' ').filter(function (c) { return c != param; }).join(' ');
                }
            },
            // sets the error message as the title attribute
            setToolTipText: function (element, errorMessage, param) {
                if (errorMessage) {
                    element.title = errorMessage;
                }
                else {
                    element.title = "";
                }
            },
            // displays the error message
            showErrorMessageText: function (element, errorMessage, param) {
                element[element.innerText ? "innerText" : "textContent"] = errorMessage;
            }
        };
    }
    /// Validates the specified view model
    DotvvmValidation.prototype.validateViewModel = function (viewModel) {
        if (!viewModel || !dotvvm.viewModels['root'].validationRules)
            return;
        // find validation rules
        var type = ko.unwrap(viewModel.$type);
        if (!type)
            return;
        var rulesForType = dotvvm.viewModels['root'].validationRules[type] || {};
        // validate all properties
        for (var property in viewModel) {
            if (!viewModel.hasOwnProperty(property) || property.indexOf("$") === 0)
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
                    // handle collections
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
        }
    };
    // merge validation rules
    DotvvmValidation.prototype.mergeValidationRules = function (args) {
        if (args.serverResponseObject.validationRules) {
            var existingRules = dotvvm.viewModels[args.viewModelName].validationRules;
            if (typeof existingRules === "undefined") {
                dotvvm.viewModels[args.viewModelName].validationRules = {};
                existingRules = dotvvm.viewModels[args.viewModelName].validationRules;
            }
            for (var type in args.serverResponseObject) {
                if (!args.serverResponseObject.hasOwnProperty(type))
                    continue;
                existingRules[type] = args.serverResponseObject[type];
            }
        }
    };
    DotvvmValidation.prototype.clearValidationErrors = function (viewModel) {
        this.clearValidationErrorsCore(viewModel);
        var errors = this.errors();
        for (var i = 0; i < errors.length; i++) {
            errors[i].errorMessage("");
        }
        this.errors.removeAll();
    };
    DotvvmValidation.prototype.clearValidationErrorsCore = function (viewModel) {
        viewModel = ko.unwrap(viewModel);
        if (!viewModel || !viewModel.$type)
            return;
        // clear validation errors
        if (viewModel.$validationErrors) {
            viewModel.$validationErrors.removeAll();
        }
        else {
            viewModel.$validationErrors = ko.observableArray([]);
        }
        // validate all properties
        for (var property in viewModel) {
            if (!viewModel.hasOwnProperty(property) || property.indexOf("$") === 0)
                continue;
            var viewModelProperty = viewModel[property];
            if (!viewModelProperty || !ko.isObservable(viewModelProperty))
                continue;
            var value = viewModel[property]();
            if (value) {
                if (Array.isArray(value)) {
                    // handle collections
                    for (var i = 0; i < value.length; i++) {
                        this.clearValidationErrorsCore(value[i]);
                    }
                }
                else if (value.$type) {
                    // handle nested objects
                    this.clearValidationErrorsCore(value);
                }
            }
        }
    };
    // get validation errors
    DotvvmValidation.prototype.getValidationErrors = function (viewModel, recursive) {
        viewModel = ko.unwrap(viewModel);
        if (!viewModel || !viewModel.$type || !viewModel.$validationErrors)
            return [];
        var errors = viewModel.$validationErrors();
        if (recursive) {
            // get child validation errors
            for (var property in viewModel) {
                if (!viewModel.hasOwnProperty(property) || property.indexOf("$") === 0)
                    continue;
                var viewModelProperty = viewModel[property];
                if (!viewModelProperty || !ko.isObservable(viewModelProperty))
                    continue;
                var value = viewModel[property]();
                if (value) {
                    if (Array.isArray(value)) {
                        // handle collections
                        for (var i = 0; i < value.length; i++) {
                            errors = errors.concat(this.getValidationErrors(value[i], recursive));
                        }
                    }
                    else if (value.$type) {
                        // handle nested objects
                        errors = errors.concat(this.getValidationErrors(value, recursive));
                    }
                }
            }
        }
        return errors;
    };
    // shows the validation errors from server
    DotvvmValidation.prototype.showValidationErrorsFromServer = function (args) {
        // resolve validation target
        var context = ko.contextFor(args.sender);
        var validationTarget = dotvvm.evaluateOnViewModel(context, args.validationTargetPath);
        validationTarget = ko.unwrap(validationTarget);
        // add validation errors
        this.clearValidationErrors(args.viewModel);
        var modelState = args.serverResponseObject.modelState;
        for (var i = 0; i < modelState.length; i++) {
            // find the observable property
            var propertyPath = modelState[i].propertyPath;
            var observable = dotvvm.evaluateOnViewModel(validationTarget, propertyPath);
            var parentPath = propertyPath.substring(0, propertyPath.lastIndexOf("."));
            var parent = parentPath ? dotvvm.evaluateOnViewModel(validationTarget, parentPath) : validationTarget;
            parent = ko.unwrap(parent);
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
        if (viewModel.$validationErrors.indexOf(error) < 0) {
            viewModel.$validationErrors.push(error);
            this.errors.push(error);
        }
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
        dotvvm.extensions.validation.clearValidationErrors(args.viewModel);
        dotvvm.extensions.validation.validateViewModel(validationTarget);
        if (dotvvm.extensions.validation.errors().length > 0) {
            console.log("validation failed: postback aborted; errors: ", dotvvm.extensions.validation.errors());
            args.cancel = true;
            args.clientValidationFailed = true;
        }
    }
    dotvvm.extensions.validation.events.validationErrorsChanged.trigger(args);
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
    dotvvm.extensions.validation.events.validationErrorsChanged.trigger(args);
});
// add knockout binding handler
ko.bindingHandlers["dotvvmValidation"] = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var observableProperty = valueAccessor();
        if (ko.isObservable(observableProperty)) {
            // try to get the options
            var options = allBindingsAccessor.get("dotvvmValidationOptions");
            var updateFunction = function (element, errorMessage) {
                for (var option in options) {
                    if (options.hasOwnProperty(option)) {
                        dotvvm.extensions.validation.elementUpdateFunctions[option](element, errorMessage, options[option]);
                    }
                }
            };
            // subscribe to the observable property changes
            var validationError = ValidationError.getOrCreate(observableProperty);
            validationError.errorMessage.subscribe(function (newValue) { return updateFunction(element, newValue); });
            updateFunction(element, validationError.errorMessage());
        }
    }
};
//# sourceMappingURL=DotVVM.Validation.js.map