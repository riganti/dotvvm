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

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class SimpleControlTests
    {
        ControlTestHelper cth = new ControlTestHelper(config: config => {
            config.RouteTable.Add("WithParams", "WithParams/{A}-{B:int}/{C?}", "WithParams.dothtml", new { B = 1 });
            config.RouteTable.Add("Simple", "Simple", "Simple.dothtml");
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task RouteLink()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- client rendering, no params -->
                <dot:RouteLink RenderSettings.Mode=Client RouteName=Simple Text='Click me' />
                <!-- client rendering, no params, query and suffix -->
                <dot:RouteLink RenderSettings.Mode=Client RouteName=Simple Text='Click me' Query-Binding={value: Integer} Query-Constant='c/y' UrlSuffix='#mySuffix' />
                <!-- client rendering, no params, text binding -->
                <dot:RouteLink RenderSettings.Mode=Client RouteName=Simple Text={value: Label} />

                <!-- server rendering, no params -->
                <dot:RouteLink RenderSettings.Mode=Server RouteName=Simple Text='Click me' />
                <!-- server rendering, no params, query and suffix -->
                <dot:RouteLink RenderSettings.Mode=Server RouteName=Simple Text='Click me' Query-Binding={value: Integer} Query-Constant='c/y' UrlSuffix='#mySuffix' />
                <!-- server rendering, no params, text binding -->
                <dot:RouteLink RenderSettings.Mode=Server RouteName=Simple Text={value: Label} />

                <!-- client rendering, static params -->
                <dot:RouteLink RenderSettings.Mode=Client RouteName=WithParams Param-A=A Param-B=1 Text='Click me' />
                <!-- client rendering, static params, query and suffix -->
                <dot:RouteLink RenderSettings.Mode=Client RouteName=WithParams Param-A=A Param-B=1 Text='Click me' Query-Binding={value: Integer} Query-Constant='c/y' UrlSuffix='#mySuffix' />
                <!-- client rendering, dynamic params, query and suffix -->
                <dot:RouteLink RenderSettings.Mode=Client RouteName=WithParams Param-A={value: Label} Param-B={value: Integer} Text='Click me' Query-Binding={value: Integer} Query-Constant='c/y' UrlSuffix='#mySuffix' />
                <!-- client rendering, static params, text binding -->
                <dot:RouteLink RenderSettings.Mode=Client RouteName=WithParams Param-A=A Param-B=1 Text={value: Label} />

                <!-- server rendering, static params -->
                <dot:RouteLink RenderSettings.Mode=Server RouteName=WithParams Param-A=A Param-B=1 Text='Click me' />
                <!-- server rendering, static params, query and suffix -->
                <dot:RouteLink RenderSettings.Mode=Server RouteName=WithParams Param-a=A Param-B=1 Text='Click me' Query-Binding={value: Integer} Query-Constant='c/y' UrlSuffix='#mySuffix' />
                <!-- server rendering, dynamic params, query and suffix -->
                <dot:RouteLink RenderSettings.Mode=Server RouteName=WithParams Param-A={value: Label} Param-b={value: Integer} Text='Click me' Query-Binding={value: Integer} Query-Constant='c/y' UrlSuffix='#mySuffix' />
                <!-- server rendering, static params, text binding -->
                <dot:RouteLink RenderSettings.Mode=Server RouteName=WithParams Param-A=A Param-B=1 Text={value: Label} />
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task Literal_ClientServerRendering()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- literal syntax, client rendering -->
                <span>
                    {{value: Integer}}
                    {{value: Float}}
                    {{value: DateTime}}
                </span>
                <!-- literal syntax, server rendering -->
                <span RenderSettings.Mode=Server>
                    {{value: Integer}}
                    {{value: Float}}
                    {{value: DateTime}}
                </span>
                <!-- control syntax, client rendering -->
                <span RenderSettings.Mode=Client>
                    <dot:Literal Text={value: Integer} />
                    <dot:Literal Text={value: Float} />
                    <dot:Literal Text={value: DateTime} />
                </span>
                <!-- control syntax, server rendering -->
                <span RenderSettings.Mode=Server>
                    <dot:Literal Text={value: Integer} />
                    <dot:Literal Text={value: Float} />
                    <dot:Literal Text={value: DateTime} />
                </span>
                <!-- control syntax, client rendering, format string -->
                <span RenderSettings.Mode=Client>
                    <dot:Literal Text={value: Integer} FormatString=X />
                    <dot:Literal Text={value: Float} FormatString=P2 />
                    <dot:Literal Text={value: DateTime} FormatString=dddd />
                </span>
                <!-- control syntax, server rendering, format string -->
                <span RenderSettings.Mode=Server>
                    <dot:Literal Text={value: Integer} FormatString=X />
                    <dot:Literal Text={value: Float} FormatString=P2 />
                    <dot:Literal Text={value: DateTime} FormatString=dddd />
                </span>
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task HtmlControl()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                just some attributes
                <span id=a class=x onclick='alert(1)'/>

                just some attributes with binding
                <span id={value: 'xx' + Integer} class={value: 'xx-' + Integer} class=another-class/>

                visible after loaded
                <div Visible={value: _page.EvaluatingOnClient}> X </div>

                control with just Id
                <div id=xxxx />

                class and style
                <div Class-xxx={value: Integer > 100}
                     Style-height={value: Integer + 'em'} />
                <div Class-another-class
                     Style-color=blue > X </div>
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task CommandBinding()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
            <dot:Button Click={command: Integer = Integer + 1} />
            <dot:Button Click={command: Integer = Integer - 1} Enabled={value: Integer > 10000000} />
            ");

            Assert.AreEqual(10000000, (int)r.ViewModel.@int);
            await r.RunCommand("Integer = Integer + 1");
            Assert.AreEqual(10000001, (int)r.ViewModel.@int);
            await r.RunCommand("Integer = Integer - 1");
            Assert.AreEqual(10000000, (int)r.ViewModel.@int);
            // invoking command on disabled button should fail
            var exception = await Assert.ThrowsExceptionAsync<Framework.Runtime.Commands.InvalidCommandInvocationException>(() =>
                r.RunCommand("Integer = Integer - 1")
            );
            Console.WriteLine(exception);
        }

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
        public async Task JsComponent()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <dot:JsComponent Name=Bazmek
                                 troll={resource: 1}
                                 scmd={staticCommand: (int s) => _js.Invoke<System.Threading.Tasks.Task<int>>('myCmd', s)}>

                    <template-MyTemplate>
                        <h1> Ahoj lidi </h1>
                    </template-MyTemplate>
                </dot:JsComponent>

                <dot:JsComponent Name=Bazmek troll={resource: 1} />
                <dot:JsComponent Name=Bazmek lol={value: Integer} />
            ", directives: "@js dotvvm.internal");
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        public class BasicTestViewModel: DotvvmViewModelBase
        {
            [Bind(Name = "int")]
            public int Integer { get; set; } = 10000000;
            [Bind(Name = "float")]
            public double Float { get; set; } = 0.11111;
            [Bind(Name = "date")]
            public DateTime DateTime { get; set; } = DateTime.Parse("2020-08-11T16:01:44.5141480");
            public string Label { get; } = "My Label";

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

            public class CustomerData
            {
                public int Id { get; set; }
                [Required]
                public string Name { get; set; }
                public bool Enabled { get; set; }
            }
        }
    }
}
