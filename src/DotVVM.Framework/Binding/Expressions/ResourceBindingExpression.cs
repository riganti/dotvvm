using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding.Expressions
{
    [BindingCompilationRequirements(
        required: new[] {typeof(CompiledBindingExpression.BindingDelegate)}
        )]
    [Options]
    public class ResourceBindingExpression : BindingExpression, IStaticValueBinding
    {
        public ResourceBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties) { }

        public CompiledBindingExpression.BindingDelegate BindingDelegate => this.GetProperty<CompiledBindingExpression.BindingDelegate>();

        public Type ResultType => this.GetProperty<ResultTypeBindingProperty>().Type;

        public class OptionsAttribute : BindingCompilationOptionsAttribute
        {
            public override IEnumerable<Delegate> GetResolvers() => new Delegate[] {

            };
        }
    }

    public class ResourceBindingExpression<T> : ResourceBindingExpression, IStaticValueBinding<T>
    {
        public ResourceBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties) { }

        public new CompiledBindingExpression.BindingDelegate<T> BindingDelegate => base.BindingDelegate.ToGeneric<T>();
    }
}