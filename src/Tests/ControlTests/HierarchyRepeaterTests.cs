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
using System.Security.Claims;
using System.Collections;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class HierarchyRepeaterTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
            config.RouteTable.Add("Simple", "Simple", "Simple.dothtml");
            config.Markup.AddMarkupControl("cc", "ButtonControl", "ButtonControl.dotcontrol");
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task UsageWithThisInBindings()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <dot:HierarchyRepeater DataSource={value: HItems}
                                           ItemChildrenBinding={value: Children}
                                           RenderSettings.Mode=Server>
                        <ItemTemplate>
                            {{resource: Label}}
                        </ItemTemplate>
                    </dot:HierarchyRepeater>
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CommandInMarkupControl(bool clientRendering)
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), $$"""

                    <dot:HierarchyRepeater DataSource={value: HItems}
                                           ItemChildrenBinding={value: Children}
                                           RenderSettings.Mode={{(clientRendering ? "Client" : "Server")}}>
                        <ItemTemplate>
                            <cc:ButtonControl DataContext={value: _this} Click={command: _root.SelectedLabel = _this.Label} />
                        </ItemTemplate>
                    </dot:HierarchyRepeater>
                """,
                directives: $"@service s = {typeof(TestService)}",
                markupFiles: new Dictionary<string, string> {
                    ["ButtonControl.dotcontrol"] = """
                        @viewModel DotVVM.Framework.Tests.ControlTests.HierarchyRepeaterTests.BasicTestViewModel.SaneHierarchicalItem
                        @property DotVVM.Framework.Binding.Expressions.Command Click

                                    <dot:Button Click={command: _control.Click()} Text={value: _this.Label} />
                        """
                },
                fileName: "CommandInMarkupControl" + (clientRendering ? "Client" : "Server")
            );

            // await r.RunCommand("_control.Click()", vm => vm is BasicTestViewModel.SaneHierarchicalItem { Label: "A_1_2" });
            // Assert.AreEqual("A_1_2", (string)r.ViewModel.SelectedLabel);
            check.CheckString(
                r.OutputString,
                checkName: clientRendering ? "client" : "server",
                fileExtension: "html"
            );
        }

        public class BasicTestViewModel: DotvvmViewModelBase
        {
            public List<SaneHierarchicalItem> HItems { get; set; } = new () {
                new() {
                    Label = "A",
                    Children = {
                        new() {
                            Label = "A_1",
                            Children = {
                                new() { Label = "A_1_1" },
                                new() { Label = "A_1_2" }
                            }
                        }
                    }
                },
                new() { Label = "B" },
            };

            public class SaneHierarchicalItem
            {
                public string Label { get; set; }
                public List<SaneHierarchicalItem> Children { get; set; } = new List<SaneHierarchicalItem>();
            }

            public string SelectedLabel { get; set; }
        }
    }
}
