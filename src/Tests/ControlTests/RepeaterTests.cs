using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class RepeaterTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
            _ = Repeater.RenderAsNamedTemplateProperty;
            config.Styles.Register<Repeater>().SetProperty(r => r.RenderAsNamedTemplate, false, StyleOverrideOptions.Ignore);
            config.Markup.AddCodeControls("cc", exampleControl: typeof(RepeaterWrapper));
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task IdGeneration()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- client rendering - explicit id -->
                <dot:Repeater DataSource={value: Items} id=client>
                    <div id=client-div>{{value: Number}}</div>
                </dot:Repeater>

                <!-- server rendering - explicit id -->
                <dot:Repeater DataSource={value: Items} id=server RenderSettings.Mode=Server>
                    <div id=server-div>{{value: Number}}</div>
                </dot:Repeater>

                <!-- client rendering - implicit id -->
                <dot:Repeater DataSource={value: Items}>
                    <div id=client-div>{{value: Number}}</div>
                </dot:Repeater>

                <!-- server rendering - implicit id -->
                <dot:Repeater DataSource={value: Items} RenderSettings.Mode=Server>
                    <div id=server-div>{{value: Number}}</div>
                </dot:Repeater>
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task IdGeneration_CreatedAtRuntime()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- client rendering - explicit id -->
                <cc:RepeaterWrapper DataSource={value: Items} id=client>
                    <div id=client-div>{{value: Number}}</div>
                </cc:RepeaterWrapper>

                <!-- server rendering - explicit id  -->
                <cc:RepeaterWrapper DataSource={value: Items} id=server RenderSettings.Mode=Server>
                    <div id=server-div>{{value: Number}}</div>
                </cc:RepeaterWrapper>

                <!-- client rendering - implicit id -->
                <cc:RepeaterWrapper DataSource={value: Items}>
                    <div id=client-div>{{value: Number}}</div>
                </cc:RepeaterWrapper>

                <!-- server rendering - implicit id  -->
                <cc:RepeaterWrapper DataSource={value: Items} RenderSettings.Mode=Server>
                    <div id=server-div>{{value: Number}}</div>
                </cc:RepeaterWrapper>
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

    }

    public class BasicTestViewModel
    {
        public TestItem[] Items { get; set; } = new []
        {
            new TestItem() { Number = 1 },
            new TestItem() { Number = 2 },
            new TestItem() { Number = 3 },
            new TestItem() { Number = 4 }
        };
    }

    public class TestItem
    {
        public int Number { get; set; }
    }

    [ControlMarkupOptions(AllowContent = false, DefaultContentProperty = "ItemTemplate")]
    public class RepeaterWrapper : CompositeControl
    {
        public static DotvvmControl GetContents(
            HtmlCapability htmlCapability,
            [ControlPropertyTypeDataContextChange("DataSource"), CollectionElementDataContextChange(1)]
            ITemplate itemTemplate,
            IValueBinding<IEnumerable> dataSource
        )
        {
            return new Repeater()
                .SetCapability(htmlCapability)
                .SetProperty(r => r.RenderAsNamedTemplate, false)
                .SetProperty(r => r.DataSource, dataSource)
                .SetProperty(r => r.ItemTemplate, itemTemplate);
        }
    }
}
