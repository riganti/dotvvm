using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Tests.Binding
{
    [TestClass]
    public class ExpressionEvaluatorTests
    {

        [TestMethod]
        public void ExpressionEvaluator_Valid_PropertyAccess()
        {
            var viewModel = new
            {
                FirstName = "aa",
                LastName = new
                {
                    Name = "bb",
                    Title = new[] { 1, 2, 3 }
                }
            };

            var evaluator = new ExpressionEvaluator();
            Assert.AreEqual(viewModel.FirstName, evaluator.Evaluate("FirstName", viewModel));
            Assert.AreEqual(viewModel.LastName, evaluator.Evaluate("LastName", viewModel));
            Assert.AreEqual(viewModel.LastName.Name, evaluator.Evaluate("LastName.Name", viewModel));
            Assert.AreEqual(viewModel.LastName.Title[2], evaluator.Evaluate("LastName.Title[2]", viewModel));
            Assert.AreEqual(viewModel.LastName.Title[2], evaluator.Evaluate("_root.LastName.Title[2]", viewModel));
            Assert.AreEqual(viewModel.LastName, evaluator.Evaluate("_root.LastName.Title[2]._parent", viewModel));
            Assert.AreEqual(viewModel, evaluator.Evaluate("_root.LastName.Title[2]._parent._parent", viewModel));
        }

        [TestMethod]
        public void ExpressionEvaluator_Valid_MethodResult()
        {
            var viewModel = new TestA() { TestProp = new TestA() };

            var evaluator = new ExpressionEvaluator() { AllowMethods = true };
            Assert.AreEqual(viewModel.GetType().GetMethod("Test"), evaluator.Evaluate("Test", viewModel));
            Assert.AreEqual(viewModel.GetType().GetMethod("Test"), evaluator.Evaluate("_root.Test", viewModel));
            Assert.AreEqual(viewModel.GetType().GetMethod("Test2"), evaluator.Evaluate("_root.TestProp.Test2", viewModel));
        }

        public class TestA
        {
            public void Test(int i)
            {
            }
            public void Test2()
            {
            }

            public TestA TestProp { get; set; } 
        }
    }
}
