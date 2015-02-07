using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Validation
{
    public class DataAnnotationsValidationProvider : IStaticViewModelValidationProvider
    {
        public virtual IEnumerable<ValidationRule> GetGlobalRules(Type type)
        {
            return new ValidationRule[0];
        }

        public virtual IEnumerable<ValidationRule> GetRules(PropertyInfo property)
        {
            var validationAttributes = this.GetValidationAttributes(property);
            foreach (var attribute in validationAttributes)
            {
                if (attribute is RequiredAttribute)
                {
                    yield return new ValidationRule()
                    {
                        RuleName = "required",
                        ValidationFunc = c => attribute.IsValid(c.Value),
                        ErrorMessage = attribute.FormatErrorMessage(property.Name)
                    };
                }
                else if (attribute is RegularExpressionAttribute)
                {
                    var typedAttribute = (RegularExpressionAttribute)attribute;
                    yield return new ValidationRule()
                    {
                        RuleName = "regexp",
                        ValidationFunc = c => attribute.IsValid(c.Value),
                        ErrorMessage = attribute.FormatErrorMessage(property.Name),
                        Parameters = new Object[] { typedAttribute.Pattern }
                    };
                }
            }
        }

        protected virtual IEnumerable<ValidationAttribute> GetValidationAttributes(PropertyInfo prop)
        {
            return prop.GetCustomAttributes<ValidationAttribute>(true);
        }
    }
}
