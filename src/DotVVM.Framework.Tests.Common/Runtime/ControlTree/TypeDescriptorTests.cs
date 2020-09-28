using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.Runtime.ControlTree
{
    [TestClass]
    public class TypeDescriptorTests
    {
        [TestMethod]
        public void TypeDescriptorUtils_GetPropertyOnInterface()
        {
            var type = typeof(IBaseGridViewDataSet<ITestDataSource>);
            var value = TypeDescriptorUtils.GetCollectionItemType(new ResolvedTypeDescriptor(type));

            Assert.IsNotNull(value);
            Assert.AreEqual("ITestDataSource", value.Name);
        }
        [TestMethod]
        public void TypeDescriptorUtils_GetPropertyOnDataSet()
        {
            var type = typeof(GridViewDataSet<ITestDataSource>);
            var value = TypeDescriptorUtils.GetCollectionItemType(new ResolvedTypeDescriptor(type));

            Assert.IsNotNull(value);
            Assert.AreEqual("ITestDataSource", value.Name);
        }
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TypeDescriptorUtils_GetPropertyOnObject()
        {
            var type = typeof(object);
            TypeDescriptorUtils.GetCollectionItemType(new ResolvedTypeDescriptor(type));
        }
    }
}
