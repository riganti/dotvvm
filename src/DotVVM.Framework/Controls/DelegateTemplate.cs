using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation;
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

        public DelegateTemplate(Action<IControlBuilderFactory, IServiceProvider, DotvvmControl> buildContentBody)
        {
            this.BuildContentBody = buildContentBody;
        }

        public DelegateTemplate(Func<IServiceProvider, DotvvmControl> buildContentBody)
        {
            this.BuildContentBody = (f, s, control) => control.Children.Add(buildContentBody(s));
        }

        public DelegateTemplate(Func<IServiceProvider, IEnumerable<DotvvmControl>> buildContentBody)
        {
            this.BuildContentBody = (f, s, control) => {
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