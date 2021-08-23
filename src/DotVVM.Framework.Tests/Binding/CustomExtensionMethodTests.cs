using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class CustomExtensionMethodTests
    {
        private ExtensionMethodsCache extensionsMethodCache;

        [TestInitialize]
        public void Init()
        {
            var configuration = DotvvmTestHelper.CreateConfiguration();
            extensionsMethodCache = configuration.ServiceProvider.GetRequiredService<ExtensionMethodsCache>();
        }

        private Expression CreateCall(MethodGroupExpression target, Expression[] args, NamespaceImport[] imports)
        {
            var memberExpressionFactory = new MemberExpressionFactory(extensionsMethodCache, imports);
            return memberExpressionFactory.Call(target, args);
        }

        [TestMethod]
        public void Call_FindCustomExtensionMethod()
        {
            var target = new MethodGroupExpression()
            {
                MethodName = nameof(TestExtensions.Increment),
                Target = Expression.Constant(11)
            };

            var expression = CreateCall(target, Array.Empty<Expression>(), new[] { new NamespaceImport("DotVVM.Framework.Tests.Binding") });
            var result = Expression.Lambda<Func<int>>(expression).Compile().Invoke();
            Assert.AreEqual(12, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Call_AmbiguousExtensionMethodsThrows()
        {
            var nonAmbiguousTarget = new MethodGroupExpression() {
                MethodName = nameof(AmbiguousExtensions.Extensions1.Decrement),
                Target = Expression.Constant(11)
            };

            // Non-ambiguous
            var expression = CreateCall(nonAmbiguousTarget, Array.Empty<Expression>(), new[] { new NamespaceImport("DotVVM.Framework.Tests.Binding.AmbiguousExtensions") });
            var result = Expression.Lambda<Func<int>>(expression).Compile().Invoke();
            Assert.AreEqual(10, result);

            var ambiguousTarget = new MethodGroupExpression() {
                MethodName = nameof(AmbiguousExtensions.Extensions1.Increment),
                Target = Expression.Constant(11)
            };

            // Ambiguous
            CreateCall(ambiguousTarget, Array.Empty<Expression>(), new[] { new NamespaceImport("DotVVM.Framework.Tests.Binding.AmbiguousExtensions") });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Call_NotImportedExtensionMethodThrows()
        {
            var importedTarget = new MethodGroupExpression() {
                MethodName = nameof(AmbiguousExtensions.Extensions1.Decrement),
                Target = Expression.Constant(11)
            };

            // Imported extension
            var expression = CreateCall(importedTarget, Array.Empty<Expression>(), new[] { new NamespaceImport("DotVVM.Framework.Tests.Binding.AmbiguousExtensions") });
            var result = Expression.Lambda<Func<int>>(expression).Compile().Invoke();
            Assert.AreEqual(10, result);

            var notImportedTarget = new MethodGroupExpression() {
                MethodName = nameof(AmbiguousExtensions.Extensions1.Decrement),
                Target = Expression.Constant(11)
            };

            // Not imported extension
            CreateCall(notImportedTarget, Array.Empty<Expression>(), new[] { new NamespaceImport("DotVVM.Framework.Tests.Binding") });
        }

        [TestMethod]
        public void Call_ExtensionMethodsWithOptionalArguments_UseDefaultValue()
        {
            var importedTarget = new MethodGroupExpression() {
                MethodName = nameof(TestExtensions.ExtensionMethodWithOptionalArgument),
                Target = Expression.Constant(11)
            };

            // Imported extension
            var expression = CreateCall(importedTarget, Array.Empty<Expression>(), new[] { new NamespaceImport("DotVVM.Framework.Tests.Binding"), new NamespaceImport("DotVVM.Framework.Tests.Binding") });
            var result = Expression.Lambda<Func<int>>(expression).Compile().Invoke();
            Assert.AreEqual(321, result);
        }

        [TestMethod]
        public void Call_ExtensionMethodsWithOptionalArguments_OverrideDefaultValue()
        {
            var importedTarget = new MethodGroupExpression() {
                MethodName = nameof(TestExtensions.ExtensionMethodWithOptionalArgument),
                Target = Expression.Constant(11)
            };

            // Imported extension
            var expression = CreateCall(importedTarget, new[] { Expression.Constant(123) }, new[] { new NamespaceImport("DotVVM.Framework.Tests.Binding") });
            var result = Expression.Lambda<Func<int>>(expression).Compile().Invoke();
            Assert.AreEqual(123, result);
        }

        [TestMethod]
        public void Call_ExtensionMethods_DuplicitImport_DoesNotThrow()
        {
            var importedTarget = new MethodGroupExpression() {
                MethodName = nameof(TestExtensions.Increment),
                Target = Expression.Constant(11)
            };

            // Imported extension
            var expression = CreateCall(importedTarget, Array.Empty<Expression>(),
                new[] {
                    new NamespaceImport("DotVVM.Framework.Tests.Binding"),
                    new NamespaceImport("DotVVM.Framework.Tests.Binding")
                });
            var result = Expression.Lambda<Func<int>>(expression).Compile().Invoke();
            Assert.AreEqual(12, result);
        }
    }

    public static class TestExtensions
    {
        public static int Increment(this int number)
            => ++number;

        public static int ExtensionMethodWithOptionalArgument(this int number, int arg = 321)
            => arg;
    }

    namespace AmbiguousExtensions
    {
        public static class Extensions1
        {
            public static int Increment(this int number)
                => ++number;

            public static int Decrement(this int number)
                => --number;
        }

        public static class Extensions2
        {
            public static int Increment(this int number)
                => ++number;
        }
    }
}
