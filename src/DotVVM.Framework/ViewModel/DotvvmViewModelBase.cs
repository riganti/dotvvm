using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ViewModel
{
    public class DotvvmViewModelBase : IDotvvmViewModel
    {
        [JsonIgnore]
        public DotvvmRequestContext Context { get; set; }


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