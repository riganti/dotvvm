using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Tests
{
    public static class TestExtensions
    {

        public static RedwoodBindableControl WithBinding(this RedwoodBindableControl control, RedwoodProperty property, BindingExpression expression)
        {
            control.SetBinding(property, expression);
            return control;
        }

        public static object Evaluate(this ExpressionEvaluator evaluator, string expression, object viewModel)
        {
            return evaluator.Evaluate(new ValueBindingExpression(expression), RedwoodBindableControl.DataContextProperty, new RedwoodView() { DataContext = viewModel });
        }
    }
}