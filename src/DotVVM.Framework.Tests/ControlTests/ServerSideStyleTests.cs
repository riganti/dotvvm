using System;
using System.Globalization;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.Infrastructure;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.Styles;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class ServerSideStyleTests
    {
        OutputChecker check = new OutputChecker("testoutputs");

        ControlTestHelper createHelper(Action<DotvvmConfiguration> c)
        {
            return new ControlTestHelper(config: config => {
                c(config);
            });
        }

        [TestMethod]
        public async Task WrapperAndAppend()
        {
            var cth = createHelper(c => {
                c.Styles.Register("div", c => c.HasAncestorWithTag("no-divs"))
                    .ReplaceWith(new HtmlGenericControl("not-a-div") { Attributes = { ["data-explanation"] = "Obviously, this is a div but not really" }});
                c.Styles.RegisterAnyControl(c => c.HasTag("prepend-icon"))
                    .Prepend(new HtmlGenericControl("img") { Attributes = { ["href"] = "myicon.png" } });
                c.Styles.RegisterAnyControl(c => c.HasTag("add-icon"))
                    .Append(new HtmlGenericControl("img") { Attributes = { ["href"] = "myicon.png" } });
                c.Styles.RegisterAnyControl(c => c.HasTag("wrap-to-box"))
                    .WrapWith(new HtmlGenericControl("div") { Attributes = { ["class"] = "box" }});
                c.Styles.RegisterAnyControl(c => c.HasTag("wrap-to-panel"))
                    .WrapWith(new HtmlGenericControl("div") { Attributes = { ["class"] = "panel" }});
                var authView = new AuthenticatedView();
                authView.WrapperTagName = null;
                authView.properties.Set(AuthenticatedView.NotAuthenticatedTemplateProperty, RawLiteral.Create("You are not allowed to see this"));
                // even wrapping into a template should work
                c.Styles.RegisterAnyControl(c => c.HasTag("autoauth"))
                    .WrapWith(authView);
            });

            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
               <!-- <span Styles.Tag=add-icon,wrap-to-box> wrapped in box, added icon </span>
                <span Styles.Tag=prepend-icon,wrap-to-box> wrapped in box, added icon before control </span>
                <span Styles.Tag=prepend-icon,add-icon,wrap-to-box> wrapped in box, added icon at both ends </span>

                <span Styles.Tag=wrap-to-panel,wrap-to-box> wrapped in box and panel</span> -->

                <nav Styles.Tag=no-divs>
                    <div Styles.Tag=wrap-to-panel,wrap-to-box> no divs allowed here </div>
                </nav>
<!--
                Authenticated stuff, nothing should be displayed
                <span Styles.Tag=autoauth,wrap-to-box> a </span> -->
            ");
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }


        [TestMethod]
        public async Task ContentInTemplates()
        {
            var cth = createHelper(c => {

                // Repeater does not allow children but has a default content property
                c.Styles.Register<Repeater>()
                    .SetContent(
                        new Literal(),
                        l => l.SetPropertyBinding(l => l.Text, "_this"),
                        StyleOverrideOptions.Ignore
                    );
                c.Styles.Register<Repeater>(c => c.HasTag("separate"))
                    .SetContent(
                        new Literal("|") { RenderSpanElement = true },
                        l => l.SetPropertyBinding(l => l.IncludeInPage, "!_collection.IsFirst"),
                        StyleOverrideOptions.Prepend
                    );
                c.Styles.RegisterAnyControl(c => c.HasTag("add-something"))
                    .SetContent(
                        new HtmlGenericControl("div") {
                            Attributes = { ["class"] = "a" },
                            Children = {
                                RawLiteral.Create("Something")
                            }
                        },
                        options: StyleOverrideOptions.Overwrite
                    );
            });

            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                This repeater will not get the default content.
                <dot:Repeater DataSource={value: Collection}>
                    Content 
                </dot:Repeater>

                But these two will
                <dot:Repeater DataSource={value: Collection}>
                    <SeparatorTemplate>Separator</SeparatorTemplate>
                </dot:Repeater>
                <dot:Repeater DataSource={value: Collection}>
                </dot:Repeater>

                This one will automatically get a separator
                <dot:Repeater DataSource={value: Collection} Styles.Tag=separate>
                    <span InnerText={value: _this.Length} />
                </dot:Repeater>

                This will have some generated content
                <div Styles.Tag=add-something>
                    something that will be overwritten
                </div>
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
            public string Label { get; } = "My Label";

            public List<string> Collection { get; } = new List<string>();
        }
    }
}
