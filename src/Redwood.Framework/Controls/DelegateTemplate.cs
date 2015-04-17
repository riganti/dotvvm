using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    public class DelegateTemplate : ITemplate
    {

        public Func<IControlBuilderFactory, RedwoodControl> BuildContentBody { get; set; }


        public void BuildContent(RedwoodRequestContext context, RedwoodControl container)
        {
            var controlBuilderFactory = context.Configuration.ServiceLocator.GetService<IControlBuilderFactory>();
            var control = BuildContentBody(controlBuilderFactory);
            container.Children.Add(control);
        }
    }
}