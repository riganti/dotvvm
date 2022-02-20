using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.DynamicData
{
    [TestClass]
    public class FieldPropertiesBindingProviderTests
    {

        [DataTestMethod]
        [DataRow("Insert", "Insert", true)]
        [DataRow("Insert", "Edit", false)]
        [DataRow("Insert|Edit", "Edit", true)]
        [DataRow("  Insert||Edit", "Edit", true)]
        [DataRow("Insert || Insert  ||edit  ", "Edit", true)]
        [DataRow("!Insert", "Edit", true)]
        [DataRow("!Edit & !Insert", "Edit", false)]
        [DataRow("!Edit & !Insert", "ReadOnly", true)]
        public void ViewNameExpressionParserTest_ValidExpressions(string expression, string currentViewName, bool expectedResult)
        {
            var result = ConditionalFieldBindingProvider.ProcessExpression(expression, i => new(string.Equals(i, currentViewName, StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(expectedResult, result.BoxedValue);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("  ")]
        [DataRow("|  ")]
        [DataRow("  &&&")]
        [DataRow("  &| k")]
        [DataRow(" !")]
        [DataRow(" !|")]
        [DataRow(" !  & k")]
        [DataRow("k!")]
        [DataRow("k&")]
        [DataRow("k|i|j&&& a ")]
        [DataRow("k|! ")]
        [DataRow("!!a ")]
        [ExpectedException(typeof(DotvvmControlException))]
        public void ViewNameExpressionParserTest_Invalid(string expression)
        {
            var result = ConditionalFieldBindingProvider.ProcessExpression(expression, i => new(true));
        }
    }
}
