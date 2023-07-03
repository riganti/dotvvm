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

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class SimpleControlTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
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
                <!-- client rendering, dynamic suffix -->
                <dot:RouteLink RenderSettings.Mode=Client RouteName=Simple Text='Click me' UrlSuffix={value: UrlSuffix} />

                <!-- server rendering, no params -->
                <dot:RouteLink RenderSettings.Mode=Server RouteName=Simple Text='Click me' />
                <!-- server rendering, no params, query and suffix -->
                <dot:RouteLink RenderSettings.Mode=Server RouteName=Simple Text='Click me' Query-Binding={value: Integer} Query-Constant='c/y' UrlSuffix='#mySuffix' />
                <!-- server rendering, no params, text binding -->
                <dot:RouteLink RenderSettings.Mode=Server RouteName=Simple Text={value: Label} />
                <!-- server rendering, dynamic suffix -->
                <dot:RouteLink RenderSettings.Mode=Server RouteName=Simple Text='Click me' UrlSuffix={value: UrlSuffix} />

                <!-- client rendering, static params -->
                <dot:RouteLink RenderSettings.Mode=Client RouteName={resource: 'WithParams'} Param-A=A Param-B={resource: 1} Text='Click me' />
                <!-- client rendering, static params, query and suffix -->
                <dot:RouteLink RenderSettings.Mode=Client RouteName=WithParams Param-A=A Param-B={resource: 1} Text='Click me' Query-Binding={value: Integer} Query-Constant='c/y' UrlSuffix='#mySuffix' />
                <!-- client rendering, dynamic params, query and suffix -->
                <dot:RouteLink RenderSettings.Mode=Client RouteName=WithParams Param-A={value: Label} Param-B={value: Integer} Text='Click me' Query-Binding={value: Integer} Query-Constant='c/y' UrlSuffix={value: UrlSuffix} />
                <!-- client rendering, static params, text binding -->
                <dot:RouteLink RenderSettings.Mode=Client RouteName=WithParams Param-A=A Param-B=1 Text={value: Label} />
                <!-- server rendering, static params -->
                <dot:RouteLink RenderSettings.Mode=Server RouteName={resource: 'WithParams'} Param-A=A Param-B={resource: 1} Text='Click me' />
                <!-- server rendering, static params, query and suffix -->
                <dot:RouteLink RenderSettings.Mode=Server RouteName=WithParams Param-a=A Param-B={resource: 1} Text='Click me' Query-Binding={value: Integer} Query-Constant='c/y' UrlSuffix='#mySuffix' />
                <!-- server rendering, dynamic params, query and suffix -->
                <dot:RouteLink RenderSettings.Mode=Server RouteName=WithParams Param-A={value: Label} Param-b={value: Integer} Text='Click me' Query-Binding={value: Integer} Query-Constant='c/y' UrlSuffix={value: UrlSuffix} />
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
                    {{value: NullableString}}
                </span>
                <!-- literal syntax, server rendering -->
                <span RenderSettings.Mode=Server>
                    {{value: Integer}}
                    {{value: Float}}
                    {{value: DateTime}}
                    {{value: NullableString}}
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
        public async Task TextBox()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- basic textbox, no formatting -->
                <span>
                    <dot:TextBox Text={value: Label} />
                    <dot:TextBox Text={value: Integer} />
                    <dot:TextBox Text={value: Float} />
                    <dot:TextBox Text={value: DateTime} />
                </span>
                <!-- disabled in different ways -->
                <span>
                    <dot:TextBox Text={value: Label} Enabled=false />
                    <dot:TextBox Text={value: Label} Enabled={value: false} />
                    <dot:TextBox Text={value: Label} Enabled={value: _page.EvaluatingOnServer} />
                    <dot:TextBox Text={value: Label} Enabled={resource: false} />
                </span>
                <!-- select all on focus -->
                <span>
                    <dot:TextBox Text={value: Label} SelectAllOnFocus />
                    <dot:TextBox Text={value: Label} SelectAllOnFocus=tRUE />
                    <dot:TextBox Text={value: Label} SelectAllOnFocus={value: Integer > 20} />
                    <!-- this should not be set -->
                    <dot:TextBox Text={value: Label} SelectAllOnFocus={resource: false} />
                </span>
                <!-- textbox types -->
                <span>
                    <dot:TextBox Text={value: DateTime} type=date />
                    <dot:TextBox Text={value: DateTime} type=DateTimeLocal />
                    <dot:TextBox Text={value: DateTime} type=month />
                    <dot:TextBox Text={value: DateTime} type=Time />
                    <dot:TextBox Text={value: Integer} type=number />
                    <dot:TextBox Text={value: Float} type=number step=0.1 />
                    <dot:TextBox Text={value: Label} type=password />
                    <dot:TextBox Text={value: Label} type=email />
                </span>
                <!-- multiline - textarea -->
                <dot:TextBox type=MultiLine Text={value: Label} />
                <!-- multiline - textarea with static text -->
                <dot:TextBox type=MultiLine Text={resource: Label} />
                <!-- UpdateTextOnInput -->
                <dot:TextBox Text={value: Label} UpdateTextOnInput />
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
        public async Task NamedCommand()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                async static command with arg
                <dot:NamedCommand Name=1
                                  Command={staticCommand: (int s) => _js.InvokeAsync<int>('myCmd', s)} />
                async static command with Invoke&lt;Task&gt;
                <dot:NamedCommand Name=1
                                  Command={staticCommand: (int s) => _js.Invoke<System.Threading.Tasks.Task<int>>('myCmd', s)} />
                Command with arg
                <dot:NamedCommand Name=2
                                  Command={command: (string x) => x + '0'} />
                sync static command with argument
                <dot:NamedCommand Name=3
                                  Command={staticCommand: (int s) => Integer = s} />
                Just command
                <dot:NamedCommand Name=4
                                  Command={command: 0} />
                async static command
                <dot:NamedCommand Name=5
                                  Command={staticCommand: _js.Invoke<System.Threading.Tasks.Task<int>>('myCmd')} />
                sync static command
                <dot:NamedCommand Name=6
                                  Command={staticCommand: 0} />

            ", directives: "@js dotvvm.internal");
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task JsComponent()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <js:Bazmek
                                 troll={resource: 1}
                                 scmd={staticCommand: (int s) => _js.Invoke<System.Threading.Tasks.Task<int>>('myCmd', s)}>

                    <MyTemplate>
                        <h1> Ahoj lidi </h1>
                    </MyTemplate>
                </js:Bazmek>

                <js:Bazmek troll={resource: 1} />
                <js:Bazmek lol={value: Integer} />
                <js:Bazmek cmd={command: (string x) => x + '0'} />
                <js:Bazmek scmd={staticCommand: (int x) => Integer = x} />
            ", directives: "@js dotvvm.internal");
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task Decorator()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <dot:Decorator class=c1>
                    <div /> 
                </dot:Decorator>
                <dot:Decorator class=c2>
                    <%-- comment --%>
                    <div /> 
                </dot:Decorator>
                <dot:Decorator class=c3>
                    <!-- comment -->
                    <div /> 
                </dot:Decorator>
            ", directives: "@js dotvvm.internal");
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task HtmlLiteral()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- Static text -->
                <dot:HtmlLiteral class=c1 Html='some text' />
                <!-- Resource binding -->
                <dot:HtmlLiteral class=c1 Html={resource: Label} />
                <!-- Value binding in <span> -->
                <dot:HtmlLiteral class=c1 Html={value: Label} WrapperTagName=span />
                <!-- Static text, no wrapper -->
                <dot:HtmlLiteral Html='some text' RenderWrapperTag=false />
            ");
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [DataTestMethod]
        [DataRow("Client")]
        [DataRow("Server")]
        public async Task FileUpload(string renderMode)
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @$"
                <!-- output should be the same in Client/Server rendering -->
                <dot:FileUpload UploadedFiles={{value: Files}}
                    RenderSettings.Mode={renderMode} />
            ", fileName: $"FileUpload-{renderMode}");
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task Auth()
        {
            var testUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { 
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.Role, "tester"), 
                new Claim("custom-claim", "trolllololololo"), 
                new Claim(ClaimTypes.NameIdentifier, "test.user")
            }, "Basic"));

            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
            
                IsAuthenticated: {{resource: _user.Identity.IsAuthenticated}}

                <div IncludeInPage={resource: _user.IsInRole('admin')}> Only for admins </div>

                <div IncludeInPage={resource: _user.IsInRole('premium')}> Only for premium users </div>


                <dot:AuthenticatedView> Only for authenticated users </dot:AuthenticatedView>

                <dot:RoleView Roles='a,b,c'><IsNotMemberTemplate> not member </IsNotMemberTemplate> <IsMemberTemplate> Only for some random roles </IsMemberTemplate> </dot:RoleView> 
            ", user: testUser);
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task MultiSelect()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- hardcoded -->
                <dot:MultiSelect SelectedValues={value: IntArray}>
                    <dot:SelectorItem Text='A' Value=0 />
                    <dot:SelectorItem Text='X Y Z' Value=1 />
                </dot:MultiSelect>

                <!-- bound -->
                <dot:MultiSelect SelectedValues={value: IntArray}
                                 DataSource={value: Customers}
                                 ItemTextBinding={value: Name}
                                 ItemValueBinding={value: Id} />
            ");
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task CurlyBraceEscaping()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                CDATA: <![CDATA[ <span>{{value: Label}}</span> ]]>

                <br>

                Escape sequence: &#123;&#123;value: Label&#125;&#125;

                <br>

                Lazy escaping: {&#123;value: Label}}
            ");
            check.CheckString(r.OutputString, fileExtension: "raw.html");
            check.CheckString(r.FormattedHtml, fileExtension: "reparsed.html");
        }


        [TestMethod]
        public async Task ClickEvents()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), """
                <dot:Button Click={command: Integer = Integer + 1} onclick="alert('Runs before the command')">Classic Btn</dot:Button>
                <dot:LinkButton Click={command: Integer = Integer + 1} onclick="alert('Runs before the command')" Text='Link Btn' />
                <div Events.Click={staticCommand: 0}> </div> <%-- TODO: onclick="alert('Runs before the command')" --%>
                <div Events.DoubleClick={staticCommand: 0}> </div>
            """);

            check.CheckString(r.OutputString, fileExtension: "html");
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

            public string NullableString { get; } = null;

            public int[] IntArray { get; set; }

            public string UrlSuffix { get; set; } = "#something";

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

            public UploadedFilesCollection Files { get; set; } = new UploadedFilesCollection();

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
