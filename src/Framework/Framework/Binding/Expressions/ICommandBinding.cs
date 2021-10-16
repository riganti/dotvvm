using System.Collections.Immutable;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Framework.Binding.Expressions
{
    public interface ICommandBinding : IBinding
    {
        ParametrizedCode CommandJavascript { get; }
        BindingDelegate BindingDelegate { get; }
        ImmutableArray<IActionFilter> ActionFilters { get; }
    }

    public interface ICommandBinding<out T>: ICommandBinding
    {
        new BindingDelegate<T> BindingDelegate { get; }
    }

    public interface IStaticCommandBinding : ICommandBinding
    {
        /// <summary> Returns code that should be placed in dotvvm.applyPostbackHandlers </summary>
        ParametrizedCode OptionsLambdaJavascript { get; }
    }
    public interface IStaticCommandBinding<out T> : ICommandBinding<T>, IStaticCommandBinding
    {
    }
}
