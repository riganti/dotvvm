using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
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


        [DataTestMethod]
        [UnificationDataSource]
        public void TypeUnificationTest(Type a, Type b, Type[] expectedGenericTypes)
        {
            var unifiedTypes = new Dictionary<Type, Type>();
            Assert.IsTrue(ReflectionUtils.TryUnifyGenericTypes(a, b, unifiedTypes));

            // map dictionary to array of method generic arguments
            var realResults = new Type[expectedGenericTypes.Length];
            foreach (var t in unifiedTypes)
                realResults[t.Key.GenericParameterPosition] = t.Value;

            // null-out generic types which are expected to stay generic
            for (int i = 0; i < expectedGenericTypes.Length; i++)
                if (expectedGenericTypes[i].IsGenericParameter && expectedGenericTypes[i].GenericParameterPosition == i)
                    expectedGenericTypes[i] = null;
            XAssert.Equal(expectedGenericTypes, realResults);
        }

        class UnificationDataSource : Attribute, ITestDataSource
        {
            public IEnumerable<object[]> GetData(MethodInfo methodInfo) =>
                from m in typeof(UnificationDataSource).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                where m.Name.StartsWith("Test")
                let parameters = m.GetParameters()
                select new object[] {
                    parameters[0].ParameterType,
                    parameters[1].ParameterType,
                    parameters.Skip(2).Select(p => p.ParameterType).ToArray()
                };
            public string GetDisplayName(MethodInfo methodInfo, object[] data) =>
                $"{methodInfo.Name}({string.Join(", ", data.Select(d => d.ToString()))})";

            // Test cases: first two arguments are unified together,
            // the rest are the expected types unified into the type arguments
            public void Test0<T>(T a, string b, string expected) { }
            public void Test1<T>(List<T> a, List<string> b, string expected) { }
            public void Test2<T, U>(Func<T, string> a, Func<int, U> b, int expected, string expected2) { }
            public void TestTypeUsedMultipleTime0<T>(Func<Func<int>> a, Func<T> b, Func<int> expected) { }
            public void TestTypeUsedMultipleTime1<T>(Func<Func<int>> a, Func<Func<T>> b, int expected) { }
            public void TestTypeUsedMultipleTime2<T, U>((IEnumerable<T>, IEnumerable<(int, int)>) a, (IEnumerable<IEnumerable<int>>, IEnumerable<U>) b, IEnumerable<int> expected, (int, int) expected2) { }
            public void TestPartial0<T>(T a, T b, T expected) { }
            public void TestPartial1<T, U>(Func<string, T> a, Func<U, T> b, T expected, string expected2) { }
            public void TestPartial2<T, U>(Tuple<T, string> a, U b, T expected, Tuple<T, string> expected2) { }

        }
    }
}
