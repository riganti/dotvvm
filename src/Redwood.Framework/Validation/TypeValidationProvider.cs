using Redwood.Framework.ViewModel;
using System;
using System.Collections;
using System.Collections.Concurrent;
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
                    Parameters = new object[] { GetUniqueTypeAlias(property.PropertyType.GenericTypeArguments[0]) },
                    ValidationFunc = ValidateCollection,
                    Groups = "**"
                };
            }
            else
            {
                yield return new ValidationRule
                {
                    RuleName = "validate",
                    Parameters = new object[] { GetUniqueTypeAlias(property.PropertyType) },
                    ValidationFunc = context => context.Validator.ValidateViewModel(context),
                    Groups = "**"
                };
            }
        }

        public readonly static ConcurrentDictionary<Type, string> Aliases = new ConcurrentDictionary<Type, string>();
        private readonly static Dictionary<string, Type> UsedAliases = new Dictionary<string, Type>();
        private readonly static object AliasCreationLock = new object();
        private static string GenerateNewAlias(Type t)
        {
            lock(AliasCreationLock)
            {
                if (!UsedAliases.ContainsKey(t.Name))
                {
                    UsedAliases.Add(t.Name, t);
                    return t.Name;
                }
                if (!UsedAliases.ContainsKey(t.FullName))
                {
                    UsedAliases.Add(t.FullName, t);
                    return t.FullName;
                }
                int i = 0;
                while (!UsedAliases.ContainsKey(t.FullName + "_" + i))
                    i++;
                UsedAliases.Add(t.FullName + "_" + i, t);
                return t.FullName + "_" + i;
            }
        }
        public static string GetUniqueTypeAlias(Type t)
        {
            return Aliases.GetOrAdd(t, GenerateNewAlias);
        }

        public static bool ValidateCollection(RedwoodValidationContext context)
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

        public static Type ResolveAlias(string name)
        {
            return UsedAliases[name];
        }
    }
}
