using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Binding.Expressions
{
    [BindingCompilationRequirements(
        required: new[] {typeof(BindingDelegate)}
        )]
    [Options]
    public class ResourceBindingExpression : BindingExpression, IStaticValueBinding
    {
        public ResourceBindingExpression(BindingCompilationService service, IEnumerable<object?> properties) : base(service, properties) { }

        public BindingDelegate BindingDelegate => this.bindingDelegate.GetValueOrThrow(this);

        public Type ResultType => this.resultType.GetValueOrThrow(this);

        public class OptionsAttribute : BindingCompilationOptionsAttribute
        {
            public override IEnumerable<Delegate> GetResolvers() => new Delegate[] {

            };
        }
    }

    public class ResourceBindingExpression<T> : ResourceBindingExpression, IStaticValueBinding<T>
    {
        public ResourceBindingExpression(BindingCompilationService service, IEnumerable<object?> properties) : base(service, properties) { }

        public new BindingDelegate<T> BindingDelegate => base.BindingDelegate.ToGeneric<T>();
    }
}
