using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Compilation.Binding
{
    public interface IBindingIdGenerator
    {
        string GetId(ResolvedBinding binding, string fileHash);
    }
}
