import dotvvm from "../dotvvm-root";
import { coerce, tryCoerce } from "../metadata/coercer";
import { asType, isType, replaceTypeInfo, updateTypeInfo } from "../metadata/typeMap";
import { diffViewModel, patchViewModel } from "../postback/updater";
import { deserialize } from "../serialization/deserialize";
import { serialize } from "../serialization/serialize";
import { StateManager } from "../state-manager";
import { allErrors, detachAllErrors, ValidationError } from "../validation/error";
import { initDotvvm } from "./helper";

const metadata: TypeMap = {
    root: { type: "object", properties: { item: { type: "base" }, items: { type: ["base"] } } },
    base: { type: "object", derivedTypes: ["left", "right"], properties: { shared: { type: "String" } } },
    left: {
        type: "object", baseTypes: ["base", "interface"], properties: {
            shared: { type: "String" }, left: { type: "Int32" }
        }
    },
    right: {
        type: "object", baseTypes: ["base", "interface"], properties: {
            shared: { type: "String" }, right: { type: "Int32" }
        }
    },
    unrelated: { type: "object", properties: {} },
    legacy: { type: "object", properties: {} },
    narrow: { type: "object", derivedTypes: [], properties: {} }
};
const left = { $type: "left", shared: "same", left: 1 };
const right = { $type: "right", shared: "same", right: 2 };

initDotvvm({ viewModel: {}, typeMetadata: metadata });
beforeEach(() => replaceTypeInfo(metadata));
afterEach(() => detachAllErrors());

test("annotated contracts accept base and registered descendants", () => {
    expect(coerce({ shared: "base" }, "base")).toEqual({ $type: "base", shared: "base" });
    expect(coerce({ ...left, left: "12" }, "base")).toEqual({ ...left, left: 12 });
    expect(coerce(right, "base", left)).toBe(right);
    expect(coerce({ ...left, ...right }, "base", left)).toEqual(right);
    expect(coerce([left, right], ["base"])).toEqual([left, right]);
    expect(coerce(right, { type: "nullable", inner: "base" })).toBe(right);
});

test.each(["unrelated", "unknown", "String"])("annotated contracts reject runtime type %s", type => {
    const value = { $type: type };
    expect(tryCoerce(value, "base").isError).toBe(true);
    expect(tryCoerce(value, "base", value).isError).toBe(true);
    expect(tryCoerce([value], ["base"]).isError).toBe(true);
    expect(tryCoerce(value, { type: "nullable", inner: "base" }).isError).toBe(true);
});

test.each([0, 1, false, "", [], "value"])("annotated contracts reject non-object value %p", value => {
    expect(tryCoerce(value, "base").isError).toBe(true);
});

test("unannotated and dynamic contracts retain permissive runtime typing", () => {
    expect(coerce(left, "legacy")).toBe(left);
    expect(coerce({ $type: "unrelated" }, "legacy")).toEqual({ $type: "unrelated" });
    expect(coerce(right, { type: "dynamic" })).toBe(right);
    expect(coerce({ arbitrary: undefined }, { type: "dynamic" })).toEqual({ arbitrary: null });
});

test("incremental validation cannot reuse a different expected contract", () => {
    const value = coerce(left, "base");
    expect(coerce(value, "base", value)).toBe(value);
    expect(tryCoerce(value, "narrow", value).isError).toBe(true);
    const array = coerce([value], ["base"]);
    expect(tryCoerce(array, ["narrow"], array).isError).toBe(true);
    updateTypeInfo({ base: { ...metadata.base as ObjectTypeMetadata, derivedTypes: [] } });
    expect(tryCoerce(value, "base", value).isError).toBe(true);
});

test("runtime type changes revalidate shared values against the new property contract", () => {
    updateTypeInfo({
        left: {
            ...metadata.left as ObjectTypeMetadata,
            properties: { child: { type: "base" } }
        },
        right: {
            ...metadata.right as ObjectTypeMetadata,
            properties: { child: { type: "narrow" } }
        }
    });
    const child = { $type: "base", shared: "child" };
    const previous = coerce({ $type: "left", child }, "base");
    const result = tryCoerce({ $type: "right", child: previous.child }, "base", previous);
    expect(result.isError).toBe(true);
});

test("subtype changes replace shapes in patches and diffs, including arrays", () => {
    expect(patchViewModel(left, right)).toEqual(right);
    expect(diffViewModel(left, right)).toEqual(right);
    const source = { item: left, items: [left] };
    const modified = { item: right, items: [right] };
    expect(patchViewModel(source, diffViewModel(source, modified))).toEqual(modified);
    expect(patchViewModel(left, { left: 3 })).toEqual({ ...left, left: 3 });
});

