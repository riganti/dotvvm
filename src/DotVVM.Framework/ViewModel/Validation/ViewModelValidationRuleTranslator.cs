#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class ViewModelValidationRuleTranslator : IValidationRuleTranslator
    {
        /// <summary>
        /// Gets the validation rules.
        /// </summary>
        public virtual IEnumerable<ViewModelPropertyValidationRule> TranslateValidationRules(PropertyInfo property, IEnumerable<ValidationAttribute> validationAttributes)
        {
            var addEnforceClientFormat = true;
            foreach (var attribute in validationAttributes)
            {
                var validationRule = new ViewModelPropertyValidationRule(sourceValidationAttribute: attribute, staticPropertyName: property.Name);
                // TODO: extensibility

                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                    validationRule.PropertyNameResolver = () => displayAttribute.GetName();

                switch (attribute)
                {
                    case RequiredAttribute _:
                        validationRule.ClientRuleName = "required";
                        break;
                    case RegularExpressionAttribute regularExpressionAttr:
                        validationRule.ClientRuleName = "regularExpression";
                        validationRule.Parameters = new[] { regularExpressionAttr.Pattern };
                        break;
                    case RangeAttribute rangeAttr:
                        validationRule.ClientRuleName = "range";
                        validationRule.Parameters = new[] { rangeAttr.Minimum, rangeAttr.Maximum };
                        break;
                    case DotvvmClientFormatAttribute enforceClientFormatAttr:
                        addEnforceClientFormat = false;
                        if (enforceClientFormatAttr.Disable)
                            break;

                        validationRule.ClientRuleName = "enforceClientFormat";
                        validationRule.Parameters = new object[] {
                                                            enforceClientFormatAttr.AllowNull,
                                                            enforceClientFormatAttr.AllowEmptyString,
                                                            enforceClientFormatAttr.AllowEmptyStringOrWhitespaces };
                        break;
                    case EmailAddressAttribute _:
                        validationRule.ClientRuleName = "emailAddress";
                        break;
                    default:
                        validationRule.ClientRuleName = string.Empty;
                        break;
                }

                yield return validationRule;
            }
            // enforce client format by default
            if (addEnforceClientFormat && (property.PropertyType.IsNullable() && property.PropertyType.UnwrapNullableType().IsNumericType() || property.PropertyType.UnwrapNullableType().IsDateOrTimeType()))
            {
                var enforceClientFormatAttr = new DotvvmClientFormatAttribute();

                var validationRule =
                    new ViewModelPropertyValidationRule(sourceValidationAttribute: enforceClientFormatAttr,
                        staticPropertyName: property.Name) {
                        Parameters = new object[] {
                            enforceClientFormatAttr.AllowNull,
                            enforceClientFormatAttr.AllowEmptyString,
                            enforceClientFormatAttr.AllowEmptyStringOrWhitespaces
                        },
                        ClientRuleName = "enforceClientFormat"
                    };

                yield return validationRule;
            }

        }
    }
}
