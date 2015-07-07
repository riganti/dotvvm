using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    public class DelegateTemplate : ITemplate
    {

        public Func<IControlBuilderFactory, DotvvmControl> BuildContentBody { get; set; }


        public void BuildContent(DotvvmRequestContext context, DotvvmControl container)
        {
            var controlBuilderFactory = context.Configuration.ServiceLocator.GetService<IControlBuilderFactory>();
            var control = BuildContentBody(controlBuilderFactory);
            container.Children.Add(control);
        }
    }
}