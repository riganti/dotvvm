using System.ComponentModel.DataAnnotations;

namespace DotVVM.Framework.ViewModel.Validation.ClientValidation
{
    public class RequiredClientValidationFactory : ClientValidationFactoryBase<RequiredAttribute>
    {
        public override string Name { get; } = "required";
    }
}