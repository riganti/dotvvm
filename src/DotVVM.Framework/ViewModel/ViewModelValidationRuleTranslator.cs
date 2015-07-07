using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DotVVM.Framework.ViewModel
{
    public class ViewModelValidationRuleTranslator
    { 

        /// <summary>
        /// Gets the validation rules.
        /// </summary>
        public IEnumerable<ViewModelPropertyValidationRule> TranslateValidationRules(PropertyInfo property, IEnumerable<ValidationAttribute> validationAttributes)
        {
            foreach (var attribute in validationAttributes) 
            {
                if (attribute is RequiredAttribute)
                {
                    yield return new ViewModelPropertyValidationRule()
                    {
                        ClientRuleName = "required",
                        SourceValidationAttribute = attribute,
                        ErrorMessage = attribute.FormatErrorMessage(property.Name)
                    };
                }
                else if (attribute is RegularExpressionAttribute)
                {
                    var typedAttribute = (RegularExpressionAttribute)attribute;
                    yield return new ViewModelPropertyValidationRule()
                    {
                        ClientRuleName = "regexp",
                        SourceValidationAttribute = attribute,
                        ErrorMessage = attribute.FormatErrorMessage(property.Name),
                        Parameters = new Object[] { typedAttribute.Pattern }
                    };
                }
                else
                {
                    yield return new ViewModelPropertyValidationRule()
                    {
                        ClientRuleName = string.Empty,
                        SourceValidationAttribute = attribute,
                        ErrorMessage = attribute.FormatErrorMessage(property.Name)
                    };
                }
            }
        }

    }
}