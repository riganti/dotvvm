using Redwood.Framework.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Validation
{
    public class TypeValidationProvider : IStaticViewModelValidationProvider
    {
        public IEnumerable<ValidationRule> GetGlobalRules(Type type)
        {
            return new ValidationRule[0];
        }

        public IEnumerable<ValidationRule> GetRules(PropertyInfo property)
        {
            var type = property.PropertyType;
            if (ViewModelJsonConverter.IsPrimitiveType(property.PropertyType))
            {
                // primitive type is not validated
            }
            else if (ViewModelJsonConverter.IsEnumerable(property.PropertyType) &&
                type.GenericTypeArguments.Length == 1 && !ViewModelJsonConverter.IsPrimitiveType(type.GenericTypeArguments[0]))
            {
                // validate every item in collection if not primitive
                yield return new ValidationRule
                {
                    RuleName = "collection",
                    Parameters = new object[] { property.PropertyType.GenericTypeArguments[0].ToString() },
                    ValidationFunc = ValidateCollection,
                    Groups = "**"
                };
            }
            else
            {
                yield return new ValidationRule
                {
                    RuleName = "validate",
                    Parameters = new object[] { },
                    ValidationFunc = context => context.Validator.ValidateViewModel(context),
                    Groups = "**"
                };

            }
        }

        public bool ValidateCollection(RedwoodValidationContext context)
        {
            var c = context.Value as IEnumerable;
            if (c == null)
                return true;
            int i = 0;
            foreach (var val in c)
            {
                context.PushLevel(val,  i);
                context.Validator.ValidateViewModel(context);
                context.PopLevel();
                i++;
            }
            return false;
        }
    }
}
