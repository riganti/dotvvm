class RedwoodValidationContext {
    constructor(public valueToValidate: any, public parentViewModel: any, public parameters: any[]) {
    }
}

class RedwoodValidatorBase {
    public isValid(context: RedwoodValidationContext): boolean {
        return false;
    }
    public isEmpty(value: string): boolean {
        return value == null || /^\s*$/.test(value);
    }
}

class RedwoodRequiredValidator extends RedwoodValidatorBase {
    public isValid(context: RedwoodValidationContext): boolean {
        var value = context.valueToValidate;
        return !this.isEmpty(value);
    }
}
class RedwoodRegularExpressionValidator extends RedwoodValidatorBase {
    public isValid(context: RedwoodValidationContext): boolean {
        var value = context.valueToValidate;
        var expr = context.parameters[0];
        return this.isEmpty(value) || new RegExp(expr).test(value);
    }
}
class RedwoodPropertyValidator extends RedwoodValidatorBase {
    constructor(public validation: RedwoodValidation) { super(); }
    isValid(context: RedwoodValidationContext) {
        var val = context.valueToValidate;
        var type = val["$type"];
        if (type == null) return true;
        return this.validation.validateTypedObject(val, type);
    }
}
class RedwoodCollectionValidator extends RedwoodValidatorBase {
    constructor(public validation: RedwoodValidation) { super(); }
    isValid(context: RedwoodValidationContext) {
        var col = <Array<any>>context.valueToValidate;
        var type = context.parameters[0];
        if (type == null) return true;
        return col.every(i => this.validation.validateTypedObject(i, type));
    }
}

class ValidationError {
    public errorMessage = ko.observable("");
    public isValid = ko.computed(() => !!this.errorMessage());

    constructor(public targetObservable: KnockoutObservable<any>) {
    }

    public static getOrCreate(targetObservable: KnockoutObservable<any>): ValidationError {
        if (!targetObservable["validationError"]) {
            targetObservable["validationError"] = new ValidationError(targetObservable);
        }
        return <ValidationError>targetObservable["validationError"];
    }
}

interface IRedwoodValidationRules {
    [name: string]: RedwoodValidatorBase;
}
interface IRedwoodValidationElementUpdateFunctions {
    [name: string]: (element: any, errorMessage: string, options: any) => void;
}

class RedwoodValidation {
    public rules: IRedwoodValidationRules = {
        "required": new RedwoodRequiredValidator(),
        "regularExpression": new RedwoodRegularExpressionValidator(),
        //"numeric": new RedwoodNumericValidator(),
        //"datetime": new RedwoodDateTimeValidator(),
        //"range": new RedwoodRangeValidator()

        "validate": new RedwoodPropertyValidator(this),
        "collection": new RedwoodCollectionValidator(this),
    }

    public errors = ko.observableArray<ValidationError>([]);

    public elementUpdateFunctions: IRedwoodValidationElementUpdateFunctions = {
        
        // shows the element when it is not valid
        hideWhenValid(element: any, errorMessage: string, options: any) {
            if (errorMessage) {
                element.style.display = "";
                element.title = "";
            } else {
                element.style.display = "none";
                element.title = errorMessage;
            }
        },

        // adds a CSS class when the element is not valid
        addCssClass(element: any, errorMessage: string, options: any) {
            var cssClass = (options && options.cssClass) ? options.cssClass : "field-validation-error";
            if (errorMessage) {
                element.className += " " + cssClass;
            } else {
                element.className = element.className.replace(new RegExp("\\b" + cssClass + "\\b", "g"), "");
            }
        },

        // displays the error message
        displayErrorMessage(element: any, errorMessage: string, options: any) {
            element[element.innerText ? "innerText" : "textContent"] = errorMessage;
        }
    }

