using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
{
    [Options, ValueBindingExpression.Options]
    public class ControlPropertyBindingExpression : ValueBindingExpression
    {
        public ControlPropertyBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties) { }

        public new class OptionsAttribute : BindingCompilationOptionsAttribute
        {
            public override IEnumerable<Delegate> GetResolvers() => new Delegate[] {

            };
        }
    }

    public class ControlPropertyBindingExpression<T> : ControlPropertyBindingExpression, IValueBinding<T>, IUpdatableValueBinding<T>
    {
        public ControlPropertyBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties) { }

        public new BindingDelegate<T> BindingDelegate => base.BindingDelegate.ToGeneric<T>();

        public new BindingUpdateDelegate<T> UpdateDelegate => base.UpdateDelegate.ToGeneric<T>();
    }
}
