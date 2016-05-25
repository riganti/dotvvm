using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class AttributeViewModelValidationMetadataProvider : IViewModelValidationMetadataProvider
    {
        public IEnumerable<ValidationAttribute> GetAttributesForProperty(PropertyInfo property)
        {
            return property.GetCustomAttributes<ValidationAttribute>(true);
        }
    }
}