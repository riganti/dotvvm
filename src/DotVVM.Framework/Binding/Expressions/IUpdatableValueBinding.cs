using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
{
    public interface IUpdatableValueBinding
    {
        void UpdateSource(object value, DotvvmBindableObject control, DotvvmProperty property);
    }
}