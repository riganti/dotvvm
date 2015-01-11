declare var redwood: Redwood;

if (!redwood) {
    throw "Redwood.js is required!";
}
redwood.extensions.validation = redwood.extensions.validation || {
    rules: [

    ]
};

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



