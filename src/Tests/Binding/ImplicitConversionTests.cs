using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        public void Conversion_NullConversion()
        {
            TypeConversion.EnsureImplicitConversion(Expression.Constant(null, typeof(object)), typeof(string));
            TypeConversion.EnsureImplicitConversion(Expression.Constant(null, typeof(object)), typeof(int?));
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

        [TestMethod]
        public void Conversion_EnumerableToObject()
        {
            CheckEnumerableConversion(new List<DateOnly> { new DateOnly(2024, 1, 2), new DateOnly(2024, 3, 4) });
            CheckEnumerableConversion(new[] { 1, 2 });
            CheckEnumerableConversion(new List<int?> { 1, null, 2 });
            CheckEnumerableConversion(new List<string> { "a", "b" });
            CheckEnumerableConversion(Array.Empty<DateOnly>());
            CheckEnumerableConversion(ImmutableArray.Create(1, 2));
        }

        private static void CheckEnumerableConversion<T>(IEnumerable<T> source)
        {
            foreach (var sourceType in new[] { source.GetType(), typeof(IEnumerable<T>) })
            {
                var parameter = Expression.Parameter(sourceType);
                var converted = TypeConversion.EnsureImplicitConversion(parameter, typeof(IEnumerable<object>));
                var result = (IEnumerable<object>)Expression.Lambda(converted, parameter).Compile().DynamicInvoke(source);
                CollectionAssert.AreEqual(source.Cast<object>().ToArray(), result.ToArray());
            }
        }

        [TestMethod]
        public void Conversion_EnumerableDoesNotAllowInvalidElementConversions()
        {
            Assert.IsNull(TypeConversion.ImplicitConversion(Expression.Parameter(typeof(List<DateOnly>)), typeof(IEnumerable<string>)));
            Assert.IsNull(TypeConversion.ImplicitConversion(Expression.Parameter(typeof(List<int>)), typeof(IList<object>)));
            Assert.IsNull(TypeConversion.ImplicitConversion(Expression.Parameter(typeof(int)), typeof(IEnumerable<object>)));
            Assert.IsNull(TypeConversion.ImplicitConversion(Expression.Parameter(typeof(string)), typeof(IEnumerable<object>)));
        }
    }
}
