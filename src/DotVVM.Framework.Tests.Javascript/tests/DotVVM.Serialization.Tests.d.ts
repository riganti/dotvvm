/// <reference path="../scripts/typings/jasmine/jasmine.d.ts" />
/// <reference path="../../DotVVM.Framework/Resources/Scripts/typings/knockout/knockout.d.ts" />
/// <reference path="../../DotVVM.Framework/Resources/Scripts/DotVVM.d.ts" />
declare var dotvvm: DotVVM;
declare var assertObservable: (object: any) => any;
declare var asserObservableString: (object: any, expected: string) => void;
declare function assertSubHierarchiesNotLinked(viewmodel: ObservableSubHierarchy, target: ObservableSubHierarchy): void;
declare function assertHierarchy(result: ObservableHierarchy): void;
declare function assertSubHierarchy(prop2Object: ObservableSubHierarchy): void;
declare function createComplexObservableTarget(): KnockoutObservable<ObservableHierarchy>;
declare function createComplexObservableTargetWithNullArrayElement(): KnockoutObservable<ObservableHierarchy>;
declare function createComplexObservableTargetWithArrayElementPropertyMissing(): KnockoutObservable<any>;
declare function createComplexObservableTargetWithArrayElementPropertyNull(): KnockoutObservable<ObservableHierarchy>;
declare function createComplexObservableTargetWithArrayElementMissingAndNull(): KnockoutObservable<ObservableHierarchy>;
declare function createComplexObservableTargetWithArrayElementPropertyObservableNull(): KnockoutObservable<ObservableHierarchy>;
declare function createComplexObservableTargetWithMissingArrayElement(): KnockoutObservable<ObservableHierarchy>;
declare function createComplexObservableTargetWithNullSubHierarchy(): KnockoutObservable<ObservableHierarchy>;
declare function createComplexObservableTargetWithMissingSubHierarchy(): KnockoutObservable<any>;
declare function createComplexObservableViewmodel(): ObservableHierarchy;
declare function createComplexObservableSubViewmodel(): ObservableSubHierarchy;
interface ObservableHierarchy {
    Prop1: KnockoutObservable<string>;
    Prop2: KnockoutObservable<ObservableSubHierarchy>;
}
interface ObservableSubHierarchy {
    Prop21: KnockoutObservable<string>;
    Prop22: KnockoutObservable<string>;
    Prop23: KnockoutObservableArray<KnockoutObservable<{
        Prop231: KnockoutObservable<string>;
    }>>;
}
