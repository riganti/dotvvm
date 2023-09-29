using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Linq;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.Tests.Runtime;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Hosting;

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
                <cc:WrappedHtmlControl TagName=div> <Content> Something here </Content> </cc:WrappedHtmlControl>

                <cc:WithPrivateGetContents TagName=article />
                "
            );

            CollectionAssert.AreEqual(new WrappedHtmlControl2[0], r.View.GetAllDescendants().OfType<WrappedHtmlControl2>().ToArray());

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task WrappedRepeaterControl()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- simple list -->
                <cc:RepeatedButton DataSource={value: List}
                                   WrapperTagName=p
                                   ID=test-id
                                   Text={value: _parent.Label + _this}
                                   ItemClick={command: _parent.Integer = _index}
                                   class=css-class-from-markup
                                   button:class=the-only-class-for-button-element
                                   button:ID=inner-button
                                   />
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task WrappedRepeaterControlWithGeneratedIds()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- SSR -->
                <cc:ControlWhichUsesUniqueIds DataSource={value: List} RenderSettings.Mode=Server />
                <!-- CSR -->
                <cc:ControlWhichUsesUniqueIds DataSource={value: List} RenderSettings.Mode=Client />
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task WrappedHierarchyRepeaterControlWithGeneratedIds()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- SSR -->
                <cc:HierarchyControlWhichUsesUniqueIds DataSource={value: Hierarchy} ItemChildrenBinding={value: Children} RenderSettings.Mode=Server />
                <!-- CSR -->
                <cc:HierarchyControlWhichUsesUniqueIds DataSource={value: Hierarchy} ItemChildrenBinding={value: Children} RenderSettings.Mode=Client />
                ",
                renderResources: true
            );

            foreach (var junkElement in r.Html.QuerySelectorAll("#__dot_viewmodel_root, script, head"))
            {
                junkElement.Remove();
            }

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


        [TestMethod]
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
        public async Task AutoclonedPostbackHandlers()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), """
                <!-- command -->
                <cc:RepeatedButton2 DataSource={value: List}
                                    ItemClick={command: Integer = 15}
                                    Precompile=false>
                    <PostBack.Handlers>
                        <dot:ConfirmPostBackHandler Message='Test not precompiled' />
                    </PostBack.Handlers>
                </cc:RepeatedButton2>
                <cc:RepeatedButton2 DataSource={value: List}
                                    ItemClick={command: Integer = 15}
                                    Precompile=true>
                    <PostBack.Handlers>
                        <dot:ConfirmPostBackHandler Message='Test precompiled' />
                    </PostBack.Handlers>
                </cc:RepeatedButton2>
                """
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");

            XAssert.Contains("Test not precompiled", r.OutputString);
            XAssert.Contains("Test precompiled", r.OutputString);
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
                    Markup control with property, but not prerendered, because it has a resource binding
                    <cc:CreatingMarkupControl TestCase={resource: _root.TestCase} />
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

            var markupControls = r.View.GetAllDescendants().OfType<DotvvmMarkupControl>().ToArray();
            Assert.AreEqual(5, markupControls.Length);
            var markupControlIds = markupControls.Select(c => c.GetDotvvmUniqueId().GetValue()).ToArray();
            Assert.AreEqual(markupControlIds.Length, markupControlIds.Distinct().Count(), $"Markup control IDs are not unique: {string.Join(", ", markupControlIds)}");

        }
        [TestMethod]
        public async Task BindingMappingWithEnum()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <div class-test-class={value: Status == 'Failed'} />
                <!-- value binding -->
                <cc:TestStatusIcon TestStatus={value: Status} />
                <!-- resource binding (should be false) -->
                <cc:TestStatusIcon TestStatus={resource: Status} />
                <!-- hardcoded value (should be true) -->
                <cc:TestStatusIcon TestStatus='Failed' />
                ");

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ControlWithMultipleEnumClasses(bool precompile)
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), $$"""
                <dot:Placeholder RenderSettings.Mode=Server>
                    <!-- static value -->
                    <cc:ControlWithMultipleEnumClasses Precompile={{precompile}} Type1=A Type2=D />
                    <!-- value binding + resource binding -->
                    <cc:ControlWithMultipleEnumClasses Precompile={{precompile}} Type1={value: EnumForCssClasses} Type2={resource: TrueBool ? 'D' : 'A'} />
                    <!-- value binding + static -->
                    <cc:ControlWithMultipleEnumClasses Precompile={{precompile}} Type1={value: EnumForCssClasses} Type2=D />
                    <!-- static + value binding -->
                    <cc:ControlWithMultipleEnumClasses Precompile={{precompile}} Type1=D Type2={value: EnumForCssClasses} />
                    <!-- both value bindings -->
                    <cc:ControlWithMultipleEnumClasses Precompile={{precompile}} Type1={value: TrueBool ? 'D' : 'A'} Type2={value: EnumForCssClasses} />
                </dot:Placeholder>
                """, fileName: $"ControlWithMultipleEnumClasses-{precompile}.dothtml"
            );

            check.CheckString(r.FormattedHtml, fileExtension: precompile ? "precompiled.html" : "html");
        }

        [TestMethod]
        public async Task ControlWithCollection()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                    <cc:ControlWithCollectionProperty> <Repeaters> <dot:Repeater DataSource={value: List}> xx </dot:Repeater> </Repeaters> </cc:ControlWithCollectionProperty>
                ");

            StringAssert.Contains(r.FormattedHtml, "1");
        }

        [TestMethod]
        public async Task ControlWithCollection_WrongType()
        {
            var e = await Assert.ThrowsExceptionAsync<DotvvmCompilationException>(() =>
                cth.RunPage(typeof(BasicTestViewModel), @"
                    <cc:ControlWithCollectionProperty> <Repeaters> <bazmek /> </Repeaters> </cc:ControlWithCollectionProperty>
                "));

            Assert.AreEqual("Control type DotVVM.Framework.Controls.HtmlGenericControl can't be used in collection of type DotVVM.Framework.Controls.Repeater.", e.Message);
        }

        [TestMethod]
        public async Task ClassBindingControl()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- Active=false -->
                <cc:ClassBindingControl Active=false />
                <!-- Active=true -->
                <cc:ClassBindingControl Active Width=100 />
                <!-- bindings -->
                <cc:ClassBindingControl Active={value: Integer > 100} Width={value: Integer} />
                "
            );

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }


        [TestMethod]
        public async Task MenuRepeater()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), @"
                <!-- Control which should render a list of links linking to a list of items bellow -->

                <!-- Server -->
                <cc:MenuRepeater DataSource={value: List} TitleBinding={value: _this} RenderSettings.Mode=Server>
                    {{value: _this}}
                </cc:MenuRepeater>

                <!-- Client -->
                <cc:MenuRepeater DataSource={value: List} TitleBinding={value: _this} RenderSettings.Mode=Client>
                    {{value: _this}}
                </cc:MenuRepeater>
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
            public string TestCase { get; set; } = "d";

            public TestStatusEnum Status { get; set; } = TestStatusEnum.StillRunning;

            public List<string> List { get; set; } = new List<string> { "list-item1", "list-item2" };

            public List<HierarchyVM> Hierarchy { get; set; } = new() {
                new("A", new()),
                new("B", new() { new("C", new() { new("D", new()) }) })
            };

            public bool TrueBool { get; set; } = true;

            public EnumForCssClasses EnumForCssClasses { get; set; } = EnumForCssClasses.C;

            public override Task PreRender()
            {
                AfterPreRender = true;
                return base.PreRender();
            }

            public record HierarchyVM(string Label, List<HierarchyVM> Children);
        }
    }

    [ControlMarkupOptions(Precompile = ControlPrecompilationMode.Never)]
    public class WrappedHtmlControl: CompositeControl
    {
        public static DotvvmControl GetContents(
            string tagName,
            HtmlCapability html,
            [MarkupOptions(MappingMode = MappingMode.InnerElement)]
            TextOrContentCapability content
        )
        {
            return new HtmlGenericControl(tagName, content).SetCapability(html);
        }
    }

    [ControlMarkupOptions(Precompile = ControlPrecompilationMode.InServerSideStyles)]
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

    [ControlMarkupOptions(Precompile = ControlPrecompilationMode.InServerSideStyles)]
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
                ItemTemplate = new CloneTemplate(
                    new Button(buttonContent, itemClick)
                        .SetCapability(buttonHtml)
                )
            }
            .SetProperty(Repeater.DataSourceProperty, dataSource)
            .SetCapability(html)
            .AddCssClass(additionalCssClass);
        }
    }

    [ControlMarkupOptions(Precompile = ControlPrecompilationMode.IfPossible)]
    public class RepeatedButton2: CompositeControl
    {
        public DotvvmControl GetContents(
            IValueBinding<IEnumerable<string>> dataSource,

            ICommandBinding itemClick = null,
            bool precompile = true
        )
        {
            if (!precompile && this.GetValue(Internal.RequestContextProperty) is null)
            {
                throw new SkipPrecompilationException();
            }
            // Places itemClick in two different data contexts
            var repeater = new Repeater() {
                RenderAsNamedTemplate = false,
                WrapperTagName = "div",
                ItemTemplate = new CloneTemplate(
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


    [ControlMarkupOptions(Precompile = ControlPrecompilationMode.Always)]
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

    [ControlMarkupOptions(Precompile = ControlPrecompilationMode.Always)]
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
                    }),
                _ => throw null
            };
        }
    }

    [ControlMarkupOptions(Precompile = ControlPrecompilationMode.IfPossible)]
    public class ControlWithMultipleEnumClasses: CompositeControl
    {
        public DotvvmControl GetContents(
            ValueOrBinding<EnumForCssClasses> type1,
            ValueOrBinding<EnumForCssClasses> type2,
            bool precompile = true
        )
        {
            if (!precompile && this.GetValue(Internal.RequestContextProperty) is null)
            {
                throw new SkipPrecompilationException();
            }
            return new HtmlGenericControl("div")
                .AddAttribute("class", type1)
                .AddAttribute("class", type2);
        }
    }

    public class ControlWhichUsesUniqueIds: CompositeControl
    {
        private readonly BindingCompilationService bindingService;

        public ControlWhichUsesUniqueIds(BindingCompilationService bindingService)
        {
            this.bindingService = bindingService;
        }
        public DotvvmControl GetContents(
            IValueBinding<IEnumerable<string>> dataSource
        )
        {
            return new Repeater() {
                WrapperTagName = "ul",
                RenderAsNamedTemplate = false // for testing
            }
                .SetProperty(Repeater.DataSourceProperty, dataSource)
                .SetProperty(Repeater.ItemTemplateProperty, new DelegateTemplate((_, container) => {
                    var li = new HtmlGenericControl("li");
                    container.Children.Add(li);
                    // this won't work unless the <li> is rooted
                    var id = li.GetDotvvmUniqueId();
                    li.AddAttribute("data-id", id);

                    li.SetProperty(c => c.InnerText, ValueBindingExpression.CreateThisBinding<string>(bindingService, li.GetDataContextType()));
                }));
        }
    }

    public class HierarchyControlWhichUsesUniqueIds: CompositeControl
    {
        private readonly BindingCompilationService bindingService;

        public HierarchyControlWhichUsesUniqueIds(BindingCompilationService bindingService)
        {
            this.bindingService = bindingService;
        }
        public DotvvmControl GetContents(
            IValueBinding<System.Collections.IEnumerable> dataSource,
            [CollectionElementDataContextChange(1)]
            [ControlPropertyBindingDataContextChange("DataSource")]
            IValueBinding<IEnumerable<object>> itemChildrenBinding
        )
        {
            return new HierarchyRepeater() {
                WrapperTagName = "ul"
            }
                .SetProperty(HierarchyRepeater.DataSourceProperty, dataSource)
                .SetProperty(HierarchyRepeater.ItemChildrenBindingProperty, itemChildrenBinding)
                .SetProperty(HierarchyRepeater.ItemTemplateProperty, new DelegateTemplate((_, container) => {
                    var li = new HtmlGenericControl("li");
                    container.Children.Add(li);
                    // this won't work unless the <li> is rooted
                    var id = li.GetDotvvmUniqueId();
                    li.AddAttribute("data-id", id);

                    li.SetProperty(c => c.InnerText, bindingService.Cache.CreateValueBinding<string>("_this.Label", li.GetDataContextType()));
                }));
        }
    }

    public class MenuRepeater: CompositeControl
    {
        public DotvvmControl GetContents(
            IValueBinding<System.Collections.IEnumerable> dataSource,
            [CollectionElementDataContextChange(1)]
            [ControlPropertyBindingDataContextChange("DataSource")]
            IValueBinding<string> titleBinding,
            [CollectionElementDataContextChange(1)]
            [ControlPropertyBindingDataContextChange("DataSource")]
            ITemplate contentTemplate,
            IDotvvmRequestContext cx)
        {
            var id = this.GetValueRaw(IDProperty) ?? this.GetValue<string>(Internal.UniqueIDProperty);
            var menuRepeater = new Repeater() {
                WrapperTagName = "ul",
                RenderAsNamedTemplate = false, // for testing
                DataSource = dataSource,
                ItemTemplate = new DelegateTemplate((_, container) => {
                    var li = new HtmlGenericControl("li");
                    container.Children.Add(li);
                    var fakeId = new PlaceHolder() { ID = "item" };
                    li.Children.Add(fakeId);
                    var id = fakeId.CreateClientId(prefix: new("#")); // TODO: how to solve URL encoding?
                    var anchor = new HtmlGenericControl("a")
                        .SetProperty(HtmlGenericControl.InnerTextProperty, titleBinding)
                        .AddAttribute("href", id);
                    li.Children.Add(anchor);
                })
            }.SetProperty(Internal.UniqueIDProperty, id);
            var contentRepeater = new Repeater() {
                WrapperTagName = "div",
                RenderAsNamedTemplate = false, // for testing
                DataSource = dataSource,
                ItemTemplate = new DelegateTemplate((_, container) => {
                    var div = new HtmlGenericControl("div") { ID = "item" };
                    container.Children.Add(div);
                    contentTemplate.BuildContent(cx, div);
                })
            }.SetProperty(Internal.UniqueIDProperty, id);

            return new PlaceHolder {
                Children = {
                    menuRepeater,
                    contentRepeater
                }
            };
        }

    }


    public enum EnumForCssClasses
    {
        [EnumMember(Value = "class-a")]
        A,
        [EnumMember(Value = "class-b")]
        B,
        [EnumMember(Value = "class-c")]
        C,
        [EnumMember(Value = "class-d")]
        D
    }

    public class ControlWithCollectionProperty: CompositeControl
    {
        public static DotvvmControl GetContents(
            IEnumerable<Repeater> repeaters
        )
        {
            return new Literal(repeaters.Count().ToString());
        }
    }
    public class ClassBindingControl: CompositeControl
    {
        public static DotvvmControl GetContents(
            ValueOrBinding<bool> active,
            ValueOrBinding<int>? width
        )
        {
            return new HtmlGenericControl("div")
                .AddCssClass("is-active", active)
                .AddCssStyle("width", width);
        }
    }

    public enum TestStatusEnum { Ok, StillRunning, Failed }

    public class TestStatusIcon : CompositeControl
    {
        public static DotvvmControl GetContents(ValueOrBinding<TestStatusEnum> testStatus)
        {
            var icon = new HtmlGenericControl("i");
            icon.AddCssClass("fas");
            icon.CssClasses.Add("fa-times", testStatus.Select(t => t == TestStatusEnum.Failed));
            return icon;
        }
    }
}
