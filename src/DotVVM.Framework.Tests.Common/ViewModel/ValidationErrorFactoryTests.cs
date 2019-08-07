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
        public void ValidationErrorFactory_GetPathFromExpression_WithLocalVariable()
        {
            var index = 1;
            var expression = ValidationErrorFactory.GetPathFromExpression(
                DotvvmTestHelper.DefaultConfig,
                (Expression<Func<TestViewModel, int>>)(vm => vm.Numbers[index]));
        }

        private class TestViewModel
        {
            public int[] Numbers { get; set; } = new[] { 0, 1, 1, 2, 3, 5 };
        }
    }
}
