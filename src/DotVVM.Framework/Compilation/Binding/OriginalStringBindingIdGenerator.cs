using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Compilation.Binding
{
    public class OriginalStringBindingIdGenerator : IBindingIdGenerator
    {
        public string GetId(ResolvedBinding binding, string fileHash)
        {
            return binding.Value;
        }
    }
}
