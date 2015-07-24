using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;

namespace DotVVM.Framework.Tests
{
    public static class TestExtensions
    {

        public static DotvvmBindableControl WithBinding(this DotvvmBindableControl control, DotvvmProperty property, BindingExpression expression)
        {
            control.SetBinding(property, expression);
            return control;
        }

        public static object Evaluate(this ExpressionEvaluator evaluator, string expression, object viewModel)
        {
            return evaluator.Evaluate(new ValueBindingExpression(expression), DotvvmBindableControl.DataContextProperty, new DotvvmView() { DataContext = viewModel });
        }
    }
}