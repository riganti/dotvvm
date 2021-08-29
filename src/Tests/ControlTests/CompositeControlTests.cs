using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class CompositeControlTests
    {
        ControlTestHelper cth = new ControlTestHelper(config: config => {
            config.Styles.Register<Repeater>().SetProperty(r => r.RenderAsNamedTemplate, false, StyleOverrideOptions.Ignore);
            config.Markup.AddCodeControls("cc", exampleControl: typeof(WrappedHtmlControl));
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
                                   />
                "
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

            public List<string> List { get; set; } = new List<string>();

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
            string wrapperTagName = "div"
        )
        {
            return new Repeater() {
                RenderAsNamedTemplate = false,
                WrapperTagName = wrapperTagName,
                ItemTemplate = new DelegateTemplate(_ =>
                    new Button { ButtonTagName = ButtonTagName.button }
                        .SetProperty("Click", itemClick)
                        .SetCapability(buttonHtml)
                        .SetCapability(buttonContent)
                )
            }
            .SetProperty(Repeater.DataSourceProperty, dataSource)
            .SetCapability(html);
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
}
