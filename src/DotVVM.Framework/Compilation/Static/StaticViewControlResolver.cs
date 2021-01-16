#nullable enable

using System;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation.Static
{
    internal class StaticViewControlResolver : DefaultControlResolver
    {
        private readonly StaticViewCompiler compiler;

        public StaticViewControlResolver(
            DotvvmConfiguration configuration,
            IControlBuilderFactory controlBuilderFactory,
            StaticViewCompiler compiler,
            CompiledAssemblyCache assemblyCache)
            : base(configuration, controlBuilderFactory, assemblyCache)
        {
            this.compiler = compiler;
        }

        protected override IControlType? FindMarkupControl(string file)
        {
            var view = compiler.GetView(file);
            if (view.ViewType is null)
            {
                throw new DotvvmCompilationException(
                    $"The referenced markup control at '{file}' could not be compiled.");
            }
            return new ControlType(view.ViewType, file, view.DataContextType);
        }
    }
}
