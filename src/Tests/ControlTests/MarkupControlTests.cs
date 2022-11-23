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
using DotVVM.Framework.Testing;
using DotVVM.Framework.Compilation.Styles;
using AngleSharp;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class MarkupControlTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
            _ = Repeater.RenderAsNamedTemplateProperty;
            config.Resources.RegisterScriptModuleUrl("somemodule", "http://localhost:99999/somemodule.js", null);
            config.Markup.AddMarkupControl("cc", "CustomControl", "CustomControl.dotcontrol");
            config.Markup.AddMarkupControl("cc", "CustomControlWithCommand", "CustomControlWithCommand.dotcontrol");
            config.Markup.AddMarkupControl("cc", "CustomControlWithProperty", "CustomControlWithProperty.dotcontrol");
            config.Markup.AddMarkupControl("cc", "CustomControlWithInvalidVM", "CustomControlWithInvalidVM.dotcontrol");
            config.Markup.AddMarkupControl("cc", "CustomControlWithInternalProperty", "CustomControlWithInternalProperty.dotcontrol");
            config.Markup.AddMarkupControl("cc", "CustomControlWithResourceProperty", "CustomControlWithResourceProperty.dotcontrol");
            config.Markup.AddMarkupControl("cc", "CustomControlWithJsInvoke", "CustomControlWithJsInvoke.dotcontrol");
            config.Styles.Register<Repeater>().SetProperty(r => r.RenderAsNamedTemplate, false, StyleOverrideOptions.Ignore);
        }, services: s => {
            s.Services.AddSingleton<TestService>();
        });
        OutputChecker check = new OutputChecker(
            "testoutputs");

        [TestMethod]
        public async Task MarkupControl_PropertyUsedManyTimes()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"

                <cc:CustomControl Another={value: Integer} Another2={value: Integer} />
                <cc:CustomControl Another=10 Another2=10 />
                ",
                directives: $"@service s = {typeof(TestService)}",
                markupFiles: new Dictionary<string, string> {
                    ["CustomControl.dotcontrol"] = @"
                        @viewModel object
                        @baseType DotVVM.Framework.Tests.ControlTests.CustomControlWithCommand
                        @wrapperTag div

                        {{value: _control.Another}}  {{resource: _control.Another2}} {{controlProperty: Another}}

                        <span class-x={controlProperty: Another > 10} />
                        "
                }
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task MarkupControl_PassingStaticCommand()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"

                <cc:CustomControlWithCommand DataContext={value: Integer} Click={staticCommand: s.Save(_parent.Integer)} Another={value: _this} />
                <dot:Repeater DataSource={value: Collection}>
                    <cc:CustomControlWithCommand Click={staticCommand: s.Save(_this)} Another={value: _root.Integer} />
                </dot:Repeater>
                ",
                directives: $"@service s = {typeof(TestService)}",
                markupFiles: new Dictionary<string, string> {
                    ["CustomControlWithCommand.dotcontrol"] = @"
                        @viewModel int
                        @baseType DotVVM.Framework.Tests.ControlTests.CustomControlWithCommand
                        @wrapperTag div
                        <dot:Button Click={staticCommand: _control.Click()} Text={resource: $'Button with number = {_control.Another}'} />"
                }
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task MarkupControl_UpdateSource()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"

                <cc:CustomControlWithProperty P={value: Integer} />
                <dot:Repeater DataSource={value: Collection} class=r1>
                    <cc:CustomControlWithProperty P={value: _root.Collection[_index]} />
                </dot:Repeater>
                <dot:Repeater DataSource={value: IntArray} class=r2>
                    <cc:CustomControlWithProperty P={value: _root.IntArray[_index]} />
                </dot:Repeater>
                ",
                directives: $"@service s = {typeof(TestService)}",
                markupFiles: new Dictionary<string, string> {
                    ["CustomControlWithProperty.dotcontrol"] = @"
                        @viewModel object
                        @baseType DotVVM.Framework.Tests.ControlTests.CustomControlWithProperty

                        <dot:Button Click={command: _control.IncrementProperty()} />
                        <dot:Button Click={controlCommand: P = P - 10} />
                        <dot:Button class=static-command-button Click={staticCommand: _control.P = _control.P + 20} />
                        "
                }
            );

            Assert.AreEqual(10000000, (int)r.ViewModel.@int);
            await r.RunCommand("_control.IncrementProperty()", vm => vm is BasicTestViewModel);
            Assert.AreEqual(10000001, (int)r.ViewModel.@int);
            await r.RunCommand("P = P - 10", vm => vm is BasicTestViewModel);
            Assert.AreEqual(10000000 - 9, (int)r.ViewModel.@int);

            Assert.AreEqual(15, (int)r.ViewModel.IntArray[0]);
            await r.RunCommand("_control.IncrementProperty()", 15.Equals);
            Assert.AreEqual(16, (int)r.ViewModel.IntArray[0]);
            await r.RunCommand("P = P - 10", 15.Equals);
            Assert.AreEqual(6, (int)r.ViewModel.IntArray[0]);

            Assert.AreEqual(10, (int)r.ViewModel.Collection[0]);
            await r.RunCommand("_control.IncrementProperty()", 10.Equals);
            Assert.AreEqual(11, (int)r.ViewModel.Collection[0]);
            await r.RunCommand("P = P - 10", 10.Equals);
            Assert.AreEqual(1, (int)r.ViewModel.Collection[0]);

            Assert.AreEqual(-20, (int)r.ViewModel.Collection[1]);
            await r.RunCommand("_control.IncrementProperty()", (-20).Equals);
            Assert.AreEqual(-19, (int)r.ViewModel.Collection[1]);

            // check only the generated static command expressions
            check.CheckString(r.Html.QuerySelector(".static-command-button").ToHtml(), fileExtension: "html");
            check.CheckString(r.Html.QuerySelector(".r1 .static-command-button").ToHtml(), fileExtension: "html");
        }

        [TestMethod]
        public async Task MarkupControl_InternalProperty()
        {
            // test that passing a string -> string dictionary into a _js.Invoke works
            // the dictionary is either in a internal DotvvmProperty or declared as a property group

            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <cc:CustomControlWithInternalProperty PropGroup-const='AA' PropGroup-resource={resource: 'XX' + Integer} PropGroup-value={value: Integer} />
                ",
                markupFiles: new Dictionary<string, string> {
                    ["CustomControlWithInternalProperty.dotcontrol"] = @"
                        @viewModel object
                        @js somemodule
                        @baseType DotVVM.Framework.Tests.ControlTests.CustomControlWithInternalProperty

                        <dot:Button Click={staticCommand: _js.Invoke('xx', _control.Something)} />
                        <dot:Button Click={staticCommand: _js.Invoke('xx', _control.PropGroup)} />

                        {{value: _control.PropGroup.ContainsKey('test')}}
                    "
                }
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task ShouldFailReasonablyWhenControlHasInvalidViewModel()
        {
            var e = await Assert.ThrowsExceptionAsync<DotvvmCompilationException>(() => cth.RunPage(typeof(BasicTestViewModel), @"
                <cc:CustomControlWithInvalidVM />
                ",
                directives: $"@service s = {typeof(TestService)}",
                markupFiles: new Dictionary<string, string> {
                    ["CustomControlWithInvalidVM.dotcontrol"] = @"
                        @viewModel ClassThatDoesNotExist
                        @wrapperTag div

                        Test"
                }
            ));

            Assert.AreEqual("Could not resolve type 'ClassThatDoesNotExist'.", e.Message);
        }

        [TestMethod]
        public async Task PropertyDirectiveWithResourceBinding()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <cc:CustomControlWithResourceProperty ShowDescription={value: true} />
                <cc:CustomControlWithResourceProperty ShowDescription=true />
                <cc:CustomControlWithResourceProperty />
                ",
                directives: $"@service s = {typeof(TestService)}",
                markupFiles: new Dictionary<string, string> {
                    ["CustomControlWithResourceProperty.dotcontrol"] = @"
                        @viewModel object
                        @property bool ShowDescription
                        
                        <p IncludeInPage={resource: _control.ShowDescription}>test</p>

                    "
                }
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task MarkupControl_JsInvoke()
        {
            var p = await cth.RunPage(typeof(BasicTestViewModel), @"
                <cc:CustomControlWithJsInvoke SomeProperty=Test />
                ",
                directives: $"@service s = {typeof(TestService)}",
                markupFiles: new Dictionary<string, string> {
                    ["CustomControlWithJsInvoke.dotcontrol"] = @"
                        @viewModel object
                        @property string SomeProperty
                        @wrapperTag div
                        @js somemodule

                        <span InnerText={value: _js.Invoke<string>('jsfn', _control.SomeProperty)} />
                        "
                }
            );

            check.CheckString(p.FormattedHtml, fileExtension: "html");
        }


        public class BasicTestViewModel : DotvvmViewModelBase
        {
            [Bind(Name = "int")]
            public int Integer { get; set; } = 10000000;

            public int[] IntArray { get; set; } = new int[] { 15 };
            public int[] Collection { get; set; } = new int[] { 10, -20 };
        }
    }

    public class CustomControlWithCommand : DotvvmMarkupControl
    {
        public static readonly DotvvmProperty ClickProperty =
            DotvvmProperty.Register<Command, CustomControlWithCommand>("Click");

        public static readonly DotvvmProperty AnotherProperty =
            DotvvmProperty.Register<int, CustomControlWithCommand>("Another");

        public static readonly DotvvmProperty Another2Property =
            DotvvmProperty.Register<int, CustomControlWithCommand>("Another2");
    }

    public class CustomControlWithProperty : DotvvmMarkupControl
    {
        public static readonly DotvvmProperty PProperty =
            DotvvmProperty.Register<int, CustomControlWithProperty>("P");

        public void IncrementProperty()
        {
            this.SetValueToSource(PProperty, (int)GetValue(PProperty) + 1);
        }
    }

    public class CustomControlWithInternalProperty : DotvvmMarkupControl
    {
        internal static readonly DotvvmProperty SomethingProperty =
            DotvvmProperty.Register<Dictionary<string, string>, CustomControlWithInternalProperty>("Something");

        [PropertyGroup("PropGroup-", ValueType = typeof(bool))]
        public VirtualPropertyGroupDictionary<string> PropGroup => new(this, PropGroupGroupDescriptor);
        public static DotvvmPropertyGroup PropGroupGroupDescriptor =
            DotvvmPropertyGroup.Register<string, CustomControlWithInternalProperty>("PropGroup-", nameof(PropGroup));

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            this.SetValue(SomethingProperty, new Dictionary<string, string> { { "test", "test" }, {"x", "y"} });
        }
    }
}
