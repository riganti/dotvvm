using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redwood.Framework.Hosting;
using Redwood.Framework.Validation;

namespace Redwood.Framework.ViewModel
{
    public class RedwoodViewModelBase : IRedwoodViewModel, IValidatingViewModel
    {
        [JsonIgnore]
        public RedwoodRequestContext Context { get; set; }

        public virtual IEnumerable<ValidationRule> GetRules()
        {
            return new ValidationRule[0];
        }

        public virtual IEnumerable<ValidationRule> GetRulesFor(Type t)
        {
            return new ValidationRule[0];
        }

        public virtual Task Init()
        {
            return Task.FromResult(0);
        }

        public virtual Task Load()
        {
            return Task.FromResult(0);
        }

        public virtual Task PreRender()
        {
            return Task.FromResult(0);
        }
    }
}