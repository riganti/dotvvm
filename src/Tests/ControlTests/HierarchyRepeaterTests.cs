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
        }
    }
}
