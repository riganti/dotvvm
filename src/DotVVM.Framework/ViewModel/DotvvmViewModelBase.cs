using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ViewModel
{
    public class DotvvmViewModelBase : IDotvvmViewModel
    {
        [JsonIgnore]
        public IDotvvmRequestContext Context { get; set; }


        public virtual Task Init()
        {
            return TaskUtils.GetCompletedTask();
        }

        public virtual Task Load()
        {
            return TaskUtils.GetCompletedTask();
        }

        public virtual Task PreRender()
        {
            return TaskUtils.GetCompletedTask();
        }
    }
}