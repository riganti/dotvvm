/// <reference path="../../../DotVVM.Framework/Resources/Scripts/typings/knockout/knockout.d.ts" />
class MathematicalOperationsViewModel {
    public Left: KnockoutObservable<number>;
    public Right: KnockoutObservable<number>;
    public Result: KnockoutObservable<number>;
    public Sum() {
        let result = this.Left() + this.Right();
        this.Result(result);
    }
    public Divide() {
        if (this.Right() == 0)
            return;
        this.Result(this.Left() / this.Right());
    }
    public Fibonacci() {
        let a = 0;
        let b = 1;
        for (let i = 0; i < this.Right(); i++) {
            let temp = a;
            a = b;
            b = temp + b;
        }
        this.Result(a);
    }
}
