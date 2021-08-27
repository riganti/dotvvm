using System;

namespace DotVVM.Framework.Compilation
{
    public interface IControlBuilderFactory
    {
        (ControlBuilderDescriptor descriptor, Lazy<IControlBuilder> builder) GetControlBuilder(string virtualPath);
    }
}
