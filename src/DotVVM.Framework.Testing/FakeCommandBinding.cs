using System;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Runtime.Filters;
using System.Collections.Immutable;

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

        public object? GetProperty(Type type, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException)
        {
            if (errorMode == ErrorHandlingMode.ReturnNull)
                return null;
            else if (errorMode == ErrorHandlingMode.ReturnException)
                return new NotImplementedException();
            else throw new NotImplementedException();
        }
    }

}
