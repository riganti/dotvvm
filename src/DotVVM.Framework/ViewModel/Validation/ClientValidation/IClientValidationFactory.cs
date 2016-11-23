using System.ComponentModel.DataAnnotations;

namespace DotVVM.Framework.ViewModel.Validation.ClientValidation
{
    public interface IClientValidationFactory<in T>
    {
        string Name { get; }
        object[] GetParameters(T attribute);
    }

    public interface IClientValidationFactory
    {
        string Name { get; }
        object[] GetParameters(ValidationAttribute attribute);
        IViewModelPropertyValidationRule CreateViewModelPropertyValidationRule(ValidationAttribute attribute, string propertyName);
    }
}