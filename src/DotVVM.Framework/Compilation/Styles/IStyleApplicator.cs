using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Compilation.Styles
{
    public interface IStyleApplicator
    {
        void ApplyStyle(ResolvedControl control, DotvvmConfiguration configuration);
    }
}
