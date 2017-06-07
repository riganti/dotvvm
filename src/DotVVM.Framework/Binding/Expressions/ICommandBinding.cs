using System.Collections.Immutable;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Framework.Binding.Expressions
{
    public interface ICommandBinding : IBinding
    {
        ParametrizedCode CommandJavascript { get; }
        CompiledBindingExpression.BindingDelegate BindingDelegate { get; }
        ImmutableArray<IActionFilter> ActionFilters { get; }
    }
}
