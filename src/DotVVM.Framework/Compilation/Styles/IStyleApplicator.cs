using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Compilation.Styles
{
    public interface IStyleApplicator
    {
        void ApplyStyle(ResolvedControl control);
    }
}
