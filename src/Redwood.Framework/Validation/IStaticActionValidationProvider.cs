using Redwood.Framework.Runtime.Filters;
using System.Collections.Generic;
using System.Reflection;

namespace Redwood.Framework.Validation
{
    public interface IStaticActionValidationProvider
    {
        IEnumerable<string> ModifyRulesForAction(MethodInfo methodInfo, IEnumerable<ValidationRule> rules, ref bool includeGlobalRules);
    }
}