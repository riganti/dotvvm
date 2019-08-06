using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.Binding
{
    [TestClass]
    public class DotvvmBindableObjectTests
    {

        [TestMethod]
        public void DotvvmBindableObject_SetValueExists()
        {
            // required by the ExpressionHelper.UpdateMember method
            var method = typeof(DotvvmBindableObject)
                .GetMethod(nameof(DotvvmBindableObject.SetValue), new[] {
                    typeof(DotvvmProperty),
                    typeof(object)
                });
            Assert.IsNotNull(method);
        }
    }
}
