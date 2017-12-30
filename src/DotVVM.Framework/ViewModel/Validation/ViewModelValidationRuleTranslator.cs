using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class ViewModelValidationRuleTranslator : IValidationRuleTranslator
    {
        /// <summary>
        /// Gets the validation rules.
        /// </summary>
        public virtual IEnumerable<ViewModelPropertyValidationRule> TranslateValidationRules(PropertyInfo property, IEnumerable<ValidationAttribute> validationAttributes)
        {
            foreach (var attribute in validationAttributes)
            {
                // TODO: extensibility
                if (attribute is RequiredAttribute)
                {
                    yield return new ViewModelPropertyValidationRule()
                    {
                        ClientRuleName = "required",
                        SourceValidationAttribute = attribute,
                        PropertyName = property.Name
                    };
                }
                else if (attribute is RegularExpressionAttribute)
                {
                    var typedAttribute = (RegularExpressionAttribute)attribute;
                    yield return new ViewModelPropertyValidationRule()
                    {
                        ClientRuleName = "regularExpression",
                        SourceValidationAttribute = attribute,
                        PropertyName = property.Name,
                        Parameters = new object[] { typedAttribute.Pattern }
                    };
                }
                else if (attribute is RangeAttribute)
                {
                    var typed = (RangeAttribute)attribute;
                    yield return new ViewModelPropertyValidationRule
                    {
                        ClientRuleName = "range",
                        SourceValidationAttribute = attribute,
                        PropertyName = property.Name,
                        Parameters = new object[] { typed.Minimum, typed.Maximum }
                    };
                }
                else if (attribute is DotvvmEnforceClientFormatAttribute)
                {
                    var typed = (DotvvmEnforceClientFormatAttribute)attribute;
                    yield return new ViewModelPropertyValidationRule
                    {
                        ClientRuleName = "enforceClientFormat",
                        SourceValidationAttribute = attribute,
                        PropertyName = property.Name,
                        Parameters = new object[] { typed.AllowNull, typed.AllowEmptyString, typed.AllowEmptyStringOrWhitespaces }
                    };
                }
                else
                {
                    yield return new ViewModelPropertyValidationRule()
                    {
                        ClientRuleName = string.Empty,
                        SourceValidationAttribute = attribute,
                        PropertyName = property.Name
                    };
                }
            }
        }
    }
}
