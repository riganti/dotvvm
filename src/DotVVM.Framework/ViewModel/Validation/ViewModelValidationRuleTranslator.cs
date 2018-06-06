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
                var validationRule = new ViewModelPropertyValidationRule(sourceValidationAttribute: attribute, propertyName: property.Name);
                // TODO: extensibility

                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                    validationRule.PropertyName = displayAttribute.GetName();

                if (attribute is RequiredAttribute)
                {
                    validationRule.ClientRuleName = "required";
                }
                else if (attribute is RegularExpressionAttribute)
                {
                    var typedAttribute = (RegularExpressionAttribute)attribute;

                    validationRule.ClientRuleName = "regularExpression";
                    validationRule.Parameters = new[] { typedAttribute.Pattern };
                }
                else if (attribute is RangeAttribute)
                {
                    var typed = (RangeAttribute)attribute;

                    validationRule.ClientRuleName = "range";
                    validationRule.Parameters = new[] { typed.Minimum, typed.Maximum };
                }
                else if (attribute is DotvvmEnforceClientFormatAttribute)
                {
                    var typed = (DotvvmEnforceClientFormatAttribute)attribute;

                    validationRule.ClientRuleName = "enforceClientFormat";
                    validationRule.Parameters = new object[] { typed.AllowNull, typed.AllowEmptyString, typed.AllowEmptyStringOrWhitespaces };
                }
                else
                {
                    validationRule.ClientRuleName = string.Empty;
                }

                yield return validationRule;
            }
        }
    }
}
