using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class CompositeControlTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
            _ = Repeater.RenderAsNamedTemplateProperty;
            config.Styles.Register<Repeater>().SetProperty(r => r.RenderAsNamedTemplate, false, StyleOverrideOptions.Ignore);
            config.Markup.AddCodeControls("cc", exampleControl: typeof(WrappedHtmlControl));
            config.Markup.AddMarkupControl("cc", "CustomControlWithSomeProperty", "x/CustomControlWithSomeProperty.dotcontrol");
            config.Markup.AddMarkupControl("cc", "CustomBasicControl", "x/CustomBasicControl.dotcontrol");

        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task BasicWrappedHtmlControl()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- no params -->
                <cc:WrappedHtmlControl TagName=div />
                <!-- static values -->
                <cc:WrappedHtmlControl TagName=div data-testattr data-boolattr={resource: !AfterPreRender} data-anotherattr=123 class-x={resource: AfterPreRender} />
                <!-- value bindings -->
                <cc:WrappedHtmlControl TagName=div data-attr={value: Integer} class-x={value: Integer < 0} style-width={value: (Float * 100) + 'px'} />
                <!-- class value binding (compile-time attribute merging should work) -->
                <cc:WrappedHtmlControl2 class={resource: Integer > 10 ? 'big' : 'small'} class={resource: Integer % 2 == 0 ? 'even' : 'odd'} />
                <!-- extended Visible -->
                <cc:WrappedHtmlControl2 Visible={value: Integer > 10}/>

                <!-- Text Property -->
                <cc:WrappedHtmlControl TagName=div Text='Static Text' />
                <cc:WrappedHtmlControl TagName=div Text={value: Label} />
                <!-- Content -->
                <cc:WrappedHtmlControl TagName=div> <!-- empty content --> </cc:WrappedHtmlControl>
                <cc:WrappedHtmlControl TagName=div> Something here </cc:WrappedHtmlControl>

                <cc:WithPrivateGetContents TagName=article />
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task WrappedRepeaterControl()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- simple list -->
                <cc:RepeatedButton DataSource={value: List}
                                   WrapperTagName=p
                                   Text={value: _parent.Label + _this}
                                   ItemClick={command: _parent.Integer = _index}
                                   class=css-class-from-markup
                                   button:class=the-only-class-for-button-element
                                   />
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task BindingMapping()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <cc:BindingMappingControl Str={value: Label} IntBinding={value: Integer}/>
                <cc:BindingMappingControl Str=TtTt IntBinding={value: 0}/>
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        public async Task CommandDataContextChange()
        {
            // RepeatedButton2 creates button in repeater, but also 
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- command -->
                <cc:RepeatedButton2 DataSource={value: List}
                                    ItemClick={command: Integer = 15} />
                <!-- staticCommand -->
                <cc:RepeatedButton2 DataSource={value: List}
                                    ItemClick={staticCommand: Integer = 12} />
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");

            Assert.AreEqual(10000000, (int)r.ViewModel.@int);
            await r.RunCommand("Integer = 15", "list-item2".Equals);
            Assert.AreEqual(15, (int)r.ViewModel.@int);
        }

        [TestMethod]
        public async Task MarkupControlCreatedFromCodeControl()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <dot:Placeholder DataContext={value: Integer}>
                    Markup control referenced as tag
                    <cc:CreatingMarkupControl TestCase=a />
                    Markup control referenced as filename
                    <cc:CreatingMarkupControl TestCase=b />
                    Markup control with property
                    <cc:CreatingMarkupControl TestCase=c />
                    Markup control with property, but different
                    <cc:CreatingMarkupControl TestCase=d />
                </dot:Placeholder>
                ",
                markupFiles: new Dictionary<string, string> {
                    ["x/CustomControlWithSomeProperty.dotcontrol"] = @"
                        @viewModel int
                        @baseType DotVVM.Framework.Tests.ControlTests.CustomControlWithSomeProperty
                        @wrapperTag div
                        {{value: _this + _control.SomeProperty.Length}}",
                    ["x/CustomBasicControl.dotcontrol"] = @"
                        @viewModel int
                        @noWrapperTag
                        {{value: _this}}",
                    ["x/CustomBasicControl2.dotcontrol"] = @"
                        @viewModel int
                        @noWrapperTag
                        {{value: _this + 1}}",
                }
            );

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
            public bool AfterPreRender { get; set; } = false;

            public List<string> List { get; set; } = new List<string> { "list-item1", "list-item2" };

            public override Task PreRender()
            {
                AfterPreRender = true;
                return base.PreRender();
            }
        }
    }

    public class WrappedHtmlControl: CompositeControl
    {
        public static DotvvmControl GetContents(
            string tagName,
            HtmlCapability html,
            TextOrContentCapability content
        )
        {
            return new HtmlGenericControl(tagName, content).SetCapability(html);
        }
    }

    public class WrappedHtmlControl2: CompositeControl
    {
        public static DotvvmControl GetContents(
            HtmlCapability html,
            TextOrContentCapability content,
            string tagName = "div"
        )
        {
            return new HtmlGenericControl(tagName, content) {
            }.SetCapability(html)
             .SetProperty(c => c.CssClasses, "hidden-class", html.Visible.Negate());
        }
    }

    public class RepeatedButton: CompositeControl
    {
        public static DotvvmControl GetContents(
            IValueBinding<IEnumerable<string>> dataSource,

            [DotvvmControlCapability(prefix: "button:")]
            [ControlPropertyBindingDataContextChange("DataSource")]
            [CollectionElementDataContextChange(1)]
            HtmlCapability buttonHtml,

            HtmlCapability html,

            [ControlPropertyBindingDataContextChange("DataSource")]
            [CollectionElementDataContextChange(1)]
            TextOrContentCapability buttonContent,

            [ControlPropertyBindingDataContextChange("DataSource")]
            [CollectionElementDataContextChange(1)]
            ICommandBinding itemClick = null,
            string wrapperTagName = "div",
            string additionalCssClass = "my-repeated-button"
        )
        {
            return new Repeater() {
                RenderAsNamedTemplate = false,
                WrapperTagName = wrapperTagName,
                ItemTemplate = new DelegateTemplate(_ =>
                    new Button(buttonContent, itemClick)
                        .SetCapability(buttonHtml)
                )
            }
            .SetProperty(Repeater.DataSourceProperty, dataSource)
            .SetCapability(html)
            .AddCssClass(additionalCssClass);
        }
    }

    public class RepeatedButton2: CompositeControl
    {
        public static DotvvmControl GetContents(
            IValueBinding<IEnumerable<string>> dataSource,

            ICommandBinding itemClick = null
        )
        {
            // Places itemClick in two different data contexts
            var repeater = new Repeater() {
                RenderAsNamedTemplate = false,
                WrapperTagName = "div",
                ItemTemplate = new DelegateTemplate(_ =>
                    new Button("Item", itemClick)
                )
            }
            .SetProperty(Repeater.DataSourceProperty, dataSource);
            return new HtmlGenericControl("div")
                .AppendChildren(
                    repeater,
                    new Button("Last Item", itemClick)
                );
        }
    }


    public class WithPrivateGetContents: CompositeControl
    {
        public string TagName
        {
            get { return (string)GetValue(TagNameProperty); }
            set { SetValue(TagNameProperty, value); }
        }
        public static readonly DotvvmProperty TagNameProperty =
            DotvvmProperty.Register<string, WithPrivateGetContents>(nameof(TagName));
        DotvvmControl GetContents()
        {
            return new HtmlGenericControl(TagName);
        }
    }

    public class BindingMappingControl: CompositeControl
    {
        public static DotvvmControl GetContents(
            ValueOrBinding<string> str,
            IValueBinding<int> intBinding
        )
        {
            return new HtmlGenericControl("div") {
                Children = {
                    RawLiteral.Create("\ntext length: "),
                    new Literal(str.Select(s => s.Length), renderSpan: true),
                    RawLiteral.Create("\ntext to lower"),
                    new Literal(str.Select(s => s.ToLowerInvariant()), renderSpan: true),
                    RawLiteral.Create("\nint times 2"),
                    new Literal(intBinding.Select(s => s * 2), renderSpan: true)
                        .SetProperty(c => c.Visible, intBinding.Select(s => s > 10)),
                }
            };
        }
    }
    public class CreatingMarkupControl: CompositeControl
    {
        public static DotvvmControl GetContents(
            string testCase
        )
        {
            return testCase switch {
                "a" => new MarkupControlContainer("cc:CustomBasicControl"),
                "b" => new MarkupControlContainer("x/CustomBasicControl2.dotcontrol"),
                "c" => new MarkupControlContainer<CustomControlWithSomeProperty>("cc:CustomControlWithSomeProperty", c => c.SomeProperty = "ahoj"),
                "d" => new MarkupControlContainer("cc:CustomControlWithSomeProperty", c => {
                        c.SetValue(CustomControlWithSomeProperty.SomePropertyProperty, "test");
                    })
            };
        }
    }
}
