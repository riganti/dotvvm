/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/knockout.mapper/knockout.mapper.d.ts" />
/// <reference path="Redwood.ts" />
var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var RedwoodValidationContext = (function () {
    function RedwoodValidationContext(valueToValidate, parentViewModel, parameters, viewModelName, path, constraints) {
        this.valueToValidate = valueToValidate;
        this.parentViewModel = parentViewModel;
        this.parameters = parameters;
        this.viewModelName = viewModelName;
        this.path = path;
        this.constraints = constraints;
    }
    return RedwoodValidationContext;
})();
var RedwoodValidatorBase = (function () {
    function RedwoodValidatorBase() {
        this.handlesConstraints = false;
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
        this.handlesConstraints = true;
    }
    RedwoodPropertyValidator.prototype.isValid = function (context) {
        var val = context.valueToValidate;
        var type = context.parameters[0];
        return val == null || this.validation.validateTypedObject(val, type, context.viewModelName, context.path, context.constraints);
    };
    return RedwoodPropertyValidator;
})(RedwoodValidatorBase);
var RedwoodCollectionValidator = (function (_super) {
    __extends(RedwoodCollectionValidator, _super);
    function RedwoodCollectionValidator(validation) {
        _super.call(this);
        this.validation = validation;
        this.handlesConstraints = true;
    }
    RedwoodCollectionValidator.prototype.isValid = function (context) {
        var _this = this;
        var col = context.valueToValidate;
        var type = context.parameters[0];
        return !col || col.every(function (item, index) { return _this.validation.validateTypedObject(item, type, context.viewModelName, context.path.concat(["[" + index + "]"]), context.constraints); });
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
    /**
    * Validates the specified view model
    * @param viewModel viewModel for validation
    * @param name of viewModel root (default "root")
    * @param groups array of active groups (default is only group '*')
    */
    RedwoodValidation.prototype.validateViewModel = function (viewModel, viewModelName, path, constraints) {
        var _this = this;
        if (viewModelName === void 0) { viewModelName = "root"; }
        if (path === void 0) { path = []; }
        if (constraints === void 0) { constraints = new ValidationConstraints(["*"], []); }
        var rules = redwood.viewModels[viewModelName].validationRules.rootRules;
        rules.forEach(function (rule) {
            if (!constraints.shouldValidate(rule, path))
                return;
            var viewModelProperty = viewModel[rule.propertyName];
            if (!viewModelProperty || !ko.isObservable(viewModelProperty))
                return;
            _this.validateProperty(viewModel, viewModelProperty, rule, viewModelName, path.concat(rule.propertyName), constraints);
        });
    };
    RedwoodValidation.prototype.validateTypedObject = function (obj, type, viewModelName, path, constraints) {
        var _this = this;
        if (!obj)
            return;
        if (!type)
            return;
        var ecount = this.errors.length;
        var rulesForType = redwood.viewModels[viewModelName].validationRules.types[type];
        if (!rulesForType)
            return;
        // validate all properties
        rulesForType.forEach(function (rule) {
            if (!constraints.shouldValidate(rule, path))
                return;
            var value;
            if (rule.propertyName) {
                if (!obj.hasOwnProperty(rule.propertyName))
                    return;
                value = obj[rule.propertyName];
            }
            else
                value = obj;
            _this.validateProperty(obj, value, rule, viewModelName, path.concat([rule.propertyName]), constraints);
        });
        return ecount == this.errors.length;
    };
    /**
    * Validates the specified property in the viewModel
    */
    RedwoodValidation.prototype.validateProperty = function (viewModel, property, rule, viewModelName, path, constraints) {
        var ruleTemplate = this.rules[rule.ruleName];
        var context = new RedwoodValidationContext(ko.isObservable(property) ? property() : property, viewModel, rule.parameters, viewModelName, path, constraints);
        var validationError = ValidationError.getOrCreate(property);
        viewModel.$validationErrors.remove(validationError);
        this.errors.remove(validationError);
        if (!ruleTemplate.isValid(context)) {
            // add error message
            validationError.errorMessage(rule.errorMessage);
            this.addValidationError(viewModel, validationError);
        }
        else {
            // remove
            validationError.errorMessage("");
        }
    };
    RedwoodValidation.prototype.clearValidationErrors = function (validationTarget) {
        if (!validationTarget.$validationErrors || !ko.isObservable(validationTarget.$validationErrors))
            return;
        var errors = validationTarget.$validationErrors();
        for (var i = 0; i < errors.length; i++) {
            errors[i].errorMessage("");
        }
        this.errors().forEach(function (e) { return e.errorMessage(""); });
        this.errors([]);
    };
    RedwoodValidation.prototype.mergeValidationRules = function (args) {
        // this.clearValidationErrors(args.viewModel);
        var rules = args.serverResponseObject.validationRules;
        if (rules) {
            var existingRules = redwood.viewModels[args.viewModelName].validationRules.types;
            for (var type in rules) {
                if (!rules.hasOwnProperty(type))
                    continue;
                existingRules[type] = rules[type];
            }
        }
    };
    RedwoodValidation.prototype.showValidationErrorsFromServer = function (args) {
        // resolve validation target
        var context = ko.contextFor(args.sender);
        var validationTarget = redwood.evaluateOnViewModel(args.viewModel, args.validationTargetPath);
        // add validation errors
        this.clearValidationErrors(redwood.evaluateOnViewModel(ko.contextFor(args.sender), args.validationTargetPath));
        var modelState = args.serverResponseObject.modelState;
        for (var i = 0; i < modelState.length; i++) {
            // find the observable property
            var propertyPath = modelState[i].propertyPath;
            var observable = redwood.evaluateOnViewModel(args.viewModel, propertyPath);
            var parent = ko.isObservable(observable) ? redwood.evaluateOnViewModel(args.viewModel, propertyPath.slice(0, propertyPath.length - 1)) : observable;
            if (observable == null || !parent) {
                throw "Invalid validation path!";
            }
            // add the error to appropriate collections
            var error = ValidationError.getOrCreate(observable);
            error.errorMessage(modelState[i].errorMessage);
            this.addValidationError(parent, error);
        }
    };
    RedwoodValidation.prototype.addValidationError = function (viewModel, error) {
        if (viewModel.$validationErrors.indexOf(error) < 0) {
            viewModel.$validationErrors.push(error);
        }
        this.errors.push(error);
    };
    RedwoodValidation.prototype.removeValidationError = function (viewModel, error) {
        viewModel.$validationErrors.remove(error);
        this.errors.remove(error);
    };
    RedwoodValidation.findActionNameInCommand = function (command) {
        if (!command)
            return "";
        var rgx = /([A-Za-z_]\w*)\(.*/;
        var match = rgx.exec(command);
        if (match) {
            return match[1];
        }
        return "";
    };
    return RedwoodValidation;
})();
;
var ValidationConstraints = (function () {
    function ValidationConstraints(activeGroups, pathPrefix) {
        var _this = this;
        this.pathPrefix = pathPrefix;
        this.activeGroups = {};
        this.activeGroups["**"] = true;
        if (activeGroups)
            activeGroups.forEach(function (g) { return _this.activeGroups[g] = true; });
        else
            this.activeGroups["*"] = true;
    }
    ValidationConstraints.prototype.shouldValidate = function (rule, path) {
        return redwood.extensions.validation.rules[rule.ruleName].handlesConstraints || (this.matchGroups(rule.groups) && this.matchPathPrefix(path));
    };
    ValidationConstraints.prototype.matchPathPrefix = function (path) {
        return path.length >= this.pathPrefix.length && this.pathPrefix.every(function (val, i) { return path[i] == val; });
    };
    ValidationConstraints.prototype.matchGroups = function (groupString) {
        var _this = this;
        if (groupString == null || groupString == "")
            groupString = "*";
        // TODO: accept some more complicated group strings
        var s = groupString.split(",");
        return s.some(function (g) { return _this.activeGroups[g]; });
    };
    return ValidationConstraints;
})();
if (!redwood) {
    throw "Redwood.js is required!";
}
redwood.extensions.validation = redwood.extensions.validation || new RedwoodValidation();
// perform the validation before postback
redwood.events.beforePostback.subscribe(function (args) {
    if (args.validationTargetPath) {
        var actionName = RedwoodValidation.findActionNameInCommand(args.command);
        var groups = redwood.viewModels[args.viewModelName].validationRules.actionGroups[actionName];
        // we can't use args.viewModelPath because is can contain complicated expressions
        var path = ko.unwrap(ko.contextFor(args.sender).$data.$path);
        var valPath = redwood.combinePaths(path, args.validationTargetPath);
        if (valPath[0] == "$root")
            valPath = valPath.slice(1);
        // validate the object
        redwood.extensions.validation.clearValidationErrors(args.viewModel);
        redwood.extensions.validation.validateViewModel(args.viewModel, args.viewModelName, [], new ValidationConstraints(groups, valPath));
        args.viewModel.$allValidationErrors(redwood.extensions.validation.errors());
        if (redwood.extensions.validation.errors().length > 0) {
            args.cancel = true;
        }
    }
});
redwood.events.preinit.subscribe(function (args) {
    args.viewModel.$allValidationErrors = ko.observableArray();
    return false;
});
redwood.events.afterPostback.subscribe(function (args) {
    if (args.serverResponseObject.action === "successfulCommand") {
        // merge validation rules from postback with those we already have (required when a new type appears in the view model)
        redwood.extensions.validation.mergeValidationRules(args);
        args.isHandled = true;
    }
    else if (args.serverResponseObject.action === "validationErrors") {
        // apply validation errors from server
        redwood.extensions.validation.showValidationErrorsFromServer(args);
        args.viewModel.$allValidationErrors(redwood.extensions.validation.errors());
        args.isHandled = true;
    }
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