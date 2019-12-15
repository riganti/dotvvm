using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
{
    [Options, CommandBindingExpression.Options]
    public class ControlCommandBindingExpression : CommandBindingExpression
    {
        public ControlCommandBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties)
        {
        }

        public new class OptionsAttribute : BindingCompilationOptionsAttribute
        {
            public override IEnumerable<Delegate> GetResolvers() => new Delegate[] {

            };
        }
    }

    public class ControlCommandBindingExpression<T> : ControlCommandBindingExpression, ICommandBinding<T>
    {
        public ControlCommandBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties) { }

        public new BindingDelegate<T> BindingDelegate => base.BindingDelegate.ToGeneric<T>();
    }
}