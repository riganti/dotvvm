using System.ComponentModel.DataAnnotations;

namespace DotVVM.Framework.ViewModel.Validation.ClientValidation
{
    public class RegularExpressionClientValidationFactory : ClientValidationFactoryBase<RegularExpressionAttribute>
    {
        public override string Name { get; } = "regularExpression";

        public override object[] GetParameters(RegularExpressionAttribute attribute)
        {
            return new object[] { attribute.Pattern };
        }
    }
}