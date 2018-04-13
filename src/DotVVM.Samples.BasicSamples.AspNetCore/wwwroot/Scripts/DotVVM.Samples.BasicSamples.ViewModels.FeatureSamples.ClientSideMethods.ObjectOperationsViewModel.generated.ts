/// <reference path="../../../DotVVM.Framework/Resources/Scripts/typings/knockout/knockout.d.ts" />
class ObjectOperationsViewModel {
	public Person: KnockoutObservable<PersonDto>;
	public UpdatePersonsAge()
		{
		this.Person().Age(Math.floor(1));
		}
}
