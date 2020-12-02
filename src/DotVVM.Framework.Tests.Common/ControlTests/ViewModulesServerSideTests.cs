using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Tests.Common.ControlTests
{
    [TestClass]
    public class ViewModulesServerSideTests
    {
        ControlTestHelper cth = new ControlTestHelper(config: config => {
            config.Resources.Register("myModule", new ScriptModuleResource(new InlineResourceLocation("export const jsCommand = { myCommand() { } }")));
            // config.Markup.AddMarkupControl("cc", "CustomControl", "custom.dotcontrol");
            // config.Resources.Register
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task IncludeViewModule()
        {
            var r = await cth.RunPage(typeof(TestViewModel), @"
                ",
                directives: "@js myModule",
                renderResources: true
            );
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        public class TestViewModel: DotvvmViewModelBase
        {
        }
    }
}
