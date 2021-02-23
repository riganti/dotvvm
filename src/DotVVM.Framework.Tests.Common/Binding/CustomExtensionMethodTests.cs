using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.Binding
{
    [TestClass]
    public class CustomExtensionMethodTests
    {
        private MemberExpressionFactory memberExpressionFactory;

        [TestInitialize]
        public void Init()
        {
            var configuration = DotvvmTestHelper.CreateConfiguration();
            var extensionsCache = configuration.ServiceProvider.GetRequiredService<ExtensionMethodsCache>();
            var imports = ImmutableList.Create(new NamespaceImport("DotVVM.Framework.Tests.Common.Binding"));
            memberExpressionFactory = new MemberExpressionFactory(extensionsCache, imports);
        }

        [TestMethod]
        public void Call_FindCustomExtensionMethod()
        {
            var target = new MethodGroupExpression()
            {
                MethodName = nameof(TestExtensions.Increment),
                Target = Expression.Constant(11)
            };

            var expression = memberExpressionFactory.Call(target, Array.Empty<Expression>());
            var result = Expression.Lambda<Func<int>>(expression).Compile().Invoke();
            Assert.AreEqual(12, result);
        }
    }

    public static class TestExtensions
    {
        public static int Increment(this int number)
            => ++number;
    }
}
