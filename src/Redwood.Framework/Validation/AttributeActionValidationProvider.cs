using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Validation
{
    public class AttributeActionValidationProvider : IStaticActionValidationProvider
    {
        private static readonly ConcurrentDictionary<MethodInfo, ValidationSettingsAttribute[]> cache = new ConcurrentDictionary<MethodInfo, ValidationSettingsAttribute[]>();
        public IEnumerable<string> ModifyRulesForAction(MethodInfo methodInfo, IEnumerable<ValidationRule> rules, ref bool includeGlobalRules)
        {
            var groups = new HashSet<string>();
            var attrs = cache.GetOrAdd(methodInfo, mi =>
                mi.GetCustomAttributes<ValidationSettingsAttribute>().ToArray());
            foreach (var attr in attrs)
            {
                groups.UnionWith(attr.ModifyRulesForAction(methodInfo, rules, ref includeGlobalRules));
            }
            if (attrs.Length == 0) includeGlobalRules = true;
            return groups;
        }
    }
}
