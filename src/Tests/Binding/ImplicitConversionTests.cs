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
            TypeConversion.EnsureImplicitConversion(Expression.Parameter(typeof(int)), typeof(double?));
        }

        [TestMethod]
        public void Conversion_DoubleNullable()
        {
            TypeConversion.EnsureImplicitConversion(Expression.Parameter(typeof(double)), typeof(double?));
        }

        [TestMethod]
        public void Conversion_IntToDouble()
        {
            TypeConversion.EnsureImplicitConversion(Expression.Parameter(typeof(int)), typeof(double));
        }

        [TestMethod]
        public void Conversion_ValidToString()
        {
            TypeConversion.EnsureImplicitConversion(Expression.Parameter(typeof(DateTime)), typeof(string), allowToString: true);
            TypeConversion.EnsureImplicitConversion(Expression.Parameter(typeof(int)), typeof(string), allowToString: true);
            TypeConversion.EnsureImplicitConversion(Expression.Parameter(typeof(string)), typeof(string), allowToString: true);
            TypeConversion.EnsureImplicitConversion(Expression.Parameter(typeof(double)), typeof(string), allowToString: true);
            TypeConversion.EnsureImplicitConversion(Expression.Parameter(typeof(TimeSpan)), typeof(string), allowToString: true);
            TypeConversion.EnsureImplicitConversion(Expression.Parameter(typeof(Tuple<int, int>)), typeof(string), allowToString: true);
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
