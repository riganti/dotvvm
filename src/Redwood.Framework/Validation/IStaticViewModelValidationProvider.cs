using Redwood.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Validation
{
    public interface IStaticViewModelValidationProvider
    {
        /// <summary>
        /// Gets validation rules specific to specified property
        /// </summary>
        IEnumerable<ValidationRule> GetRules(PropertyInfo property);
        /// <summary>
        /// Get validation rules not specific to any property (like sum of all properties has to be 42)
        /// </summary>
        IEnumerable<ValidationRule> GetGlobalRules(Type type);
    }
}
