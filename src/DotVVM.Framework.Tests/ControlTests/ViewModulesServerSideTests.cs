using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Compilation;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class ViewModulesServerSideTests
    {
        ControlTestHelper cth = new ControlTestHelper(config: config => {
            config.Resources.Register("myModule", new ScriptModuleResource(new InlineResourceLocation("export const jsCommand = { myCommand() { } }")));
            config.Resources.Register("viewModule", new ScriptModuleResource(new InlineResourceLocation("export const jsCommand = { myCommand() { } }")));
            config.Resources.Register("controlModule", new ScriptModuleResource(new InlineResourceLocation("export const jsCommand = { myCommand() { } }")));
            config.Markup.AddMarkupControl("cc", "CustomControlWithModule", "CustomControlWithModule.dotcontrol");
            // config.Resources.Register
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task IncludeViewModule()
        {
            var r = await cth.RunPage(typeof(object), @"
                ",
                directives: "@js myModule",
                renderResources: true
            );
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task NamedCommandWithoutViewModule()
        {
            var r = await Assert.ThrowsExceptionAsync<DotvvmCompilationException>(() => cth.RunPage(typeof(object), @"
                <dot:NamedCommand Name=""Command"" Command=""{staticCommand: ;}"" />"));
            Assert.AreEqual("The NamedCommand control can be used only in pages or controls that have the @js directive.", r.Message);
        }

        [TestMethod]
        public async Task IncludeViewModuleInControl()
        {
            var r = await cth.RunPage(typeof(TestViewModel), @"

                <cc:CustomControlWithModule />

                <dot:Repeater DataSource={value: Collection}>
                    <cc:CustomControlWithModule />
                </dot:Repeater>
                ",
                directives: "@js viewModule",
                renderResources: true,
                markupFiles: new Dictionary<string, string> {
                    ["CustomControlWithModule.dotcontrol"] = @"
                        @js controlModule
                        @viewModel object
                        @wrapperTag div
                        <dot:Button Click={staticCommand: _js.Invoke('name', 1)} />"
                }
            );
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        public class TestViewModel: DotvvmViewModelBase
        {
            public List<string> Collection { get; set; } = new List<string> { "A", "B" };
        }
    }
}
