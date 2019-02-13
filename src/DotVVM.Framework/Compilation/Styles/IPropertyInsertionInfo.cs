using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Compilation.Styles
{
    public interface IPropertyInsertionInfo
    {
        StyleOverrideOptions Type { get; }

        ResolvedPropertySetter GetPropertySetter(ResolvedControl resolvedControl, DotvvmConfiguration configuration);
    }
}
