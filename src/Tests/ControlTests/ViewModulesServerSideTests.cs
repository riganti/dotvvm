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
using DotVVM.Framework.Testing;

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
            config.Markup.AddMarkupControl("cc", "PlainTextControl", "PlainTextControl.dotcontrol");
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
        public async Task NamedCommandWithoutViewModule_StaticCommand()
        {
            var r = await Assert.ThrowsExceptionAsync<DotvvmCompilationException>(() => cth.RunPage(typeof(object), @"
                <dot:NamedCommand Name=""Command"" Command=""{staticCommand: ;}"" />"));
            Assert.AreEqual("Validation error in NamedCommand at line 7: The NamedCommand control can be used only in pages or controls that have the @js or @csharp directive.", r.Message);
        }
        [TestMethod]
        public async Task NamedCommandWithoutViewModule_Command()
        {
            var r = await Assert.ThrowsExceptionAsync<DotvvmCompilationException>(() => cth.RunPage(typeof(object), @"
                <dot:NamedCommand Name=""Command"" Command=""{command: ;}"" />"));
            Assert.AreEqual("Validation error in NamedCommand at line 7: The NamedCommand control can be used only in pages or controls that have the @js or @csharp directive.", r.Message);
        }

        [TestMethod]
        public async Task IncludeViewModuleInControl()
        {
            var r = await cth.RunPage(typeof(TestViewModel), @"
                <cc:CustomControlWithModule />

                <dot:Repeater DataSource={value: Collection} RenderAsNamedTemplate=false>
                    <cc:CustomControlWithModule />
                    <cc:PlainTextControl />
                </dot:Repeater>
                ",
                directives: "@js viewModule",
                renderResources: true,
                markupFiles: new Dictionary<string, string> {
                    ["CustomControlWithModule.dotcontrol"] = @"
                        @js controlModule
                        @viewModel object
                        @wrapperTag div
                        <dot:Button Click={staticCommand: _js.Invoke('name', 1)} />",
                    ["PlainTextControl.dotcontrol"] = @"
                        @viewModel object
                        @noWrapperTag
                        This control should not have any module, it's just a text"
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
