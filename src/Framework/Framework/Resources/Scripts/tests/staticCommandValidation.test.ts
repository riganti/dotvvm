import { error } from "../events";
import './stateManagement.data'
import { globalValidationObject as validation, ValidationErrorDescriptor } from "../validation/validation"
import { resolveRelativeValidationPaths } from '../postback/staticCommand'
import { getErrors } from "../validation/error"

describe("staticCommand validation - core functions", () => {

    const rootContext: KnockoutBindingContext = ko.contextFor(document.body)
    if (!rootContext) throw new Error("Root context undefined")

    expect(rootContext.$data.Str()).toBe("A")
    expect(rootContext.$data.Str.state).toBe("A")

    test("resolveRelativeValidationPaths - basic property", () => {
        var [ abs ] = resolveRelativeValidationPaths(["Str"], rootContext)!;

        expect(abs).toBe("/Str");
    })

    test("resolveRelativeValidationPaths - nested property", () => {
        var [ abs ] = resolveRelativeValidationPaths(["Inner/P1"], rootContext)!;

        expect(abs).toBe("/Inner/P1");
    })

    test("resolveRelativeValidationPaths - nested viewmodel", () => {
        const nestedContext = rootContext.createChildContext(() => rootContext.$data.Inner)
        expect(nestedContext.$data).toBeTruthy()

        var [ abs ] = resolveRelativeValidationPaths(["P1"], nestedContext)!;

        expect(abs).toBe("/Inner/P1");
    })

    test("resolveRelativeValidationPaths - nested viewmodel with .", () => {
        const nestedContext = rootContext.createChildContext(() => rootContext.$data.Inner)

        var [ abs ] = resolveRelativeValidationPaths(["."], nestedContext)!;

        expect(abs).toBe("/Inner");
    })

    test("resolveRelativeValidationPaths - nested viewmodel with ..", () => {
        const nestedContext = rootContext.createChildContext(() => rootContext.$data.Inner)

        var [ abs ] = resolveRelativeValidationPaths(["../Str"], nestedContext)!;

        expect(abs).toBe("/Str");
    })
    test("resolveRelativeValidationPaths - nested array", () => {
        const nestedContext = rootContext.createChildContext(() => rootContext.$data.Array)
        const nestedContext2 = nestedContext.createChildContext(() => rootContext.$data.Array()[0])

        var [ abs ] = resolveRelativeValidationPaths(["."], nestedContext2)!;

        expect(abs).toBe("/Array/0");
    })
    test("resolveRelativeValidationPaths - nested array with ..", () => {
        const nestedContext = rootContext.createChildContext(() => rootContext.$data.Array)
        const nestedContext2 = nestedContext.createChildContext(() => rootContext.$data.Array()[0])

        var [ abs ] = resolveRelativeValidationPaths([".."], nestedContext2)!;

        expect(abs).toBe("/Array");
    })
    test("resolveRelativeValidationPaths - nested array with ../..", () => {
        const nestedContext = rootContext.createChildContext(() => rootContext.$data.Array)
        const nestedContext2 = nestedContext.createChildContext(() => rootContext.$data.Array()[0])

        var [ abs ] = resolveRelativeValidationPaths(["../.."], nestedContext2)!;

        expect(abs).toBe("/");
    })
    test("resolveRelativeValidationPaths - context is primitive type", () => {
        const nestedContext = rootContext.createChildContext(() => rootContext.$data.Inner)
        const nestedContext2 = nestedContext.createChildContext(() => rootContext.$data.Inner().P1)

        var [ abs ] = resolveRelativeValidationPaths([""], nestedContext2)!;

        expect(abs).toBe("/Inner/P1");
    })
});
