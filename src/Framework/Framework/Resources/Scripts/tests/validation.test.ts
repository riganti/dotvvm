import { error } from "../events";
import { globalValidationObject as validation, ValidationErrorDescriptor } from "../validation/validation"
import { createComplexObservableSubViewmodel, createComplexObservableViewmodel, ObservableHierarchy, ObservableSubHierarchy } from "./observableHierarchies"
import { getErrors } from "../validation/error"
import { setLogger } from "../utils/logging";
import { runClientSideValidation } from '../validation/validation'
import dotvvm from '../dotvvm-root'
import { getStateManager } from "../dotvvm-base";
import { StateManager } from "../state-manager";

require("./stateManagement.data")


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
            expect(warnMock.mock.calls[0][1]).toContain("Validation error could not been applied to property specified by propertyPath /Prop1/NonExistent. Property with name NonExistent does not exist on /Prop1.");
        }
        finally {
            warnMock.mockRestore();
        }
    })

    test("addErrors - second level nonexistent property (setLogger capture)", () => {
        //Setup
        const vm = createComplexObservableViewmodel();
        const logMessages: any[][] = []
        let captureLog = true
        setLogger((next, level, area, ...args) => {
            if (captureLog && level == "warn")
                logMessages.push([ "area", ...args ])
            else
                next(level, area, ...args)
        })

        try {
            //Act
            validation.addErrors(
                [
                    { errorMessage: "Does not matter", propertyPath: "/Prop1/NonExistent" },
                ],
                { root: ko.observable(vm) }
            );

            //Check
            expect(logMessages.length).toBe(1);
            expect(logMessages[0][1]).toContain("Validation error could not been applied to property specified by propertyPath /Prop1/NonExistent. Property with name NonExistent does not exist on /Prop1.");
        }
        finally {
            captureLog = false
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
            expect(warnMock.mock.calls[0][1]).toContain("Validation error could not been applied to property specified by propertyPath /NonExistent. Property with name NonExistent does not exist on root.");
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


describe("DotVVM.Validation - view model validation", () => {
    const s = getStateManager() as StateManager<any>
    test("Validated object in dynamic", () => {
        dotvvm.updateState(x => ({...x, Dynamic: { something: "abc", validatedObj: { $type: "tValidated", RegexValidated: "abcd" } } }))
        s.doUpdateNow()
        runClientSideValidation(s.stateObservable, {} as any)
        expect(dotvvm.validation.errors).toHaveLength(0)
        s.patchState({ Dynamic: { validatedObj: { RegexValidated: "abcde" }}})
        s.doUpdateNow()
        runClientSideValidation(s.stateObservable, {} as any)
        expect(dotvvm.validation.errors).toHaveLength(1)
    })
})

describe("DotVVM.Validation - stale errors after value changes (issue #438)", () => {
    const s = getStateManager() as StateManager<any>

    afterEach(() => validation.removeErrors("/"))

    test.each(["correct the field", "replace Detail"])("clear errors when we %s without another validation postback", change => {
        validation.removeErrors("/")
        dotvvm.updateState(x => ({ ...x, Dynamic: { Detail: { $type: "tValidated", RegexValidated: "abcde" } } }))
        s.doUpdateNow()
        runClientSideValidation(s.stateObservable, {} as any)

        const originalField = s.stateObservable().Dynamic().Detail().RegexValidated
        expect(getErrors(originalField)).toHaveLength(1)
        expect(validation.errors[0].propertyPath).toBe("/Dynamic/Detail/RegexValidated")
        expect(validation.errors[0].errorMessage).toBe("Must have even length")

        if (change === "correct the field") {
            originalField("abcd")
        } else {
            dotvvm.updateState(x => ({ ...x, Dynamic: { Detail: { $type: "tValidated", RegexValidated: "abcd" } } }))
        }
        s.doUpdateNow()
        ko.tasks.runEarly()

        const currentField = s.stateObservable().Dynamic().Detail().RegexValidated
        expect(currentField()).toBe("abcd")
        // The current value satisfies the rule. Old errors must not keep the field invalid.
        expect(getErrors(currentField)).toHaveLength(0)
        expect(validation.errors).toHaveLength(0)
    })

    test("clear only the changed property's errors and release its subscription", () => {
        validation.removeErrors("/")
        const name = ko.observable("old")
        const other = ko.observable("unchanged")
        validation.addErrors([
            { propertyPath: "/Name", errorMessage: "First error" },
            { propertyPath: "/Name", errorMessage: "Second error" },
            { propertyPath: "/Other", errorMessage: "Other error" }
        ], { root: ko.observable({ Name: name, Other: other }) })
        const changed = jest.fn()
        validation.events.validationErrorsChanged.subscribe(changed)
        try {
            name("old")
            name.valueHasMutated!()
            ko.tasks.runEarly()
            expect(getErrors(name)).toHaveLength(2)
            expect(changed).not.toHaveBeenCalled()

            name("new")
            ko.tasks.runEarly()
            expect(name()).toBe("new")
            expect(getErrors(name)).toHaveLength(0)
            expect(getErrors(other)).toHaveLength(1)
            expect(validation.errors.map(e => e.errorMessage)).toEqual(["Other error"])
            expect(changed).toHaveBeenCalledTimes(1)
            expect(name.getSubscriptionsCount()).toBe(0)

            validation.addErrors([{ propertyPath: "/Name", errorMessage: "New error" }], { root: ko.observable({ Name: name }) })
            validation.removeErrors("/Name")
            expect(name.getSubscriptionsCount()).toBe(0)
        } finally {
            validation.events.validationErrorsChanged.unsubscribe(changed)
        }
    })

    test.each(["remove Detail", "replace Detail type", "replace observable", "remove array item"])("discard child errors when we %s", change => {
        validation.removeErrors("/")
        const invalid = () => ({ $type: "tValidated", RegexValidated: "abcde" })
        dotvvm.updateState(x => ({ ...x, Dynamic: { Detail: invalid(), Other: invalid(), Items: [invalid()] } }))
        s.doUpdateNow()
        runClientSideValidation(s.stateObservable, {} as any)
        const vm = s.stateObservable().Dynamic()
        const removedField = change === "remove array item" ? vm.Items()[0]().RegexValidated : vm.Detail().RegexValidated
        const unchangedField = vm.Other().RegexValidated
        expect(validation.errors).toHaveLength(3)

        if (change === "replace observable") {
            vm.Detail({ $type: "tValidated", RegexValidated: "abcd" })
        } else {
            const patch = change === "remove Detail" ? { Detail: null }
                : change === "replace Detail type" ? { Detail: { $type: "t2", Id: 1 } }
                : { Items: [] }
            s.patchState({ Dynamic: patch })
        }
        s.doUpdateNow()
        ko.tasks.runEarly()
        expect(getErrors(removedField)).toHaveLength(0)
        expect(getErrors(unchangedField)).toHaveLength(1)
        expect(validation.errors).toHaveLength(2)
    })

    test.each(["scalar", "array"])("keep a fresh %s error during deferred state synchronization", kind => {
        validation.removeErrors("/")
        s.patchState({ Dynamic: { Name: "old", Items: ["old"] } })
        s.doUpdateNow()
        const vm = s.stateObservable().Dynamic()
        const field = kind === "scalar" ? vm.Name : vm.Items
        const path = kind === "scalar" ? "/Dynamic/Name" : "/Dynamic/Items"
        validation.addErrors([{ propertyPath: path, errorMessage: "Old error" }])
        if (kind === "scalar") field("new")
        else field.push("new")

        validation.addErrors([{ propertyPath: path, errorMessage: "New value failed server validation" }])
        s.doUpdateNow()
        ko.tasks.runEarly()
        expect(getErrors(field).map(e => e.errorMessage)).toEqual(["New value failed server validation"])
        expect(validation.errors).toHaveLength(1)

        s.doUpdateNow()
        ko.tasks.runEarly()
        expect(getErrors(field)).toHaveLength(1)
    })

    test("clear errors for an old KO value validated while a newer state patch is pending", () => {
        validation.removeErrors("/")
        dotvvm.updateState(x => ({ ...x, Dynamic: { Detail: { $type: "tValidated", RegexValidated: "abcde" } } }))
        s.doUpdateNow()
        const field = s.stateObservable().Dynamic().Detail().RegexValidated
        s.patchState({ Dynamic: { Detail: { RegexValidated: "abcd" } } })
        expect(field()).toBe("abcde")
        runClientSideValidation(s.stateObservable, {} as any)
        expect(getErrors(field)).toHaveLength(1)

        s.doUpdateNow()
        ko.tasks.runEarly()
        expect(field()).toBe("abcd")
        expect(getErrors(field)).toHaveLength(0)
        expect(validation.errors).toHaveLength(0)
    })

    test("refresh validator text, styling, and error count after a value change", () => {
        validation.removeErrors("/")
        const name = ko.observable("old")
        const root = ko.observable({ Name: name })
        const div = document.createElement("div")
        div.innerHTML = `<span class="message" data-bind="'dotvvm-validation': Name, 'dotvvm-validationOptions': { showErrorMessageText: true, invalidCssClass: 'invalid' }"></span>
            <span class="count" data-bind="'dotvvm-validationErrorsCount': { target: Name, includeErrorsFromChildren: false, includeErrorsFromTarget: true, hideWhenValid: false }"></span>`
        ko.applyBindings(root, div)
        try {
            validation.addErrors([{ propertyPath: "/Name", errorMessage: "Old value is invalid" }], { root })
            expect(div.querySelector(".message")!.textContent).toBe("Old value is invalid")
            expect(div.querySelector(".message")!.classList.contains("invalid")).toBe(true)
            expect((div.querySelector(".count") as HTMLElement).innerText).toBe("1")

            name("new")
            ko.tasks.runEarly()
            expect(div.querySelector(".message")!.textContent).toBe("")
            expect(div.querySelector(".message")!.classList.contains("invalid")).toBe(false)
            expect((div.querySelector(".count") as HTMLElement).innerText).toBe("0")
        } finally {
            ko.cleanNode(div)
        }
    })
})

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
