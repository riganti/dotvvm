using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Controls
{
    public class DelegateTemplate : ITemplate
    {
        public Action<IControlBuilderFactory, IServiceProvider, DotvvmControl> BuildContentBody { get; set; }

        public DelegateTemplate()
        {
            // this constructor must be here otherwise the user controls won't compile
        }

        public DelegateTemplate(Action<IServiceProvider, DotvvmControl> buildContentBody)
        {
            BuildContentBody = (_, s, container) => buildContentBody(s, container);
        }

        public DelegateTemplate(Action<IControlBuilderFactory, IServiceProvider, DotvvmControl> buildContentBody)
        {
            BuildContentBody = buildContentBody;
        }

        public DelegateTemplate(Func<IServiceProvider, DotvvmControl> buildContentBody)
        {
            BuildContentBody = (_, s, container) => container.Children.Add(buildContentBody(s));
        }

        public DelegateTemplate(Func<IServiceProvider, IEnumerable<DotvvmControl>> buildContentBody)
        {
            BuildContentBody = (_, s, control) =>
            {
                foreach (var c in buildContentBody(s))
                    control.Children.Add(c);
            };
        }

        public void BuildContent(IDotvvmRequestContext context, DotvvmControl container)
        {
            var controlBuilderFactory = context.Services.GetService<IControlBuilderFactory>();
            BuildContentBody.Invoke(controlBuilderFactory, context.Services, container);
        }
    }
}
