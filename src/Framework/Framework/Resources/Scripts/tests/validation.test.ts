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

    test("addErrors - second level nonexistent property", () => {
        //Setup
        const vm = createComplexObservableViewmodel();

        //Act
        const act = () => validation.addErrors(
            [
                { errorMessage: "Does not matter", propertyPath: "/Prop1/NonExistent" },
            ],
            { root: ko.observable(vm) }
        )

        //Check
        expect(act).toThrowError("Validation error could not been applied to property specified by propertyPath /Prop1/NonExistent. Property with name NonExistent does not exist on /Prop1.");
    })

    test("addErrors - root level nonexistent property", () => {
        //Setup
        const vm = createComplexObservableViewmodel();

        //Act
        const act = () => validation.addErrors(
            [
                { errorMessage: "Does not matter", propertyPath: "/NonExistent" },
            ],
            { root: ko.observable(vm) }
        )

        //Check
        expect(act).toThrowError("Validation error could not been applied to property specified by propertyPath /NonExistent. Property with name NonExistent does not exist on root.");
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
