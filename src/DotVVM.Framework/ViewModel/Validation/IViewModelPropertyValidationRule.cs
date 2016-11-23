using System.ComponentModel.DataAnnotations;

namespace DotVVM.Framework.ViewModel.Validation
{
    public interface IViewModelPropertyValidationRule
    {
        string ClientRuleName { get; }
        string ErrorMessage { get; }
        object[] Parameters { get; }
        ValidationAttribute SourceValidationAttribute { get; }
    }
}