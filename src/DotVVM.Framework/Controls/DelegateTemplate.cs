using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    public class DelegateTemplate : ITemplate
    {

        public Action<IControlBuilderFactory, DotvvmControl> BuildContentBody { get; set; }


        public void BuildContent(IDotvvmRequestContext context, DotvvmControl container)
        {
            var controlBuilderFactory = context.Configuration.ServiceLocator.GetService<IControlBuilderFactory>();
            BuildContentBody.Invoke(controlBuilderFactory, container);
        }
    }
}