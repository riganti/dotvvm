ko.extenders.passwordStrength = function (target, overrideMessage) 
{
    //add some sub-observables to our observable
    target.passwordStrengthMessage = ko.observable();

    //define a function to do password strength test
    function testPassword(newValue) {

        // very simple password test - checks the string length only

        var val = newValue || "";
        if (val.length === 0) 
        {
            target.passwordStrengthMessage("Enter password");
        }
        else
        {
            target.passwordStrengthMessage("Poor");
        }

        if (val.length > 5) {
            target.passwordStrengthMessage("Good");
        }

        if (val.length > 8) {
            target.passwordStrengthMessage("Super");
        }

        if (val.length > 12) {
            target.passwordStrengthMessage("Excellent");
        }
    }

    //initial validation
    testPassword(target());

    //validate whenever the value changes
    target.subscribe(testPassword);

    //return the original observable
    return target;
};
