using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
{
    public interface IStaticValueBinding: IBinding
    {
        object Evaluate(DotvvmBindableObject control, DotvvmProperty property);
    }
}
