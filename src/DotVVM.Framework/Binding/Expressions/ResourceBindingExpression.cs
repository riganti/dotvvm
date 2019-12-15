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
        required: new[] {typeof(BindingDelegate)}
        )]
    [Options]
    public class ResourceBindingExpression : BindingExpression, IStaticValueBinding
    {
        public ResourceBindingExpression(BindingCompilationService service, IEnumerable<object> properties) : base(service, properties) { }

        public BindingDelegate BindingDelegate => this.GetProperty<BindingDelegate>();

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

        public new BindingDelegate<T> BindingDelegate => base.BindingDelegate.ToGeneric<T>();
    }
}