var MasterPageViewModel = /** @class */ (function () {
    function MasterPageViewModel() {
        this.Title = ko.observable();
    }
    MasterPageViewModel.prototype.SetTitleToEmpty = function () {
        this.Title('');
    };
    return MasterPageViewModel;
}());
var ListOperationsViewModel = /** @class */ (function () {
    function ListOperationsViewModel() {
        this.NamesList = ko.observableArray();
        this.Index = ko.observable();
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
            if (!(name_1.indexOf('iterated') !== -1))
                this.NamesList()[i](name_1 + ' iterated');
        }
        ;
    };
    ListOperationsViewModel.prototype.SetTitleToEmpty = function () {
        this.Title('');
    };
    return ListOperationsViewModel;
}());
var MathematicalOperationsViewModel = /** @class */ (function () {
    function MathematicalOperationsViewModel() {
        this.Left = ko.observable();
        this.Right = ko.observable();
        this.Result = ko.observable();
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
    MathematicalOperationsViewModel.prototype.SetTitleToEmpty = function () {
        this.Title('');
    };
    return MathematicalOperationsViewModel;
}());
var MultipleTypeOperationsViewModel = /** @class */ (function () {
    function MultipleTypeOperationsViewModel() {
        this.Title = ko.observable();
        this.Number = ko.observable();
        this.IsVisible = ko.observable();
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
    MultipleTypeOperationsViewModel.prototype.SetTitleToEmpty = function () {
        this.Title('');
    };
    return MultipleTypeOperationsViewModel;
}());
var ObjectOperationsViewModel = /** @class */ (function () {
    function ObjectOperationsViewModel() {
        this.Person = ko.observable();
        this.Name = ko.observable();
        this.Age = ko.observable();
        this.Persons = ko.observableArray();
    }
    ObjectOperationsViewModel.prototype.UpdatePersonsAge = function () {
        this.Person().Age(Math.floor(1));
    };
    ObjectOperationsViewModel.prototype.CreateNewPerson = function () {
        this.Person(new PersonDto('Karel', 27));
    };
    ObjectOperationsViewModel.prototype.AddPerson = function (name, age) {
        this.Persons.push(ko.observable(new PersonDto(name, age)));
    };
    ObjectOperationsViewModel.prototype.RemovePerson = function (dto) {
        this.Persons.remove(function (item) { var rawItem = ko.unwrap(item); return rawItem == dto; });
    };
    ObjectOperationsViewModel.prototype.SetTitleToEmpty = function () {
        this.Title('');
    };
    return ObjectOperationsViewModel;
}());
var PersonDto = /** @class */ (function () {
    function PersonDto(name, age) {
        this.Name = ko.observable();
        this.Age = ko.observable();
        this.Name(name);
        this.Age(Math.floor(age));
    }
    return PersonDto;
}());
