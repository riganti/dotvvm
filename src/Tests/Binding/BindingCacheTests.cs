using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime.Caching;
using DotVVM.Framework.Tests;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Binding
{
    [TestClass]
    public class BindingCacheTests
    {
        private BindingCompilationService service = DotvvmTestHelper.DefaultConfig.ServiceProvider.GetService<BindingCompilationService>();

        [TestMethod]
        public void ThisBinding()
        {
            var dataContext = DataContextStack.Create(typeof(string));

            var binding1 = ValueBindingExpression.CreateThisBinding<string>(service, dataContext);
            var binding2 = ValueBindingExpression.CreateThisBinding<string>(service, dataContext);
            var binding3 = ValueBindingExpression.CreateThisBinding<object>(service, dataContext);
            var binding4 = ValueBindingExpression.CreateThisBinding<string>(service, DataContextStack.Create(typeof(string), dataContext));
            var binding5 = ValueBindingExpression.CreateThisBinding<string>(service, DataContextStack.Create(typeof(string), dataContext));

            Assert.AreEqual(binding1, binding2);
            Assert.AreNotEqual(binding1, binding3);
            Assert.AreNotEqual(binding1, binding4);
            Assert.AreNotEqual(binding2, binding3);
            Assert.AreNotEqual(binding2, binding4);
            Assert.AreNotEqual(binding3, binding4);
            Assert.AreEqual(binding4, binding5);
        }
    }
}