test("legacy deserialization replaces subtype properties and retains same-type identity", () => {
    const observable = ko.observable(deserialize(left));
    const oldObject = observable();
    expect(deserialize({ ...left, left: 3 }, observable)).toBe(observable);
    expect(observable()).toBe(oldObject);
    deserialize(right, observable);
    expect(observable()).not.toBe(oldObject);
    expect(observable().left).toBeUndefined();
    expect(serialize(observable)).toEqual(right);
    const array = ko.observableArray([ko.observable(deserialize(left))]);
    deserialize([right], array);
    expect(serialize(array)).toEqual([right]);
});

test("state and observable subtype changes remove old shape and obsolete errors", () => {
    const state = new StateManager<any>({ $type: "root", item: left, items: [left] });
    state.doUpdateNow();
    const vm = state.stateObservable() as any;
    const oldItem = vm.item();
    ValidationError.attach("obsolete", "/item/left", oldItem.left);
    state.patchState({ item: right, items: [right] });
    state.doUpdateNow();
    expect(vm.item()).not.toBe(oldItem);
    expect(vm.item().left).toBeUndefined();
    expect(vm.items()[0]().left).toBeUndefined();
    expect(allErrors).toHaveLength(0);
    expect(serialize(vm.item)).toEqual(right);
    oldItem.left(10);
    expect(state.state.item).toEqual(right);
    const sameType = vm.item();
    state.patchState({ item: { right: 3 } });
    state.doUpdateNow();
    expect(vm.item()).toBe(sameType);
    vm.item(left);
    expect(vm.item().right).toBeUndefined();
    expect(state.state.item).toEqual(left);
});

test("root state retains its annotated expected contract", () => {
    const state = new StateManager<any>({ $type: "base", shared: "base" });
    state.setState(left);
    state.setState(right);
    expect(() => state.setState({ $type: "unrelated" })).toThrow();
    state.doUpdateNow();
    expect(serialize(state.stateObservable)).toEqual(right);
});

test("runtime type changes replace client extenders on shared property names", () => {
    const extenders = ko.extenders as any;
    const previousExtender = extenders.polymorphismTest;
    extenders.polymorphismTest = (observable: any, parameter: any) => {
        observable.subtype = parameter;
        return observable;
    };
    try {
        for (const type of ["left", "right"]) {
            updateTypeInfo({
                [type]: {
                    ...metadata[type] as ObjectTypeMetadata,
                    properties: {
                        ...(metadata[type] as ObjectTypeMetadata).properties,
                        shared: { type: "String", clientExtenders: [{ name: "polymorphismTest", parameter: type }] }
                    }
                }
            });
        }
        const state = new StateManager<any>({ $type: "root", item: left, items: [] });
        const item = (state.stateObservable() as any).item;
        const oldShared = item().shared;
        expect(oldShared.subtype).toBe("left");
        state.patchState({ item: right });
        state.doUpdateNow();
        expect(item().shared).not.toBe(oldShared);
        expect(item().shared.subtype).toBe("right");
    } finally {
        if (previousExtender === undefined) delete extenders.polymorphismTest;
        else extenders.polymorphismTest = previousExtender;
    }
});

test("client switches allow signed and encrypted descendant properties", () => {
    updateTypeInfo({
        right: {
            ...metadata.right as ObjectTypeMetadata,
            properties: {
                ...(metadata.right as ObjectTypeMetadata).properties,
                secret: { type: "String", post: "no", update: "no" }
            }
        }
    });
    const state = new StateManager<any>({ $type: "root", item: left, items: [] });
    state.patchState({ item: { ...right, secret: "client" } });
    state.doUpdateNow();
    expect(state.state.item.secret).toBe("client");
    expect(serialize((state.stateObservable() as any).item)).toEqual(right);
});

test("runtime helpers use CLR base types, not allowed serialization descendants", () => {
    expect(isType(left, "left")).toBe(true);
    expect(isType(left, "base")).toBe(true);
    expect(isType(left, "interface")).toBe(true);
    expect(isType(left, "right")).toBe(false);
    updateTypeInfo({ narrow: { ...metadata.narrow as ObjectTypeMetadata, derivedTypes: ["left"] } });
    expect(isType(left, "narrow")).toBe(false);
    expect(asType(left, "base")).toBe(left);
    expect(asType(left, "right")).toBeNull();
    expect(dotvvm.metadata.isType(left, "base")).toBe(true);
    expect(dotvvm.metadata.asType(left, "base")).toBe(left);
});

test("runtime helpers unwrap observable objects and type identifiers", () => {
    const observableObject = { $type: ko.observable("left"), left: ko.observable(1) };
    expect(isType(ko.observable(observableObject), "base")).toBe(true);
    expect(asType(ko.observable(observableObject), "base")).toBe(observableObject);
    expect(isType(Object.freeze(left), "base")).toBe(true);
    for (const value of [null, undefined, {}, 1, ko.observable(null), { $type: ko.observable(null) }]) {
        expect(isType(value, "base")).toBe(false);
        expect(asType(value, "base")).toBeNull();
    }
});
