using System.ComponentModel.DataAnnotations;

namespace DotVVM.Framework.ViewModel.Validation.ClientValidation
{
    public class RangeClientValidationFactory : ClientValidationFactoryBase<RangeAttribute>
    {
        public override string Name { get; } = "range";

        public override object[] GetParameters(RangeAttribute attribute)
        {
            return new[] { attribute.Minimum, attribute.Maximum };
        }
    }
}