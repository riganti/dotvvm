using System;
using System.Globalization;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        public async Task GridViewColumn_Usage()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <dot:GridView DataSource={value: Customers}>
                    <dot:GridViewCheckBoxColumn ValueBinding={value: Enabled} />
                </dot:GridView>");
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

        public class BasicTestViewModel: DotvvmViewModelBase
        {
            [Bind(Name = "int")]
            public int Integer { get; set; } = 10000000;
            [Bind(Name = "float")]
            public double Float { get; set; } = 0.11111;
            [Bind(Name = "date")]
            public DateTime DateTime { get; set; } = DateTime.Parse("2020-08-11T16:01:44.5141480");
            public string Label { get; } = "My Label";

            public GridViewDataSet<CustomerData> Customers { get; set; } = new GridViewDataSet<CustomerData>();

            public class CustomerData
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
                public bool Enabled { get; set; }
            }
        }
    }
}
