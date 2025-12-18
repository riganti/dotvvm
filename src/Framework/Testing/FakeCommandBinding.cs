using System;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Runtime.Filters;
using System.Collections.Immutable;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using System.Diagnostics.CodeAnalysis;

namespace DotVVM.Framework.Testing
{
    public class FakeCommandBinding : ICommandBinding
    {
        private readonly ParametrizedCode commandJavascript;
        private readonly BindingDelegate bindingDelegate;

        public FakeCommandBinding(ParametrizedCode commandJavascript, BindingDelegate bindingDelegate)
        {
            this.commandJavascript = commandJavascript;
            this.bindingDelegate = bindingDelegate;
        }
        public ParametrizedCode CommandJavascript => commandJavascript ?? throw new NotImplementedException();

        public BindingDelegate BindingDelegate => bindingDelegate ?? throw new NotImplementedException();

        public ImmutableArray<IActionFilter> ActionFilters => ImmutableArray<IActionFilter>.Empty;

        public DataContextStack? DataContext => null;

        public BindingResolverCollection? GetAdditionalResolvers() => null;

        public object? GetProperty(Type type, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException)
        {
            if (errorMode == ErrorHandlingMode.ReturnNull)
                return null;
            else if (errorMode == ErrorHandlingMode.ReturnException)
                return new NotImplementedException();
            else throw new NotImplementedException();
        }

        public T GetProperty<T>() => (T)GetProperty(typeof(T), ErrorHandlingMode.ThrowException)!;

        public bool TryGetPropety<T>([NotNullWhen(true)] out T? value) => TryGetPropety<T>(out value, out _);

        public bool TryGetPropety<T>([NotNullWhen(true)] out T? value, [NotNullWhen(false)] out Exception? error)
        {
            switch (GetProperty(typeof(T), ErrorHandlingMode.ReturnException))
            {
                case Exception e: (error, value) = (e, default(T)); return false;
                case T v: (error, value) = (null, v); return true;
                default: throw new Exception();
            }
        }
    }

}
