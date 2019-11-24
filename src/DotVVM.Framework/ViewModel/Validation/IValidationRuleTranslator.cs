#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ViewModel.Validation
{
    public interface IValidationRuleTranslator
    {
        IEnumerable<ViewModelPropertyValidationRule> TranslateValidationRules(PropertyInfo property, IEnumerable<ValidationAttribute> validationAttributes);
    }
}
