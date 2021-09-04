using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class DotvvmErrorTests
    {
        ControlTestHelper cth = new ControlTestHelper(config: config => {
        }, services: s => {
            s.AddSingleton<TestService>();
        });
        OutputChecker check = new OutputChecker(
            "testoutputs");

        [TestMethod]
        public async Task JsDirective_NotScriptModule()
        {
            var r = await Assert.ThrowsExceptionAsync<DotvvmCompilationException>(() => cth.RunPage(typeof(object), @"", directives: "@js dotvvm"));

            Assert.AreEqual("The resource named 'dotvvm' referenced by the @js directive must be of the ScriptModuleResource type!", r.Message);
            Assert.AreEqual("dotvvm", r.AffectedSpans.Single().Trim());
        }
        [TestMethod]
        public async Task JsDirective_NotFound()
        {
            var r = await Assert.ThrowsExceptionAsync<DotvvmCompilationException>(() => cth.RunPage(typeof(object), @"", directives: "@js ResourceDoesNotExist"));

            Assert.AreEqual("Cannot find resource named 'ResourceDoesNotExist' referenced by the @js directive!", r.Message);
            Assert.AreEqual("ResourceDoesNotExist", r.AffectedSpans.Single().Trim());
        }
    }
}
