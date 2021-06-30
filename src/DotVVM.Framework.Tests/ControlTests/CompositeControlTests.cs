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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class CompositeControlTests
    {
        ControlTestHelper cth = new ControlTestHelper(config: config => {
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
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }


        // [TestMethod]
        // public async Task CommandBinding()
        // {
        //     var r = await cth.RunPage(typeof(BasicTestViewModel), @"
        //     <dot:Button Click={command: Integer = Integer + 1} />
        //     <dot:Button Click={command: Integer = Integer - 1} Enabled={value: Integer > 10000000} />
        //     ");

        //     Assert.AreEqual(10000000, (int)r.ViewModel.@int);
        //     await r.RunCommand("Integer = Integer + 1");
        //     Assert.AreEqual(10000001, (int)r.ViewModel.@int);
        //     await r.RunCommand("Integer = Integer - 1");
        //     Assert.AreEqual(10000000, (int)r.ViewModel.@int);
        //     // invoking command on disabled button should fail
        //     var exception = await Assert.ThrowsExceptionAsync<Framework.Runtime.Commands.InvalidCommandInvocationException>(() =>
        //         r.RunCommand("Integer = Integer - 1")
        //     );
        //     Console.WriteLine(exception);
        // }

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

}
