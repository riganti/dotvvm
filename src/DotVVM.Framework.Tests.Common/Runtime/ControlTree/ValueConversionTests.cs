using DotVVM.Framework.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    [TestClass]
    public class ValueConversionTests
    {
        [TestMethod]
        public void ValueConversion_Enum()
        {
            Assert.AreEqual(DateTimeKind.Local, ReflectionUtils.ConvertValue("Local", typeof(DateTimeKind)));
            Assert.AreEqual(DateTimeKind.Unspecified, ReflectionUtils.ConvertValue("Unspecified", typeof(DateTimeKind)));
        }

        [TestMethod]
        public void ValueConversion_EnumFlags()
        {
            Assert.AreEqual(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, ReflectionUtils.ConvertValue("Instance, Public, NonPublic", typeof(BindingFlags)));
            Assert.AreEqual(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, ReflectionUtils.ConvertValue("Static | Public | NonPublic", typeof(BindingFlags)));
        }

        [TestMethod]
        public void ValueConversion_DoubleInStrangeCulture()
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo("cs-CZ");
            Assert.AreEqual(1.2, double.Parse("1,2"));
            Assert.AreEqual(1.2, ReflectionUtils.ConvertValue("1.2", typeof(double)));
        }

        [TestMethod]
        public void ValueConversion_ErrorEnumFlags()
        {
            Assert.ThrowsException<Exception>(() =>
            {
                ReflectionUtils.ConvertValue("Local | NonPublic", typeof(DateTimeKind));
            });
        }
    }
}
