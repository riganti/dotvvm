using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.ViewModel
{
    [TestClass]
    public class ValidationErrorFactoryTests
    {

        [TestMethod]
        public void GetPathFromExpression_WithPrimitiveLocal_CorrectExpression()
        {
            var index = 1;
            var expression = ValidationErrorFactory.GetPathFromExpression(
                DotvvmTestHelper.DefaultConfig,
                (Expression<Func<TestViewModel, int>>)(vm => vm.Numbers[index]));
            Assert.AreEqual("Numbers/1", expression);
        }

        [TestMethod]
        public void GetPathFromExpression_WithComplexLocal_CorrectExpression()
        {
            var sample = new Sample { Index = 42 };

            var complex = ValidationErrorFactory.GetPathFromExpression(
                DotvvmTestHelper.DefaultConfig,
                (Expression<Func<TestViewModel, int>>)(vm => vm.Numbers[sample.Index]));
            Assert.AreEqual("Numbers/42", complex);
        }

        [TestMethod]
        public void GetPathFromExpression_MultipleLocals_CorrectExpression()
        {
            // test that the cache built into ValidationErrorFactory does not leak

            Assert.AreEqual("Numbers/1", GetNumbers(1));
            Assert.AreEqual("Numbers/2", GetNumbers(2));
            Assert.AreEqual("Numbers/3", GetNumbers(3));
        }

        [TestMethod]
        public void GetPathFromExpression_SameLocalValue_SameReference()
        {
            // test that the cache built into ValidationErrorFactory actually works

            var expr1 = GetNumbers(1);
            var expr2 = GetNumbers(1);

            // the expression must be referentially equal if they are from the cache
            Assert.IsTrue((object)expr1 == expr2);
        }

        private string GetNumbers(int index)
        {
            return ValidationErrorFactory.GetPathFromExpression(
                DotvvmTestHelper.DefaultConfig,
                (Expression<Func<TestViewModel, int>>)(vm => vm.Numbers[index]));
        }

        private class Sample
        {
            public int[] Indices = { 3, 2, 1 };

            public int Index { get; set; }
        }

        private class TestViewModel
        {
            public int[] Numbers { get; set; } = new[] { 0, 1, 1, 2, 3, 5 };
        }
    }
}
