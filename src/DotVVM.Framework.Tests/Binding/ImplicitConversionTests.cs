using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using DotVVM.Framework.Compilation.Binding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class ImplicitConversionTests
    {
        [TestMethod]
        public void Conversion_IntToNullableDouble()
        {
            TypeConversion.ImplicitConversion(Expression.Parameter(typeof(int)), typeof(double?), throwException: true);
        }

        [TestMethod]
        public void Conversion_DoubleNullable()
        {
            TypeConversion.ImplicitConversion(Expression.Parameter(typeof(double)), typeof(double?), throwException: true);
        }

        [TestMethod]
        public void Conversion_IntToDouble()
        {
            TypeConversion.ImplicitConversion(Expression.Parameter(typeof(int)), typeof(double), throwException: true);
        }

        [TestMethod]
        public void Conversion_ValidToString()
        {
            TypeConversion.ImplicitConversion(Expression.Parameter(typeof(DateTime)), typeof(string), throwException: true, allowToString: true);
            TypeConversion.ImplicitConversion(Expression.Parameter(typeof(int)), typeof(string), throwException: true, allowToString: true);
            TypeConversion.ImplicitConversion(Expression.Parameter(typeof(string)), typeof(string), throwException: true, allowToString: true);
            TypeConversion.ImplicitConversion(Expression.Parameter(typeof(double)), typeof(string), throwException: true, allowToString: true);
            TypeConversion.ImplicitConversion(Expression.Parameter(typeof(TimeSpan)), typeof(string), throwException: true, allowToString: true);
            TypeConversion.ImplicitConversion(Expression.Parameter(typeof(Tuple<int, int>)), typeof(string), throwException: true, allowToString: true);
        }

        [TestMethod]
        public void Conversion_InvalidToString()
        {
            // System.Linq.Expression does not override ToString, so the conversion is invalid
            Assert.IsNull(TypeConversion.ImplicitConversion(Expression.Parameter(typeof(Expression)), typeof(string)));
            Assert.IsNull(TypeConversion.ImplicitConversion(Expression.Parameter(typeof(object)), typeof(string)));
        }
    }
}
