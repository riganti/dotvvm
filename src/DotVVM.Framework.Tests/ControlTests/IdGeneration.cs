using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class IdGeneration
    {
        ControlTestHelper cth = new ControlTestHelper();
        // ControlTestHelper cth = new ControlTestHelper(config: config => {
        //     config.Markup.AddMarkupControl("cc", "CustomControl", "custom.dotcontrol");
        // });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod, Ignore("The tests actually fails, the bug will be fixed in a separate PR - see https://github.com/riganti/dotvvm/issues/886")]
        public async Task AutomaticIdGeneration_Repeater()
        {
            var r = await cth.RunPage(typeof(TestViewModel), @"
                <!-- just id -->
                <span id=mySpan1></span>
                <span id={value: SomeId}></span>
                <!-- repeater without id, client rendering -->
                <dot:Repeater DataSource={value: Nested.Nested}>
                    <ItemTemplate> <span id=mySpan_item></span> </ItemTemplate>
                    <SeparatorTemplate> <span id=mySpan_sep></span> </SeparatorTemplate>
                </dot:Repeater>
                <!-- repeater with id, client rendering -->
                <dot:Repeater DataSource={value: Nested.Nested} id=myRepeater>
                    <ItemTemplate> <span id=mySpan_item></span> </ItemTemplate>
                    <SeparatorTemplate> <span id=mySpan_sep></span> </SeparatorTemplate>
                </dot:Repeater>
                <!-- repeater without id, client rendering -->
                <dot:Repeater DataSource={value: Nested.Nested} RenderSettings.Mode=Server>
                    <ItemTemplate> <span id=ssr_mySpan_item></span> </ItemTemplate>
                    <SeparatorTemplate> <span id=ssr_mySpan_sep></span> </SeparatorTemplate>
                </dot:Repeater>
                <!-- repeater with id, client rendering -->
                <dot:Repeater DataSource={value: Nested.Nested} id=myRepeater RenderSettings.Mode=Server>
                    <ItemTemplate> <span id=ssr_mySpan_item></span> </ItemTemplate>
                    <SeparatorTemplate> <span id=ssr_mySpan_sep></span> </SeparatorTemplate>
                </dot:Repeater>
                "
            );
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        public class TestViewModel: DotvvmViewModelBase
        {
            public NestedViewModel Nested { get; set; } = new NestedViewModel {
                Id = "A",
                Nested = {
                    new NestedViewModel { Id = "AX" },
                    new NestedViewModel { Id = "AY" },
                    new NestedViewModel {
                        Id = "AZ",
                        Nested = {
                            new NestedViewModel { Id = "AZA" }
                        }
                    },
                }
            };

            public GridViewDataSet<NestedViewModel> NestedDataSet { get; set; } = new GridViewDataSet<NestedViewModel>();

            public List<NestedViewModel> NestedList => new List<NestedViewModel> { Nested };
            public string SomeId { get; } = "SomeId";

            public override async Task Init()
            {
                NestedDataSet.LoadFromQueryable(
                    Enumerable.Range(0, 100)
                    .Select(x => new NestedViewModel { Id = $"row {x}" })
                    .AsQueryable()
                );
                await Task.CompletedTask;
            }
        }

        public class NestedViewModel
        {
            public string Id { get; set; }
            public List<NestedViewModel> Nested { get; set; } = new List<NestedViewModel>();
        }
    }
}
