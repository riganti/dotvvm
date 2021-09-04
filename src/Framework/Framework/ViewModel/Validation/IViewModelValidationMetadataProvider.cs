using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DotVVM.Framework.ViewModel.Validation
{
    public interface IViewModelValidationMetadataProvider
    {

        IEnumerable<ValidationAttribute> GetAttributesForProperty(PropertyInfo property);

    }
}
