using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.Binding
{
    [TestClass]
    public class ExpressionHelperTests
    {
        [TestMethod]
        public void UpdateMember_GetValue()
        {
            var cP = Expression.Parameter(typeof(DotvvmControl), "c");
            var newValueP = Expression.Parameter(typeof(object), "newValue");
            var updateExpr = ExpressionHelper.UpdateMember(ExpressionUtils.Replace((DotvvmControl c) => c.GetValue(DotvvmBindableObject.DataContextProperty, true), cP), newValueP);
            Assert.IsNotNull(updateExpr);
            Assert.AreEqual("c.SetValue(DotvvmBindableObject.DataContextProperty, newValue)", updateExpr.ToString());
        }

        [TestMethod]
        public void UpdateMember_NormalProperty()
        {
            var vmP = Expression.Parameter(typeof(Tests.Binding.TestViewModel), "vm");
            var newValueP = Expression.Parameter(typeof(DateTime), "newValue");
            var updateExpr = ExpressionHelper.UpdateMember(ExpressionUtils.Replace((Tests.Binding.TestViewModel c) => c.DateFrom, vmP), newValueP);
            Assert.IsNotNull(updateExpr);
            Assert.AreEqual("(vm.DateFrom = Convert(newValue, Nullable`1))", updateExpr.ToString());
        }

        [TestMethod]
        public void UpdateMember_ReadOnlyProperty()
        {
            var vmP = Expression.Parameter(typeof(Tests.Binding.TestViewModel), "vm");
            var newValueP = Expression.Parameter(typeof(long[]), "newValue");
            var updateExpr = ExpressionHelper.UpdateMember(ExpressionUtils.Replace((Tests.Binding.TestViewModel c) => c.LongArray, vmP), newValueP);
            Assert.IsNull(updateExpr);
        }
    }
}
