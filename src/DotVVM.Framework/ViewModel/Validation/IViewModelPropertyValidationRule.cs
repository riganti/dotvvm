using System.ComponentModel.DataAnnotations;

namespace DotVVM.Framework.ViewModel.Validation
{
    public interface IViewModelPropertyValidationRule
    {
        string ClientRuleName { get; set; }
        string ErrorMessage { get; set; }
        object[] Parameters { get; set; }
        ValidationAttribute SourceValidationAttribute { get; set; }
    }
}