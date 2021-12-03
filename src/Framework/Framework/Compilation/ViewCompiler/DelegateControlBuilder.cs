using System;
using DotVVM.Framework.Compilation.ViewCompiler;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation
{
    public record DelegateControlBuilder : IControlBuilder
    {
        private readonly Func<IControlBuilderFactory, IServiceProvider, DotvvmControl> controlBuilderDelegate;

        public DelegateControlBuilder(ControlBuilderDescriptor controlBuilderDescriptor, Func<IControlBuilderFactory, IServiceProvider, DotvvmControl> controlBuilderDelegate)
        {
            Descriptor = controlBuilderDescriptor;
            this.controlBuilderDelegate = controlBuilderDelegate;
        }

        public ControlBuilderDescriptor Descriptor { get; }

        public DotvvmControl BuildControl(IControlBuilderFactory controlBuilderFactory, IServiceProvider services)
        {
            return controlBuilderDelegate(controlBuilderFactory, services);
        }
    }
}
