using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DotVVM.Framework.ViewModel.Validation
{
    public class ViewModelValidationRuleTranslator
    {
        //public Dictionary<Type, ViewModelPropertyValidationRule> rulesForType = new Dictionary<Type, ViewModelPropertyValidationRule>
        //{
        //    { typeof(int), new ViewModelPropertyValidationRule("intrange", null, "not an valid integer", int.MinValue, int.MaxValue) },
        //    { typeof(long), new ViewModelPropertyValidationRule("intrange", null, "not an valid integer", long.MinValue, long.MaxValue) },
        //    { typeof(uint), new ViewModelPropertyValidationRule("intrange", null, "not an valid integer", uint.MinValue, uint.MaxValue) },
        //    { typeof(ulong), new ViewModelPropertyValidationRule("intrange", null, "not an valid integer", ulong.MinValue, ulong.MaxValue) },
        //    { typeof(short), new ViewModelPropertyValidationRule("intrange", null, "not an valid integer", short.MinValue, short.MaxValue) },
        //    { typeof(ushort), new ViewModelPropertyValidationRule("intrange", null, "not an valid integer", ushort.MinValue, ushort.MaxValue) },
        //    { typeof(sbyte), new ViewModelPropertyValidationRule("intrange", null, "not an valid integer", sbyte.MinValue, sbyte.MaxValue) },
        //    { typeof(byte), new ViewModelPropertyValidationRule("intrange", null, "not an valid integer", byte.MinValue, byte.MaxValue) },
        //};
        //public static readonly ViewModelPropertyValidationRule RequiredNotNull = new ViewModelPropertyValidationRule("notnull", null, "invalid value");

        /// <summary>
        /// Gets the validation rules.
        /// </summary>
        public IEnumerable<ViewModelPropertyValidationRule> TranslateValidationRules(PropertyInfo property, IEnumerable<ValidationAttribute> validationAttributes)
        {
            //// property type
            //ViewModelPropertyValidationRule typeRule;
            //if (rulesForType.TryGetValue(property.PropertyType, out typeRule))
            //    yield return typeRule;

            foreach (var attribute in validationAttributes)
            {
                // TODO: extensibility
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
                        ClientRuleName = "regularExpression",
                        SourceValidationAttribute = attribute,
                        ErrorMessage = attribute.FormatErrorMessage(property.Name),
                        Parameters = new Object[] { typedAttribute.Pattern }
                    };
                }
                else if (attribute is RangeAttribute)
                {
                    var typed = (RangeAttribute)attribute;
                    yield return new ViewModelPropertyValidationRule
                    {
                        ClientRuleName = "range",
                        SourceValidationAttribute = attribute,
                        ErrorMessage = attribute.FormatErrorMessage(property.Name),
                        Parameters = new object[] { typed.Minimum, typed.Maximum }
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