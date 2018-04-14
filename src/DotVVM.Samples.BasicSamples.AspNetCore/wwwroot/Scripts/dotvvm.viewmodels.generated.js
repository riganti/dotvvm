var ListOperationsViewModel = /** @class */ (function () {
    function ListOperationsViewModel() {
    }
    ListOperationsViewModel.prototype.Add = function () {
        this.NamesList.push(ko.observable('test' + this.Index()));
        this.Index(this.Index() + 1);
    };
    ListOperationsViewModel.prototype.RemoveTest = function () {
        this.Remove('test');
    };
    ListOperationsViewModel.prototype.Remove = function (name) {
        this.NamesList.remove(function (item) { var rawItem = ko.unwrap(item); return rawItem == name; });
    };
    ListOperationsViewModel.prototype.Clear = function () {
        this.NamesList.removeAll();
    };
    ListOperationsViewModel.prototype.Iterate = function () {
        for (var i = 0; i < this.NamesList().length; i++) {
            var name_1 = this.NamesList()[i]();
            this.NamesList()[i](name_1 + ' iterated');
        }
        ;
    };
    return ListOperationsViewModel;
}());
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
var ObjectOperationsViewModel = /** @class */ (function () {
    function ObjectOperationsViewModel() {
    }
    ObjectOperationsViewModel.prototype.UpdatePersonsAge = function () {
        this.Person().Age(Math.floor(1));
    };
    return ObjectOperationsViewModel;
}());
var PersonDto = /** @class */ (function () {
    function PersonDto() {
    }
    return PersonDto;
}());
