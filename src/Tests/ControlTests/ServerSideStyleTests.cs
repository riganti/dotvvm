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
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class ServerSideStyleTests
    {
        OutputChecker check = new OutputChecker("testoutputs");

        ControlTestHelper createHelper(Action<DotvvmConfiguration> c)
        {
            return new ControlTestHelper(config: config => {
                _ = Repeater.RenderAsNamedTemplateProperty;
                config.Styles.Register<Repeater>().SetProperty(r => r.RenderAsNamedTemplate, false, StyleOverrideOptions.Ignore);
                c(config);
            });
        }

        [TestMethod]
        public async Task WrapperAndAppend()
        {
            var cth = createHelper(c => {
                c.Styles.Register("div", c => c.HasAncestorWithTag("no-divs"))
                    .ReplaceWith(new HtmlGenericControl("not-a-div").SetAttribute("data-explanation", "Obviously, this is a div but not really"));
                c.Styles.RegisterAnyControl(c => c.HasTag("prepend-icon"))
                    .Prepend(new HtmlGenericControl("img").SetAttribute("href", "myicon.png"));
                c.Styles.RegisterAnyControl(c => c.HasTag("add-icon"))
                    .Append(new HtmlGenericControl("img").SetAttribute("href", "myicon.png"));
                c.Styles.RegisterAnyControl(c => c.HasTag("wrap-to-box"))
                    .WrapWith(new HtmlGenericControl("div").SetAttribute("class", "box"));
                c.Styles.RegisterAnyControl(c => c.HasTag("wrap-to-panel"))
                    .WrapWith(new HtmlGenericControl("div").SetAttribute("class", "panel"));
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
                        new HtmlGenericControl("div")
                            .SetAttribute("class", "a")
                            .AppendChildren(RawLiteral.Create("Something")),
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

        [TestMethod]
        public async Task PostbackHandlers()
        {
            var cth = createHelper(c => {

                // Repeater does not allow children but has a default content property
                c.Styles.RegisterAnyControl(c => c.HasTag("a") || c.HasTag("b"))
                    .AddPostbackHandler(
                        new ConfirmPostBackHandler("a") { EventName = "Click" }
                    );
                c.Styles.RegisterAnyControl(c => c.HasTag("b"))
                    .AddPostbackHandler(
                        c => new ConfirmPostBackHandler(c.GetHtmlAttribute("data-msg") ?? "default message")
                    )
                    .AddPostbackHandler(
                        c => new ConfirmPostBackHandler("Handler for some other property") { EventName = "Bazmek" }
                    );
                c.Styles.RegisterAnyControl(c => c.HasTag("c"))
                    .SetDotvvmProperty(
                        PostBack.HandlersProperty,
                        c => new ConfirmPostBackHandler("C"),
                        StyleOverrideOptions.Overwrite
                    );
            });

            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                One handler
                <dot:Button Styles.Tag=a Click={command: 0} />
                Two handlers
                <dot:Button Styles.Tag=b Click={command: 0} data-msg=ahoj />
                Two handlers, value binding message
                <dot:Button Styles.Tag=b Click={command: 0} data-msg={value: Label} />
                Two handlers, resource binding message
                <dot:Button Styles.Tag=b Click={command: 0} data-msg={resource: Label} />
                Two handlers, default message
                <dot:Button Styles.Tag=b Click={command: 0} />
                One handler, because override
                <dot:Button Styles.Tag='a,b,c' Click={command: 0} data-msg=ahoj />
            ");
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task ChildrenMatching()
        {
            var cth = createHelper(c => {

                c.Styles.Register<HtmlGenericControl>(c => c.AllChildren<ConfirmPostBackHandler>().Any())
                    .AppendAttribute("data-confirm-handler", c => c.ControlProperty<ConfirmPostBackHandler>(PostBack.HandlersProperty).First().Property(p => p.Message));
                c.Styles.Register<HtmlGenericControl>(c => c.AllDescendants().Any(d => d.HasClass("magic-class")))
                    .AddClass("magic-class");
                c.Styles.Register<HtmlGenericControl>(c => c.Descendants<TextBox>(allowDefaultContentProperty: false).Any())
                    .AddClass("beware-textbox");
                c.Styles.Register<HtmlGenericControl>(c => c.Descendants().Any(c => c.ControlType() == typeof(RawLiteral)))
                    .AddClass("never-happens");
                c.Styles.Register<HtmlGenericControl>(c => c.Descendants(includeRawLiterals: true).Any(c => c.IsRawLiteral(out _, out var text) && text.Contains("bazmek")))
                    .AddClass("contains-bazmek");
            });

            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                magic-class and contains-bazmek should be propagated through the hierarchy
                beware-textbox does not propagate through the Repeater
                <div>
                    <dot:Repeater DataSource={value: Collection}>
                        <div class=magic-class>
                            <span> bazmek <dot:TextBox Text={value: _this} /> </span>
                        </div>
                    </dot:Repeater>
                </div>
                magic-class should be propagated, but contains-bazmek shouldn't
                <div>
                    <dot:Repeater DataSource={value: Collection}>
                        <SeparatorTemplate>
                        <div class=magic-class>
                            <span> bazmek </span>
                        </div>
                        </SeparatorTemplate>
                        something
                    </dot:Repeater>
                    
                </div>
            ");
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task CapabilitySetting()
        {
            var cth = createHelper(c => {
                var dataAttributeName = "data-test2";
                c.Styles.Register<HtmlGenericControl>(c =>
                    c.PropertyValue(x => x.GetCapability<HtmlCapability>().Attributes["class"]) as string == "test")
                    .SetProperty(x => x.Attributes["data-test1"], "true")
                    .SetProperty(x => x.HtmlCapability.Attributes[dataAttributeName], "also true");
                // test conversion to/from ValueOrBinding
                // this is not needed in this case, but might be useful for workarounding some type-parameter magic rough edges
                c.Styles.Register<HtmlGenericControl>(c =>
                    c.PropertyValue(x => new ValueOrBinding<object>(x.Attributes["class"])) as string == "test2")
                    .SetProperty(x => (ValueOrBinding<object>)x.Attributes["data-test1"], "true")
                    .SetProperty(x => x.HtmlCapability.Attributes[dataAttributeName].ValueOrDefault, "also true");
            });

            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <div class=test />
                <div class=test2 />
            ");
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }


        [TestMethod]
        public async Task StyleBindingMapping()
        {
            var cth = createHelper(c => {
                c.Styles.Register<HtmlGenericControl>(c => c.HasHtmlAttribute("data-custom-class-attr"))
                    .AddClass(c => c.GetHtmlAttribute("data-custom-class-attr").Value.Select(c => "class123-" + c));
                c.Styles.Register<CheckBox>(c => !c.HasProperty(c => c.Checked) && c.HasDataContext<BasicTestViewModel>())
                    .SetPropertyBinding(c => c.Checked, "_this.Boolean");
                c.Styles.Register<CheckBox>()
                    .AddClass(c => c.Property(c => c.Checked).Select(c => c == true ? "checkbox-checked" : ""));
                c.Styles.Register<TextBox>()
                    .SetProperty(c => c.Visible, c => c.Property(c => c.Visible).And(c.Property(c => c.Text).Select(t => t != "hidden")));
                c.Styles.Register<HtmlGenericControl>(c => c.HasProperty(c => c.Visible))
                    .SetPropertyGroupMember("Class-", "hide", c => c.Property(c => c.Visible).Negate())
                    .SetProperty(c => c.Visible, true);
            });

            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                static value
                <div class=a data-custom-class-attr=x></div>
                resource binding
                <div class=a data-custom-class-attr={resource: SomeClass}></div>
                value binding
                <div class=a data-custom-class-attr={value: SomeClass}></div>
                checkbox with checkbox-checked class
                <dot:CheckBox Checked={value: Integer > 10} />
                <dot:CheckBox />
                
                a div with visible class 
                <div Visible={value: Boolean} />

                <dot:TextBox Text={value: Label} />
            ");
            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task MarkupControlCreatedFromStyles()
        {
            var cth = createHelper(c => {
                c.Markup.AddMarkupControl("cc", "CustomControlWithSomeProperty", "CustomControlWithSomeProperty.dotcontrol");
                c.Markup.AddMarkupControl("cc", "CustomBasicControl", "CustomBasicControl.dotcontrol");


                c.Styles.Register<HtmlGenericControl>(c => c.HasTag("a"))
                    .AppendContent(new MarkupControlContainer("cc:CustomBasicControl"));
                c.Styles.Register<HtmlGenericControl>(c => c.HasTag("b"))
                    .AppendContent(new MarkupControlContainer("CustomBasicControl2.dotcontrol"));
                c.Styles.Register<HtmlGenericControl>(c => c.HasTag("c"))
                    .AppendContent(new MarkupControlContainer<CustomControlWithSomeProperty>("cc:CustomControlWithSomeProperty", c => c.SomeProperty = "ahoj"));
                c.Styles.Register<HtmlGenericControl>(c => c.HasTag("d"))
                    .AppendContent(new MarkupControlContainer("cc:CustomControlWithSomeProperty"), c => {
                        c.SetDotvvmProperty(CustomControlWithSomeProperty.SomePropertyProperty, "test");
                    });
            });

            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <dot:Placeholder DataContext={value: Integer}>
                    Markup control referenced as tag
                    <div Styles.tag=a />
                    Markup control referenced as filename
                    <div Styles.tag=b />
                    Markup control with property
                    <div Styles.tag=c />
                    Markup control with property, but different
                    <div Styles.tag=d />
                </dot:Placeholder>
                ",
                markupFiles: new Dictionary<string, string> {
                    ["CustomControlWithSomeProperty.dotcontrol"] = @"
                        @viewModel int
                        @baseType DotVVM.Framework.Tests.ControlTests.CustomControlWithSomeProperty
                        @wrapperTag div
                        {{value: _this + _control.SomeProperty.Length}}",
                    ["CustomBasicControl.dotcontrol"] = @"
                        @viewModel int
                        @noWrapperTag
                        {{value: _this}}",
                    ["CustomBasicControl2.dotcontrol"] = @"
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
            public string SomeClass { get; } = "some-class";
            public bool Boolean { get; set; }

            public List<string> Collection { get; } = new List<string>();
        }
    }

    public class CustomControlWithSomeProperty : DotvvmMarkupControl
    {
        public string SomeProperty
        {
            get { return (string)GetValue(SomePropertyProperty); }
            set { SetValue(SomePropertyProperty, value); }
        }
        public static readonly DotvvmProperty SomePropertyProperty =
            DotvvmProperty.Register<string, CustomControlWithSomeProperty>("SomeProperty");
    }
}
