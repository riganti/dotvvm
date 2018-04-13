/// <reference path="../../../DotVVM.Framework/Resources/Scripts/typings/knockout/knockout.d.ts" />
var MathematicalOperationsViewModel = /** @class */ (function () {
    function MathematicalOperationsViewModel() {
    }
    MathematicalOperationsViewModel.prototype.Sum = function () {
        var result = this.Left() + this.Right();
        this.Result(Math.floor(result));
    };
    MathematicalOperationsViewModel.prototype.Divide = function () {
        if (this.Right() == 0)
            return;
        this.Result(Math.floor(this.Left() / this.Right()));
    };
    MathematicalOperationsViewModel.prototype.Fibonacci = function () {
        var a = 0;
        var b = 1;
        for (var i = 0; i < this.Right(); i++) {
            var temp = a;
            a = Math.floor(b);
            b = Math.floor(temp + b);
        }
        ;
        this.Result(Math.floor(a));
    };
    return MathematicalOperationsViewModel;
}());
//# sourceMappingURL=DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ClientSideMethods.MathematicalOperationsViewModel.generated.js.map