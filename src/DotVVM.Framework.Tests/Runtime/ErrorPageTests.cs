using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting.ErrorPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Testing;
using System.IO;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class ErrorPageTests
    {
        static DotvvmConfiguration config = DotvvmTestHelper.CreateConfiguration();
        ErrorFormatter formatter = CreateFormatter();
        BindingCompilationService bcs = config.ServiceProvider.GetService<BindingCompilationService>().WithoutInitialization();
        IDotvvmRequestContext context = DotvvmTestHelper.CreateContext(config);

        private static ErrorFormatter CreateFormatter()
        {
            var errorFormatter = ErrorFormatter.CreateDefault();
            return errorFormatter;
        }

        private static Exception ThrowAndCatch(Exception exception) =>
            CatchError(() => throw exception);

        private static Exception CatchError(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                return ex;
            }
            throw new Exception("Expected exception");
        }

        [TestMethod]
        public void InvalidBindingException()
        {
            var binding = new ValueBindingExpression(bcs, new object[] { });
            var exception = Assert.ThrowsException<BindingPropertyException>(() => binding.KnockoutExpression);
            var tt = formatter.ErrorHtml(exception, context.HttpContext);

            Assert.IsTrue(tt.Contains(exception.GetType().FullName));
            Assert.IsTrue(tt.Contains(exception.Message));

            // binding tab should contain the binding name
            Assert.IsTrue(tt.Contains(binding.GetType().FullName));

            // the exception contains the property name
            Assert.IsTrue(tt.Contains("DotVVM.Framework.Binding.Properties.KnockoutExpressionBindingProperty"));
        }
    }
}
