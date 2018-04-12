/// <reference path="../../../DotVVM.Framework/Resources/Scripts/typings/knockout/knockout.d.ts" />
var MultipleTypeOperationsViewModel = /** @class */ (function () {
    function MultipleTypeOperationsViewModel() {
    }
    MultipleTypeOperationsViewModel.prototype.Increase = function () {
        this.Number(this.Number() + 1);
        this.SetTitle();
    };
    MultipleTypeOperationsViewModel.prototype.Decrease = function () {
        this.Number(this.Number() - 1);
        this.SetTitle();
    };
    MultipleTypeOperationsViewModel.prototype.SetTitle = function () {
        this.Title(this.Number() == 0 ? 'Click me!' : ('You have clicked me: ' + this.Number()) + ' times');
    };
    MultipleTypeOperationsViewModel.prototype.Reset = function () {
        this.Number(Math.floor(0));
    };
    MultipleTypeOperationsViewModel.prototype.Show = function () {
        this.IsVisible(true);
    };
    MultipleTypeOperationsViewModel.prototype.Hide = function () {
        this.IsVisible(false);
    };
    return MultipleTypeOperationsViewModel;
}());
//# sourceMappingURL=DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ClientSideMethods.MultipleTypeOperations.generated.js.map