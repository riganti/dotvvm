using System.ComponentModel.DataAnnotations;

namespace DotVVM.Framework.ViewModel.Validation.ClientValidation
{
    public class DotvvmEnforceClientFormatClientValidationFactory : ClientValidationFactoryBase<DotvvmEnforceClientFormatAttribute>
    {
        public override string Name { get; } = "enforceClientFormat";

        public override object[] GetParameters(DotvvmEnforceClientFormatAttribute attribute)
        {
            return new object[] { attribute.AllowNull, attribute.AllowEmptyString, attribute.AllowEmptyStringOrWhitespaces };
        }
    }
}