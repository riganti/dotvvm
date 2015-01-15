declare var redwood: Redwood;

if (!redwood) {
    throw "Redwood.js is required!";
}
redwood.extensions.validation = redwood.extensions.validation || {
    rules: {
        "required": new RedwoodRequiredValidator(),
        "regularExpression": new RedwoodRegularExpressionValidator()
        //"numeric": new RedwoodNumericValidator(),
        //"datetime": new RedwoodDateTimeValidator(),
        //"range": new RedwoodRangeValidator()
    }
};
redwood.events.beforePostback.subscribe(args => {
    var errors = [];

    // TODO
    return errors.length > 0;
});

class RedwoodValidationContext {
    public observable: KnockoutObservable<any>;
    public parentViewModel: any;
    public arguments: any[];
}

class RedwoodValidator {
    public ruleName: string;
    public validationGroup: string;

    public isValid = ko.computed(() => redwood.extensions.validation.rules[this.ruleName].isValid(this.context.observable));
    public errorMessage = ko.observable<string>();

    constructor(private context: RedwoodValidationContext) { }
}

class RedwoodValidationRuleBase {
    public isValid(context: RedwoodValidationContext): boolean {
        return false;
    }
    protected isEmpty(value: string): boolean {
        return <any>value || !/^\s*/.test(value);
    }
}

class RedwoodRequiredValidator extends RedwoodValidationRuleBase {
    public isValid(context: RedwoodValidationContext): boolean {
        var value = context.observable();
        return !this.isEmpty(value);
    }
}
class RedwoodRegularExpressionValidator extends RedwoodValidationRuleBase {
    public isValid(context: RedwoodValidationContext): boolean {
        var value = context.observable();
        var expr = context.arguments[0];
        return this.isEmpty(value) || new RegExp(expr).test(value);
    }
}



