using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.ControlTests
{
    [TestClass]
    public class MarkupControlTests
    {
        ControlTestHelper cth = new ControlTestHelper(config: config => {
            config.Markup.AddMarkupControl("cc", "CustomControlWithCommand", "CustomControlWithCommand.dotcontrol");
        }, services: s => {
            s.AddSingleton<TestService>();
        });
        OutputChecker check = new OutputChecker(
            "testoutputs");

        [TestMethod]
        public async Task MarkupControl_PassingStaticCommand()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"

                <cc:CustomControlWithCommand DataContext={value: Integer} Click={staticCommand: s.Save(_parent.Integer)} />
                <dot:Repeater DataSource={value: Collection}>
                    <cc:CustomControlWithCommand Click={staticCommand: s.Save(_this)} />
                </dot:Repeater>
                ",
                directives: $"@service s = {typeof(TestService)}",
                markupFiles: new Dictionary<string, string> {
                    ["CustomControlWithCommand.dotcontrol"] = @"
                        @viewModel int
                        @baseType DotVVM.Framework.Tests.Common.ControlTests.CustomControlWithCommand
                        @wrapperTag div
                        <dot:Button Click={staticCommand: _control.Click()} />"
                }
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        public class BasicTestViewModel : DotvvmViewModelBase
        {
            [Bind(Name = "int")]
            public int Integer { get; set; } = 10000000;

            public List<int> Collection { get; set; } = new List<int> { 10, -20 };
        }
    }

    public class CustomControlWithCommand : DotvvmMarkupControl
    {
        public static readonly DotvvmProperty ClickProperty =
            DotvvmProperty.Register<Command, CustomControlWithCommand>("Click");
    }
}
