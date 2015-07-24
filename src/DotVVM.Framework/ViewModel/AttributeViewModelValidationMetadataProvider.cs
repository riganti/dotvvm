using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DotVVM.Framework.ViewModel
{
    public class AttributeViewModelValidationMetadataProvider : IViewModelValidationMetadataProvider
    {
        public IEnumerable<ValidationAttribute> GetAttributesForProperty(PropertyInfo property)
        {
            return property.GetCustomAttributes<ValidationAttribute>(true);
        }
    }
}