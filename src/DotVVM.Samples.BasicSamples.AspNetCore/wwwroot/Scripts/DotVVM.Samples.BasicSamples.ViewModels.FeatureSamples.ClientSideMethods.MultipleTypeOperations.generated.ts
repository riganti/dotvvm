/// <reference path="../../../DotVVM.Framework/Resources/Scripts/typings/knockout/knockout.d.ts" />
class MultipleTypeOperationsViewModel {
    public Title: KnockoutObservable<string>;
    public Number: KnockoutObservable<number>;
    public IsVisible: KnockoutObservable<boolean>;
    public Increase() {
        this.Number(this.Number() + 1);
        this.SetTitle();
    }
    public Decrease() {
        this.Number(this.Number() - 1);
        this.SetTitle();
    }
    public SetTitle() {
        this.Title(this.Number() == 0 ? 'Click me!' : ('You have clicked me: ' + this.Number()) + ' times');
    }
    public Reset() {
        this.Number(Math.floor(0));
    }
    public Show() {
        this.IsVisible(true);
    }
    public Hide() {
        this.IsVisible(false);
    }
}

