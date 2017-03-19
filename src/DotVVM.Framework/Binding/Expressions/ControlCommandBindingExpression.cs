using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.Expressions
{
    [Options]
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
}