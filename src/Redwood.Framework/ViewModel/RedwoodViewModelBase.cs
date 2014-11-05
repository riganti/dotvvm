using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redwood.Framework.Hosting;

namespace Redwood.Framework.ViewModel
{
    public class RedwoodViewModelBase : IRedwoodViewModel
    {
        [JsonIgnore]
        public RedwoodRequestContext Context { get; set; }


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