using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Validation
{
    public interface IValidatingViewModel
    {
        IEnumerable<ValidationRule> GetRules();
        IEnumerable<ValidationRule> GetRulesFor(Type t);
    }
}
