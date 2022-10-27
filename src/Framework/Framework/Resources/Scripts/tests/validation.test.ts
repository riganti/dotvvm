import { error } from "../events";
import { globalValidationObject as validation, ValidationErrorDescriptor } from "../validation/validation"
import { createComplexObservableSubViewmodel, createComplexObservableViewmodel, ObservableHierarchy, ObservableSubHierarchy } from "./observableHierarchies"
import { getErrors } from "../validation/error"


describe("DotVVM.Validation - public API", () => {

    test("addErrors - single first level property", () => {
        //Setup
        validation.removeErrors("/");
        const vm = createComplexObservableViewmodel();

        //Act
        validation.addErrors(
            [
                { errorMessage: "Prop1 is too short.", propertyPath: "/Prop1" },
                { errorMessage: "Prop1 is too long.", propertyPath: "/Prop1/" }
            ],
            { root: ko.observable(vm) }
        )

        //Check
        expect(validation.errors.length).toBe(2);
        expect(validation.errors[0].errorMessage).toBe("Prop1 is too short.");
        expect(validation.errors[1].errorMessage).toBe("Prop1 is too long.");

        const errorsFromObservable = getErrors(vm.Prop1);

        expect(errorsFromObservable.length).toBe(2);
        expect(errorsFromObservable[0].errorMessage).toBe("Prop1 is too short.");
        expect(errorsFromObservable[1].errorMessage).toBe("Prop1 is too long.");
    })

    test("addErrors - two different properties", () => {
        //Setup
        validation.removeErrors("/");
        const vm = createComplexObservableViewmodel();

        //Act
        validation.addErrors(
            [
                { errorMessage: "Prop1 is too short.", propertyPath: "/Prop1" },
                { errorMessage: "Prop21 is too long.", propertyPath: "/Prop2/Prop21" }
            ],
            { root: ko.observable(vm) }
        )

        //Check
        expect(validation.errors.length).toBe(2);
        expect(validation.errors[0].errorMessage).toBe("Prop1 is too short.");
        expect(validation.errors[1].errorMessage).toBe("Prop21 is too long.");

        const errorsFromProp1 = getErrors(vm.Prop1);
        const errorsFromProp21 = getErrors((vm.Prop2 as KnockoutObservable<ObservableSubHierarchy>)().Prop21);

        expect(errorsFromProp1.length).toBe(1);
        expect(errorsFromProp21.length).toBe(1);

        expect(errorsFromProp1[0].errorMessage).toBe("Prop1 is too short.");
        expect(errorsFromProp21[0].errorMessage).toBe("Prop21 is too long.");
    })

    test("addErrors - on root", () => {
        //Setup
        validation.removeErrors("/");
        const vm = ko.observable(createComplexObservableViewmodel());

        //Act
        validation.addErrors(
            [
                { errorMessage: "Everything is invalid.", propertyPath: "/" },
                { errorMessage: "Everything is wrong.", propertyPath: "/" }
            ],
            { root: vm }
        )

        //Check
        expect(validation.errors.length).toBe(2);
        expect(validation.errors[0].errorMessage).toBe("Everything is invalid.");
        expect(validation.errors[1].errorMessage).toBe("Everything is wrong.");

        const errorsFromRoot = getErrors(vm);

        expect(errorsFromRoot.length).toBe(2);

        expect(errorsFromRoot[0].errorMessage).toBe("Everything is invalid.");
        expect(errorsFromRoot[1].errorMessage).toBe("Everything is wrong.");
    })

    test("addErrors - on array element", () => {
        //Setup
        validation.removeErrors("/");
        const vm = ko.observable(createComplexObservableViewmodel());

        //Act
        validation.addErrors(
            [
                { errorMessage: "Element 0 is invalid.", propertyPath: "/Prop2/Prop23/0/" },
                { errorMessage: "Element 1 is wrong.", propertyPath: "/Prop2/Prop23/1" }
            ],
            { root: vm }
        )

        //Check
        expect(validation.errors.length).toBe(2);
        expect(validation.errors[0].errorMessage).toBe("Element 0 is invalid.");
        expect(validation.errors[1].errorMessage).toBe("Element 1 is wrong.");

        const errorsFromElement0 = getErrors((vm().Prop2 as KnockoutObservable<ObservableSubHierarchy>)().Prop23()[0]);
        const errorsFromElement1 = getErrors((vm().Prop2 as KnockoutObservable<ObservableSubHierarchy>)().Prop23()[1]);

        expect(errorsFromElement0.length).toBe(1);
        expect(errorsFromElement1.length).toBe(1);

        expect(errorsFromElement0[0].errorMessage).toBe("Element 0 is invalid.");
        expect(errorsFromElement1[0].errorMessage).toBe("Element 1 is wrong.");
    })

    test("addErrors - second level nonexistent property", () => {
        //Setup
        const vm = createComplexObservableViewmodel();
        var warnMock = jest.spyOn(console, 'warn').mockImplementation(() => { });
        try {
            //Act
            validation.addErrors(
                [
                    { errorMessage: "Does not matter", propertyPath: "/Prop1/NonExistent" },
                ],
                { root: ko.observable(vm) }
            );

            //Check
            expect(warnMock).toHaveBeenCalled();
            expect(warnMock.mock.calls[0][2]).toContain("Unable to find viewmodel property /Prop1/NonExistent");
        }
        finally {
            warnMock.mockRestore();
        }
    })

    test("addErrors - root level nonexistent property", () => {
        //Setup
        const vm = createComplexObservableViewmodel();
        var warnMock = jest.spyOn(console, 'warn').mockImplementation(() => { });
        try {
            //Act
            validation.addErrors(
                [
                    { errorMessage: "Does not matter", propertyPath: "/NonExistent" },
                ],
                { root: ko.observable(vm) }
            );

            //Check
            expect(warnMock).toHaveBeenCalled();
            expect(warnMock.mock.calls[0][2]).toContain("Unable to find viewmodel property /NonExistent");
        }
        finally {
            warnMock.mockRestore();
        }
    })

    test("removeErrors - remove everything from root up", () => {
        //Setup
        const vm = SetupComplexObservableViewmodelWithErrorsOnProp1AndProp21();

        //Act
        validation.removeErrors("/");

        //Check
        expect(validation.errors.length).toBe(0);

        const errorsFromProp1 = getErrors(vm.Prop1);
        const errorsFromProp21 = getErrors((vm.Prop2 as KnockoutObservable<ObservableSubHierarchy>)().Prop21);

        expect(errorsFromProp1.length).toBe(0);
        expect(errorsFromProp21.length).toBe(0);
    })

    test("removeErrors - remove only on specific property", () => {
        //Setup
        const vm = SetupComplexObservableViewmodelWithErrorsOnProp1AndProp21();

        //Act
        validation.removeErrors("/Prop2/Prop21");

        //Check
        expect(validation.errors.length).toBe(1);

        const errorsFromProp1 = getErrors(vm.Prop1);
        const errorsFromProp21 = getErrors((vm.Prop2 as KnockoutObservable<ObservableSubHierarchy>)().Prop21);

        expect(errorsFromProp1.length).toBe(1);
        expect(errorsFromProp21.length).toBe(0);
    })

    test("removeErrors - remove only on property in subhierarchy", () => {
        //Setup
        const vm = SetupComplexObservableViewmodelWithErrorsOnProp1AndProp21();

        //Act
        validation.removeErrors("/Prop2");

        //Check
        expect(validation.errors.length).toBe(1);

        const errorsFromProp1 = getErrors(vm.Prop1);
        const errorsFromProp21 = getErrors((vm.Prop2 as KnockoutObservable<ObservableSubHierarchy>)().Prop21);

        expect(errorsFromProp1.length).toBe(1);
        expect(errorsFromProp21.length).toBe(0);
    })

    test("removeErrors - remove from 2 different properties in subhierarchy", () => {
        //Setup
        validation.removeErrors("/");
        const vm = createComplexObservableViewmodel();

        validation.addErrors(
            [
                { errorMessage: "Prop1 is too short.", propertyPath: "/Prop1" },
                { errorMessage: "Prop21 is too long.", propertyPath: "/Prop2/Prop21" },
                { errorMessage: "Prop21 is too large.", propertyPath: "/Prop2/Prop22" }

            ],
            { root: ko.observable(vm) }
        );

        //Act
        validation.removeErrors("/Prop2");

        //Check
        expect(validation.errors.length).toBe(1);

        const errorsFromProp1 = getErrors(vm.Prop1);
        const errorsFromProp21 = getErrors((vm.Prop2 as KnockoutObservable<ObservableSubHierarchy>)().Prop21);
        const errorsFromProp22 = getErrors((vm.Prop2 as KnockoutObservable<ObservableSubHierarchy>)().Prop22);

        expect(errorsFromProp1.length).toBe(1);
        expect(errorsFromProp21.length).toBe(0);
        expect(errorsFromProp22.length).toBe(0);
    })

    test("validationErrorsChanged - gets called on error added.", () => {
        //Setup
        validation.removeErrors("/");
        const vm = createComplexObservableViewmodel();

        const onErrorsCallback = jest.fn();

        validation.events.validationErrorsChanged.subscribe(onErrorsCallback);

        //Act
        validation.addErrors(
            [{ errorMessage: "Prop1 is too short.", propertyPath: "/Prop1" }],
            { root: ko.observable(vm) }
        )

        validation.addErrors(
            [{ errorMessage: "Prop1 is too long.", propertyPath: "/Prop2" }],
            { root: ko.observable(vm) }
        )

        //Check
        expect(onErrorsCallback).toHaveBeenCalledTimes(2);
    })

    test("validationErrorsChanged - gets called on existing error removed.", () => {
        //Setup
        validation.removeErrors("/");
        const vm = createComplexObservableViewmodel();

        const onErrorsCallback = jest.fn();

        validation.events.validationErrorsChanged.subscribe(onErrorsCallback);

        //Act
        validation.addErrors(
            [{ errorMessage: "Prop1 is too short.", propertyPath: "/Prop1" }],
            { root: ko.observable(vm) }
        )

        validation.removeErrors("/Prop1")

        //Check
        expect(onErrorsCallback).toHaveBeenCalledTimes(2);
    })
});

function SetupComplexObservableViewmodelWithErrorsOnProp1AndProp21() {
    validation.removeErrors("/");
    const vm = createComplexObservableViewmodel();

    validation.addErrors(
        [
            { errorMessage: "Prop1 is too short.", propertyPath: "/Prop1" },
            { errorMessage: "Prop21 is too long.", propertyPath: "/Prop2/Prop21" }
        ],
        { root: ko.observable(vm) }
    );
    return vm;
}
