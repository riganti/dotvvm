using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;

namespace DotVVM.Framework.Tests
{
    public static class TestExtensions
    {
        public static DotvvmControl WithBinding(this DotvvmControl control, DotvvmProperty property, BindingExpression expression)
        {
            control.SetBinding(property, expression);
            return control;
        }
    }
}