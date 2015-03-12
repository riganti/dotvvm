using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Validation
{
    public class ValidationSettingsAttribute : Attribute
    {
        public bool ValidateAll { get; set; }
        public string[] DefineGroups { get; set; }
        public string[] IncludeProperties { get; set; }

        public ValidationSettingsAttribute()
        {
            ValidateAll = true;
            DefineGroups = new string[0];
            IncludeProperties = new string[0];
        }
        
        public virtual IEnumerable<string> ModifyRulesForAction(MethodInfo methodInfo, IEnumerable<ValidationRule> rules, ref bool includeGlobalRules)
        {
            includeGlobalRules = ValidateAll;
            var inc = new HashSet<string>(IncludeProperties);
            var group = "action:" + methodInfo.DeclaringType.Name + "." + methodInfo.Name;
            foreach (var r in rules)
            {
                if (inc.Contains(r.PropertyName) || inc.Contains(r.Property.Name))
                    r.Groups += "," + group;
            }
            return DefineGroups.Concat(new[] { group });
        }
    }
}
