using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redwood.Framework.Binding;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Tests.Binding
{
    [TestClass]
    public class ExpressionEvaluatorComplexTests
    {

        /// <summary>
        /// Evaluates the binding with respect to DataContext which the target control is nested in.
        /// </summary>
        [TestMethod]
        public void ExpressionEvaluator_Complex_EvaluateInRootDataContext()
        {
            var view = new RedwoodView()
            {
                DataContext = new TestViewModel() { SubObject = new TestViewModel2() { Value = "hello" } },
                Children =
                {
                    new HtmlGenericControl("html")
                    {
                        Children =
                        {
                            new TextBox()
                            {
                                ID = "txb"    
                            }
                        }
                    }
                    .WithBinding(RedwoodBindableControl.DataContextProperty, new ValueBindingExpression("SubObject"))
                }
            };
            var textbox = view.FindControl("txb") as TextBox;

            var evaluator = new ExpressionEvaluator();
            var result = evaluator.Evaluate(new ValueBindingExpression("Value"), RedwoodBindableControl.DataContextProperty, textbox);

            Assert.IsInstanceOfType(result, typeof(string));
            Assert.AreEqual("hello", result);
        }


        class TestViewModel
        {
            public TestViewModel2 SubObject { get; set; }
        }

        class TestViewModel2
        {
            public string Value { get; set; }
        }
    }
}