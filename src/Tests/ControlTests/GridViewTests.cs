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
using DotVVM.Framework.ResourceManagement;
using System.Security.Claims;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class GridViewTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
            config.Resources.RegisterStylesheet("test-css", new InlineResourceLocation(""));
            config.Markup.AddCodeControls("cc", exampleControl: typeof(FancyEditorWithResource));
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task GridViewColumn_CellDecorators()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <dot:GridView DataSource={value: Customers} RenderSettings.Mode=Server InlineEditing=true>
                    <dot:GridViewTextColumn HeaderText=Id ValueBinding={value: Id}>
                        <HeaderCellDecorators>
                            <dot:Decorator class={value: 'header-' + Integer} />
                        </HeaderCellDecorators>
                        <CellDecorators>
                            <dot:Decorator class={value: 'cell-' + Id} />
                        </CellDecorators>
                        <EditCellDecorators>
                            <dot:Decorator class={value: 'cell-' + Id + '-edit'} />
                        </EditCellDecorators>
                    </dot:GridViewTextColumn>
                </dot:GridView>");

            Assert.IsTrue(r.Html.QuerySelectorAll("th")[0].ClassList.Contains("header-10000000"));
            Assert.IsTrue(r.Html.QuerySelectorAll("td")[0].ClassList.Contains("cell-1-edit"));
            Assert.IsTrue(r.Html.QuerySelectorAll("td")[1].ClassList.Contains("cell-2"));
        }

        [TestMethod]
        public async Task GridViewColumn_HeaderRowDecorators()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <dot:GridView DataSource={value: Customers} RenderSettings.Mode=Server InlineEditing=true>
                    <HeaderRowDecorators>
                        <dot:Decorator class={value: 'header-' + Integer} />
                    </HeaderRowDecorators>
                    <Columns>
                        <dot:GridViewTextColumn HeaderText=Id ValueBinding={value: Id} />
                    </Columns>
                </dot:GridView>");

            Assert.IsTrue(r.Html.QuerySelectorAll("tr")[0].ClassList.Contains("header-10000000"));
        }

        [TestMethod]
        public async Task GridViewColumn_Usage_Validators()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <dot:GridView DataSource={value: Customers} InlineEditing=true>
                    <dot:GridViewTextColumn ValueBinding={value: Name} ValidatorPlacement=AttachToControl />
                </dot:GridView>");

            Assert.IsTrue(r.Html.QuerySelector("input[type=text]").GetAttribute("data-bind").Contains("dotvvm-validation: Name"));
        }

        [TestMethod]
        public async Task GridViewColumn_Usage_AttachedProperties()
        {
            var exception = await Assert.ThrowsExceptionAsync<DotvvmCompilationException>(() => 
                cth.RunPage(typeof(BasicTestViewModel), @"
                <dot:GridView DataSource={value: Customers}>
                    <dot:GridViewCheckBoxColumn Validation.Enabled=false ValueBinding={value: Enabled} />
                </dot:GridView>"));

            Assert.IsTrue(exception.Message.Contains("The column doesn't support the property Validation.Enabled! If you need to set an attached property applied to a table cell, use the CellDecorators property."));
        }

        [TestMethod]
        public async Task GridViewColumn_Usage_DataContext()
        {
            var exception = await Assert.ThrowsExceptionAsync<DotvvmCompilationException>(() => 
                cth.RunPage(typeof(BasicTestViewModel), @"
                <dot:GridView DataSource={value: Customers}>
                    <dot:GridViewTextColumn DataContext={value: _this} ValueBinding={value: Name} />
                </dot:GridView>"));

            Assert.IsTrue(exception.Message.Contains("Changing the DataContext property on the GridViewColumn is not supported!"));
        }

        [TestMethod]
        public async Task RequiredResourceInEditTemplate()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), """
                <dot:GridView DataSource={value: EmptyCustomers} RenderSettings.Mode=Client InlineEditing=true>
                    <Columns>
                        <dot:GridViewTextColumn HeaderText=Id ValueBinding={value: Id} />
                        <dot:GridViewTextColumn HeaderText=Name ValueBinding={value: Name}>
                            <EditTemplate>
                                <cc:FancyEditorWithResource />
                            </EditTemplate>
                        </dot:GridViewTextColumn>
                    </Columns>
                </dot:GridView>
                """);
            CollectionAssert.Contains(r.InitialContext.ResourceManager.RequiredResources.ToArray(), "test-css");

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        public class BasicTestViewModel: DotvvmViewModelBase
        {
            [Bind(Name = "int")]
            public int Integer { get; set; } = 10000000;

            public GridViewDataSet<CustomerData> Customers { get; set; } = new GridViewDataSet<CustomerData>() {
                RowEditOptions = {
                    EditRowId = 1,
                    PrimaryKeyPropertyName = nameof(CustomerData.Id)
                },
                Items = {
                    new CustomerData() { Id = 1, Name = "One" },
                    new CustomerData() { Id = 2, Name = "Two" }
                }
            };
            public GridViewDataSet<CustomerData> AfterPreRenderCustomers { get; set; }
            public GridViewDataSet<CustomerData> EmptyCustomers { get; set; } = new GridViewDataSet<CustomerData>();

            public override Task PreRender()
            {
                AfterPreRenderCustomers = new GridViewDataSet<CustomerData>() {
                    RowEditOptions = { EditRowId = 1, PrimaryKeyPropertyName = nameof(CustomerData.Id) },
                    Items = Customers.Items.ToList()
                };
                return base.PreRender();
            }
            public class CustomerData
            {
                public int Id { get; set; }
                [Required]
                public string Name { get; set; }
                public bool Enabled { get; set; }
            }
        }
    }

    public class FancyEditorWithResource: DotvvmControl
    {
        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            context.ResourceManager.AddRequiredResource("test-css");
            base.OnInit(context);
        }
        protected override void RenderContents(IHtmlWriter writer, Hosting.IDotvvmRequestContext context)
        {
            writer.WriteText("editor");
        }
    }
}