    /// Validates the specified view model
    public validateViewModel(viewModel: any, viewModelName: string = "root") {
        var rules = <Array<any>>redwood.viewModels.root.validationRules[viewModelName];
        rules.forEach(rule => {

            var viewModelProperty = viewModel[rule.propertyName];
            if (!viewModelProperty || !ko.isObservable(viewModelProperty)) return;

            var value = viewModelProperty();

            this.validateProperty(viewModel, viewModelProperty, value, rule);
        });
    }

    public validateTypedObject(obj: any, type: string): boolean {
        if (!obj) return;
        if (!type) return;
        var ecount = this.errors.length;
        var rulesForType = redwood.viewModels.root.validationRules.types[type];
        if (!rulesForType) return;

        // validate all properties
        for (var property in obj) {
            if (!obj.hasOwnProperty(property) || property.indexOf("$") >= 0) continue;

            var viewModelProperty = obj[property];
            if (!viewModelProperty || !ko.isObservable(viewModelProperty)) continue;

            var value = obj[property]();

            // run validation rules
            if (rulesForType.hasOwnProperty(property)) {
                rulesForType[property].forEach(r => this.validateProperty(obj, viewModelProperty, value, r));
            }
        }
        return ecount == this.errors.length;
    }

    /// Validates the specified property in the viewModel
    public validateProperty(viewModel: any, property: KnockoutObservable<any>, value: any, rule: any) {
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
        } else {
            // remove
            validationError.errorMessage("");
        }
    }

    // clears validation errors
    public clearValidationErrors(validationTarget: any) {
        if (!validationTarget.$validationErrors || !ko.isObservable(validationTarget.$validationErrors)) return;
        var errors = validationTarget.$validationErrors();
        for (var i = 0; i < errors.length; i++) {
            errors[i].errorMessage("");
        }
        this.errors.removeAll(errors);
    }

    // merge validation rules
    public mergeValidationRules(args: RedwoodAfterPostBackEventArgs) {
        this.clearValidationErrors(args.viewModel);
        if (args.serverResponseObject.validationRules) {
            var existingRules = redwood.viewModels[args.viewModelName].validationRules;

            for (var type in args.serverResponseObject) {
                if (!args.serverResponseObject.hasOwnProperty(type)) continue;
                existingRules[type] = args.serverResponseObject[type];
            }
        }
    }

    // shows the validation errors from server
    public showValidationErrorsFromServer(args: RedwoodAfterPostBackEventArgs) {
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
    }
};

// init the plugin
declare var redwood: Redwood;
interface RedwoodExtensions {
    validation?: RedwoodValidation;
}
if (!redwood) {
    throw "Redwood.js is required!";
}
redwood.extensions.validation = redwood.extensions.validation || new RedwoodValidation();

// perform the validation before postback
redwood.events.beforePostback.subscribe(args => {
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

redwood.events.afterPostback.subscribe(args => {
    if (args.serverResponseObject.action === "successfulCommand") {
        // merge validation rules from postback with those we already have (required when a new type appears in the view model)
        redwood.extensions.validation.mergeValidationRules(args);
        return false;
    } else if (args.serverResponseObject.action === "validationErrors") {
        // apply validation errors from server
        redwood.extensions.validation.showValidationErrorsFromServer(args);
        return true;
    }
    return false;
});

// add knockout binding handler
ko.bindingHandlers["redwoodValidation"] = {
    init(element: any, valueAccessor: () => any, allBindingsAccessor: KnockoutAllBindingsAccessor, viewModel: any, bindingContext: KnockoutBindingContext) {
        var observableProperty = valueAccessor();
        if (ko.isObservable(observableProperty)) {
            // try to get the options
            var options = allBindingsAccessor.get("redwoodValidationOptions");
            var mode = (options && options.mode) ? options.mode : "addCssClass";
            var updateFunction = redwood.extensions.validation.elementUpdateFunctions[mode];

            // subscribe to the observable property changes
            var validationError = ValidationError.getOrCreate(observableProperty);
            validationError.errorMessage.subscribe(newValue => updateFunction(element, newValue, options));
            updateFunction(element, validationError.errorMessage(), options);
        }
    }
};


