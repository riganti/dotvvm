using System;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;

namespace DotVVM.Compiler
{
    public class StaticViewControlResolver : DefaultControlResolver
    {
        public StaticViewControlResolver(
            DotvvmConfiguration configuration,
            IControlBuilderFactory controlBuilderFactory)
            : base(configuration.Markup, controlBuilderFactory)
        {
        }

        protected override IControlType FindMarkupControl(string file)
        {
            throw new NotImplementedException();
        }
    }
}
