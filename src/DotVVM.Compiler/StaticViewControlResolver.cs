using System;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;

namespace DotVVM.Compiler
{
    public class StaticViewControlResolver : DefaultControlResolver
    {
        private readonly StaticViewCompiler compiler;

        public StaticViewControlResolver(
            DotvvmConfiguration configuration,
            IControlBuilderFactory controlBuilderFactory,
            StaticViewCompiler compiler)
            : base(configuration.Markup, controlBuilderFactory)
        {
            this.compiler = compiler;
        }

        protected override IControlType? FindMarkupControl(string file)
        {
            var view = compiler.GetView(file);
            if (view.ViewType is null)
            {
                throw new InvalidOperationException(
                    $"The '{file}' markup control was not compiled. This is likely a compiler bug.");
            }
            return new ControlType(view.ViewType, file, view.DataContextType);
        }
    }
}
