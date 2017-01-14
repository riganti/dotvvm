using System;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
{
    public interface IStaticValueBinding: IBinding
    {
        CompiledBindingExpression.BindingDelegate BindingDelegate { get; }
        Type ResultType { get; }
        //T+ object Evaluate(DotvvmBindableObject control, DotvvmProperty property);
    }
}
