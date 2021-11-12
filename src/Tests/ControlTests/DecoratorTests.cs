using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Runtime;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class DecoratorTests
    {
        ControlTestHelper cth = new ControlTestHelper(config: config => {
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task Decorator()
        {
            var r = await cth.RunPage(typeof(object), @"
                <dot:Decorator class=c1>
                    <div /> 
                </dot:Decorator>
                <dot:Decorator class=c2>
                    <%-- comment --%>
                    <div /> 
                </dot:Decorator>
                <dot:Decorator class=c3>
                    <!-- comment -->
                    <div /> 
                </dot:Decorator>
            ");
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }
        [TestMethod]
        public async Task Decorator_MultipleChildren()
        {
            var e = await Assert.ThrowsExceptionAsync<DotvvmCompilationException>(() =>
                cth.RunPage(typeof(object), @"
                    <dot:Decorator class=c1>
                        <div /> 
                        <span />
                    </dot:Decorator>
                "));
            Assert.AreEqual(e.Message, "Validation error in Decorator at line 7: Decorator must have only one child control.");
        }
        [TestMethod]
        public async Task Decorator_NoChild()
        {
            var e = await Assert.ThrowsExceptionAsync<DotvvmControlException>(() =>
                cth.RunPage(typeof(object), @"
                    <dot:Decorator class=c1></dot:Decorator>
                "));
            Assert.AreEqual(e.Message, "Decorator must have a child control");
        }
    }
}
