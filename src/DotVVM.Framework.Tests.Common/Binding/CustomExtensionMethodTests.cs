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
        private DotvvmConfiguration configuration;
        private MemberExpressionFactory memberExpressionFactory;

        [TestInitialize]
        public void Init()
        {
            this.configuration = DotvvmTestHelper.CreateConfiguration();
            this.memberExpressionFactory = configuration.ServiceProvider.GetRequiredService<MemberExpressionFactory>();
            this.memberExpressionFactory.ImportedNamespaces = ImmutableList.Create(new NamespaceImport("DotVVM.Framework.Tests.Common.Binding"));
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
