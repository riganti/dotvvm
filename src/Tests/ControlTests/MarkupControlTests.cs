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

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class MarkupControlTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
            config.Markup.AddMarkupControl("cc", "CustomControl", "CustomControl.dotcontrol");
            config.Markup.AddMarkupControl("cc", "CustomControlWithCommand", "CustomControlWithCommand.dotcontrol");
            config.Markup.AddMarkupControl("cc", "CustomControlWithProperty", "CustomControlWithProperty.dotcontrol");
            config.Styles.Register<Repeater>().SetProperty(r => r.RenderAsNamedTemplate, false, StyleOverrideOptions.Ignore);
        }, services: s => {
            s.AddSingleton<TestService>();
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
}
