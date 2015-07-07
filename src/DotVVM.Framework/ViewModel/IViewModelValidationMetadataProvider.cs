using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DotVVM.Framework.ViewModel
{
    public interface IViewModelValidationMetadataProvider
    {

        IEnumerable<ValidationAttribute> GetAttributesForProperty(PropertyInfo property);

    }
}