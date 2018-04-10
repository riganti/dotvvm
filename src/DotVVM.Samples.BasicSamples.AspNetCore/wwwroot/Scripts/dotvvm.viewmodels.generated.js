var MathematicalOperationsViewModel = /** @class */ (function () {
    function MathematicalOperationsViewModel() {
    }
    MathematicalOperationsViewModel.prototype.Sum = function () {
        var result = this.Left() + this.Right();
        this.Result(result);
    };
    MathematicalOperationsViewModel.prototype.Divide = function () {
        if (this.Right() == 0)
            return;
        this.Result(this.Left() / this.Right());
    };
    MathematicalOperationsViewModel.prototype.Fibonacci = function () {
        var a = 0;
        var b = 1;
        for (var i = 0; i < this.Right(); i++) {
            var temp = a;
            a = b;
            b = temp + b;
        }
        this.Result(a);
    };
    return MathematicalOperationsViewModel;
}());
