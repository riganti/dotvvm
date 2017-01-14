using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
{
    public interface IUpdatableValueBinding: IBinding
    {
        CompiledBindingExpression.BindingUpdateDelegate UpdateDelegate { get; }
        //t+ void UpdateSource(object value, DotvvmBindableObject control, DotvvmProperty property);
    }
}