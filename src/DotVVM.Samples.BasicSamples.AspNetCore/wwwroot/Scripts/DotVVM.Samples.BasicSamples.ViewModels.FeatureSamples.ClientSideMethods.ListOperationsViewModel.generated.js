/// <reference path="../../../DotVVM.Framework/Resources/Scripts/typings/knockout/knockout.d.ts" />
var ListOperationsViewModel = /** @class */ (function () {
    function ListOperationsViewModel() {
    }
    ListOperationsViewModel.prototype.RemoveTest = function () {
        this.Remove('test');
    };
    ListOperationsViewModel.prototype.Remove = function (name) {
        this.NamesList.remove(function (item) { var rawItem = ko.unwrap(item); return rawItem == name; });
    };
    return ListOperationsViewModel;
}());
//# sourceMappingURL=DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ClientSideMethods.ListOperationsViewModel.generated.js.map