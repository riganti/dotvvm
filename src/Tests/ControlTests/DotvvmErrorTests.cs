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
using DotVVM.Framework.Tests.Runtime;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class DotvvmErrorTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
        }, services: s => {
            s.Services.AddSingleton<TestService>();
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

        [TestMethod]
        public async Task HtmlLiteral_InvalidWrapperTagUsage()
        {
            await check.CheckExceptionAsync(() =>
                cth.RunPage(typeof(object), @" <dot:HtmlLiteral Html='' RenderWrapperTag=false WrapperTagName=span />"));
        }

        [TestMethod]
        public async Task HtmlLiteral_InvalidWrapperTagUsage2()
        {
            var e = await Assert.ThrowsExceptionAsync<DotvvmControlException>(() =>
                cth.RunPage(typeof(object), @" <dot:HtmlLiteral Html='' RenderWrapperTag=false class=my-class />"));
            Assert.AreEqual("Cannot set HTML attributes, Visible, ID, Postback.Update, ... bindings on a control which does not render its own element!", e.Message);
        }

        [TestMethod]
        public async Task AuthView_InvalidWrapperTagUsage()
        {
            await check.CheckExceptionAsync(() =>
                cth.RunPage(typeof(object), @" <dot:AuthenticatedView WrapperTagName=span> </dot:AuthenticatedView>"));
        }
    }
}
