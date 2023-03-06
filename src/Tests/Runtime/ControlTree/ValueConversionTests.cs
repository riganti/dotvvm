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
            try
            {

    #if !NETCOREAPP1_0
                System.Threading.Thread.CurrentThread.CurrentCulture =
                System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("cs-CZ");
    #else
                CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo("cs-CZ");
    #endif
                Assert.AreEqual(1.2, double.Parse("1,2"));
                Assert.AreEqual(1.2, ReflectionUtils.ConvertValue("1.2", typeof(double)));
            }
            finally
            {
    #if !NETCOREAPP1_0
                System.Threading.Thread.CurrentThread.CurrentCulture =
                System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
    #else
                CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
    #endif
            }
        }

        [TestMethod]
        public void ValueConversion_ErrorEnumFlags()
        {
            Assert.ThrowsException<Exception>(() =>
            {
                ReflectionUtils.ConvertValue("Local | NonPublic", typeof(DateTimeKind));
            });
        }

        [TestMethod]
        public void ValueConversion_CommaInDoubleThrowException()
        {
            Assert.ThrowsException<FormatException>(() => {
                ReflectionUtils.ConvertValue("1,2", typeof(double));
            });
            Assert.ThrowsException<FormatException>(() => {
                ReflectionUtils.ConvertValue("1,2", typeof(float));
            });
            Assert.ThrowsException<FormatException>(() => {
                ReflectionUtils.ConvertValue("1,2", typeof(decimal));
            });
            Assert.ThrowsException<FormatException>(() => {
                ReflectionUtils.ConvertValue("1,2", typeof(long));
            });
            Assert.ThrowsException<FormatException>(() => {
                ReflectionUtils.ConvertValue("1 2", typeof(int));
            });
        }

        [TestMethod]
        public void ValueConversions_BasicNumberTypes()
        {
            Assert.AreEqual(3, ReflectionUtils.ConvertValue("3", typeof(int)));
            Assert.AreEqual(3, ReflectionUtils.ConvertValue("+3", typeof(int)));
            Assert.AreEqual(-66, ReflectionUtils.ConvertValue("-66", typeof(int)));
            Assert.AreEqual(15, ReflectionUtils.ConvertValue("00015", typeof(int)));
            Assert.AreEqual((uint)3, ReflectionUtils.ConvertValue("3", typeof(uint)));
            Assert.AreEqual(long.MaxValue, ReflectionUtils.ConvertValue(long.MaxValue.ToString(), typeof(long)));
            Assert.AreEqual(ulong.MaxValue, ReflectionUtils.ConvertValue(ulong.MaxValue.ToString(), typeof(ulong)));
            Assert.AreEqual((byte)5, ReflectionUtils.ConvertValue("5", typeof(byte)));
            Assert.AreEqual((sbyte)-5, ReflectionUtils.ConvertValue("-5", typeof(sbyte)));

            Assert.AreEqual(5.5, ReflectionUtils.ConvertValue("5.5", typeof(double)));
            Assert.AreEqual(5.5f, ReflectionUtils.ConvertValue("5.5", typeof(float)));
            Assert.AreEqual(5.5m, ReflectionUtils.ConvertValue("5.5", typeof(decimal)));
            Assert.AreEqual(5e5, ReflectionUtils.ConvertValue("5e5", typeof(double)));
            Assert.AreEqual(5.5e5, ReflectionUtils.ConvertValue("5.5e5", typeof(double)));
            Assert.AreEqual(5.5e5m, ReflectionUtils.ConvertValue("5.5e5", typeof(decimal)));
        }
    }
}
