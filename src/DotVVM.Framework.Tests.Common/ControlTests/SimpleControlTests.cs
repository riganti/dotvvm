using System;
using System.Globalization;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DotVVM.Framework.Tests.Common.ControlTests
{
    [TestClass]
    public class SimpleControlTests
    {
        ControlTestHelper cth = new ControlTestHelper();
        OutputChecker check = new OutputChecker("testoutputs");

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
                <!-- IncludeInPage=false -->
                <dot:Literal Text=XX IncludeInPage={resource: false} RenderSettings.Mode=Client />
                <!-- DataContext, Events.Click -->
                <dot:Literal DataContext={value: Float} Text={value: _this} IncludeInPage={value: _parent.Integer > 400} Events.Click={staticCommand: 0} />
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
        public async Task ClickEvents()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <dot:Button Click={command: Integer = Integer + 1} onclick=""alert('Runs before the command')"" >Classic Btn</dot:Button>
                <dot:LinkButton Click={command: Integer = Integer + 1} onclick=""alert('Runs before the command')"" Text='Link Btn' />
                <div Events.Click={staticCommand: 0}> </div> <%-- TODO: onclick=""alert('Runs before the command')"" --%>
                <div Events.DoubleClick={staticCommand: 0}> </div>
            ");

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task EmptyData()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- empty data, server -->
                <dot:EmptyData IncludeInPage={value: True} DataSource={value: EmptyDataSet} RenderSettings.Mode=Server>
                    Data is empty
                </dot:EmptyData>
                <!-- empty data, client -->
                <dot:EmptyData IncludeInPage={value: True} DataSource={value: EmptyDataSet} RenderSettings.Mode=Client>
                    Data is empty
                </dot:EmptyData>
                <!-- non empty data, server -->
                <dot:EmptyData IncludeInPage={value: True} DataSource={value: NonEmptyDataSet} RenderSettings.Mode=Server>
                    Data is empty
                </dot:EmptyData>
                <!-- non empty data, client -->
                <dot:EmptyData IncludeInPage={value: True} DataSource={value: NonEmptyDataSet} RenderSettings.Mode=Client>
                    Data is empty
                </dot:EmptyData>

                <!-- non empty data, client -->
                <dot:EmptyData IncludeInPage={resource: true} DataSource={value: NonEmptyDataSet} RenderSettings.Mode=Client>
                    Data is empty
                </dot:EmptyData>
            ");

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task HtmlElements()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- InnerText -->
                <div InnerText={value: Integer}></div>
                <div InnerText={value: Integer} RenderSettings.Mode=Server>Something different</div>
                <div InnerText='My Inner Text'>another text</div>

                <!-- css classes -->
                <div class-staticClass1 class=staticClass3 />
                <div class-dynamicClass={value: Integer > 200} />
                <!-- binding attributes -->
                <div class={value: Integer > 200 ? 'class1' : 'class2'} data-x={value: Integer > 200 ? 'x1' : 'x2'} />
                <!-- binding attributes, SSR -->
                <div class={value: Integer > 200 ? 'class1' : 'class2'} data-x={value: Integer > 200 ? 'x1' : 'x2'} RenderSettings.Mode=Server />
                <!-- css style -->
                <div style-background-color=red />
                <div style-border-width={value: Integer} />
                <div style-border-radius={resource: Integer} />

                <!-- visible -->
                <div Visible={value: !True} />
                <div Visible={resource: !True} />
            ");

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

            public bool True { get; set; } = true;

            public GridViewDataSet<string> EmptyDataSet { get; set; } = new GridViewDataSet<string>();
            public GridViewDataSet<string> NonEmptyDataSet { get; set; } = new GridViewDataSet<string> {
                Items = { "yayay" }
            };
        }
    }
}
