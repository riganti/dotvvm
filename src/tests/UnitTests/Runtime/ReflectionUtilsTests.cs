using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class ReflectionUtilsTests
    {
        [TestMethod]
        [DataRow(typeof(Task<string>), typeof(string))]
        [DataRow(typeof(string), typeof(string))]
        [DataRow(typeof(ValueTask<string>), typeof(string))]
        [DataRow(typeof(Task), typeof(void))]
        public void UnwrapTaskTypeTest(Type taskType, Type type)
        {
            var actualType = ReflectionUtils.UnwrapTaskType(taskType);
            Assert.AreEqual(actualType, type);
        }
    }
}
