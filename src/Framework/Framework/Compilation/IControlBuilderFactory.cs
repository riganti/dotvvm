using System;
using DotVVM.Framework.Compilation.ViewCompiler;

namespace DotVVM.Framework.Compilation
{
    public interface IControlBuilderFactory
    {
        (ControlBuilderDescriptor descriptor, Lazy<IControlBuilder> builder) GetControlBuilder(string virtualPath);
    }
}
