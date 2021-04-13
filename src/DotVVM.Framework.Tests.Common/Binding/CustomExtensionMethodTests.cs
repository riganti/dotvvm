using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.Binding
{
    [TestClass]
    public class CustomExtensionMethodTests
    {
        private DotvvmConfiguration configuration;
        private MemberExpressionFactory memberExpressionFactory;

        [TestInitialize]
        public void Init()
        {
            this.configuration = DotvvmTestHelper.CreateConfiguration(services => services.AddScoped<IExtensionsProvider, TestExtensionsProvider>());
            this.memberExpressionFactory = configuration.ServiceProvider.GetRequiredService<MemberExpressionFactory>();
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

    static class TestExtensions
    {
        public static int Increment(this int number)
            => ++number;
    }

    class TestExtensionsProvider : DefaultExtensionsProvider
    {
        public TestExtensionsProvider()
        {
            AddTypeForExtensionsLookup(typeof(TestExtensions));
        }
    }
}
