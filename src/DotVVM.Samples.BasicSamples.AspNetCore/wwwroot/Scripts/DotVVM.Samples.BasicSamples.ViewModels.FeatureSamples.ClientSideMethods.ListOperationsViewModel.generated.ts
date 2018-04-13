/// <reference path="../../../DotVVM.Framework/Resources/Scripts/typings/knockout/knockout.d.ts" />
class ListOperationsViewModel {
	public Names: KnockoutObservableArray<string>;
	public NamesList: KnockoutObservableArray<string>;
	public RemoveTest()
		{
this.Remove('test');
		}
	public Remove(name : string)
		{
		    this.NamesList.remove(function (item) { var rawItem = ko.unwrap(item); return rawItem == name;});
		}
}
