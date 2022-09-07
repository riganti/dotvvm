export interface ObservableHierarchy {
    $type: string | KnockoutObservable<string>
    Prop1: KnockoutObservable<string>
    Prop2: null | KnockoutObservable<ObservableSubHierarchy>
}

export interface ObservableSubHierarchy {
    $type: string | KnockoutObservable<string>
    Prop21: KnockoutObservable<string>
    Prop22: KnockoutObservable<string>
    Prop23: KnockoutObservableArray<null | KnockoutObservable<ObservableSubSubHierarchy>>
}

export interface ObservableSubSubHierarchy {
    Prop231: null | KnockoutObservable<null | string>,
    $type: string | KnockoutObservable<string>
}

export function createComplexObservableViewmodel(): ObservableHierarchy {
    return {
        $type: ko.observable("t5"),
        Prop1: ko.observable("aa"),
        Prop2: ko.observable(createComplexObservableSubViewmodel())
    }
}

export function createComplexObservableSubViewmodel(): ObservableSubHierarchy {
    return {
        $type: ko.observable("t5_a"),
        Prop21: ko.observable("bb"),
        Prop22: ko.observable("cc"),
        Prop23: ko.observableArray<KnockoutObservable<ObservableSubSubHierarchy> | null>([
            ko.observable<ObservableSubSubHierarchy>({
                $type: ko.observable("t5_a_a"),
                Prop231: ko.observable("dd")
            }),
            ko.observable<ObservableSubSubHierarchy>({
                $type: ko.observable("t5_a_a"),
                Prop231: ko.observable("ee")
            })
        ])
    }
}
