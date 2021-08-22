using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Binding.Properties;
using System.Linq;
using DotVVM.Framework.DependencyInjection;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Testing;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class DefaultViewCompilerTests
    {
        private IDotvvmRequestContext context;


        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_ElementWithAttributeProperty()
        {
            var markup = @"@viewModel System.Object, mscorlib
test <dot:Literal Text='test' />";
            var page = CompileMarkup(markup);

            Assert.IsInstanceOfType(page, typeof(DotvvmView));
            Assert.AreEqual(2, page.Children.Count);

            Assert.IsInstanceOfType(page.Children[0], typeof(RawLiteral));
            Assert.AreEqual("test ", ((RawLiteral)page.Children[0]).EncodedText);
            Assert.IsInstanceOfType(page.Children[1], typeof(Literal));
            Assert.AreEqual("test", ((Literal)page.Children[1]).Text);
        }

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_ElementWithBindingProperty()
        {
            var markup = string.Format("@viewModel {0}, {1}\r\ntest <dot:Literal Text='{{{{value: FirstName}}}}' />", typeof(ViewCompilerTestViewModel).FullName, typeof(ViewCompilerTestViewModel).Assembly.GetName().Name);
            var page = CompileMarkup(markup);

            Assert.IsInstanceOfType(page, typeof(DotvvmView));
            Assert.AreEqual(2, page.Children.Count);

            Assert.IsInstanceOfType(page.Children[0], typeof(RawLiteral));
            Assert.AreEqual("test ", ((RawLiteral)page.Children[0]).EncodedText);
            Assert.IsInstanceOfType(page.Children[1], typeof(Literal));

            var binding = ((Literal)page.Children[1]).GetBinding(Literal.TextProperty) as ValueBindingExpression;
            Assert.IsNotNull(binding);
            Assert.AreEqual("FirstName", binding.GetProperty<OriginalStringBindingProperty>().Code);
        }

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_BindingInText()
        {
            var markup = string.Format("@viewModel {0}, {1}\r\ntest {{{{value: FirstName}}}}", typeof(ViewCompilerTestViewModel).FullName, typeof(ViewCompilerTestViewModel).Assembly.GetName().Name);
            var page = CompileMarkup(markup);

            Assert.IsInstanceOfType(page, typeof(DotvvmView));
            Assert.AreEqual(2, page.Children.Count);

            Assert.IsInstanceOfType(page.Children[0], typeof(RawLiteral));
            Assert.AreEqual("test ", ((RawLiteral)page.Children[0]).EncodedText);
            Assert.IsInstanceOfType(page.Children[1], typeof(Literal));

            var binding = ((Literal)page.Children[1]).GetBinding(Literal.TextProperty) as ValueBindingExpression;
            Assert.IsNotNull(binding);
            Assert.AreEqual("FirstName", binding.GetProperty<OriginalStringBindingProperty>().Code);
        }

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_NestedControls()
        {
            var markup = @"@viewModel System.Object, mscorlib
<dot:PlaceHolder>test <dot:Literal /></dot:PlaceHolder>";
            var page = CompileMarkup(markup);

            Assert.IsInstanceOfType(page, typeof(DotvvmView));
            Assert.AreEqual(1, page.Children.Count);

            Assert.IsInstanceOfType(page.Children[0], typeof(PlaceHolder));

            Assert.AreEqual(2, page.Children[0].Children.Count);
            Assert.IsTrue(page.Children[0].Children[0] is RawLiteral);
            Assert.IsTrue(page.Children[0].Children[1] is Literal);
            Assert.AreEqual("test ", ((RawLiteral)page.Children[0].Children[0]).EncodedText);
            Assert.AreEqual("", ((Literal)page.Children[0].Children[1]).Text);
        }


        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_ElementCannotHaveContent_TextInside()
        {
            Assert.ThrowsException<DotvvmCompilationException>(() =>
            {
                var markup = @"@viewModel System.Object, mscorlib
test <dot:Literal>aaa</dot:Literal>";
                var page = CompileMarkup(markup);
            });
        }

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_ElementCannotHaveContent_BindingAndWhiteSpaceInside()
        {
            Assert.ThrowsException<DotvvmCompilationException>(() =>
            {
                var markup = @"@viewModel System.Object, mscorlib
test <dot:Literal>{{value: FirstName}}  </dot:Literal>";
                var page = CompileMarkup(markup);
            });
        }

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_ElementCannotHaveContent_ElementInside()
        {
            Assert.ThrowsException<DotvvmCompilationException>(() =>
     {
         var markup = @"@viewModel System.Object, mscorlib
test <dot:Literal><a /></dot:Literal>";
         var page = CompileMarkup(markup);
     });
        }

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_Template()
        {
            var markup = string.Format("@viewModel {0}, {1}\r\n", typeof(ViewCompilerTestViewModel).FullName, typeof(ViewCompilerTestViewModel).Assembly.GetName().Name) +
@"<dot:Repeater DataSource=""{value: FirstName}"">
    <ItemTemplate>
        <p>This is a test</p>
    </ItemTemplate>
</dot:Repeater>";
            var page = CompileMarkup(markup);

            Assert.IsInstanceOfType(page, typeof(DotvvmView));
            Assert.AreEqual(1, page.Children.Count);

            Assert.IsInstanceOfType(page.Children[0], typeof(Repeater));

            DotvvmControl placeholder = new PlaceHolder();
            ((Repeater)page.Children[0]).ItemTemplate.BuildContent(context, placeholder);

            Assert.AreEqual(3, placeholder.Children.Count);
            Assert.IsTrue(string.IsNullOrWhiteSpace(((RawLiteral)placeholder.Children[0]).EncodedText));
            Assert.AreEqual("p", ((HtmlGenericControl)placeholder.Children[1]).TagName);
            Assert.AreEqual("This is a test", ((RawLiteral)placeholder.Children[1].Children[0]).EncodedText);
            Assert.IsTrue(string.IsNullOrWhiteSpace(((RawLiteral)placeholder.Children[2]).EncodedText));
        }



        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_AttachedProperty()
        {
            var markup = @"@viewModel System.Object, mscorlib
<dot:Button Validation.Enabled=""false"" /><dot:Button Validation.Enabled=""true"" /><dot:Button />";
            var page = CompileMarkup(markup);

            Assert.IsInstanceOfType(page, typeof(DotvvmView));

            var button1 = page.Children[0];
            Assert.IsInstanceOfType(button1, typeof(Button));
            Assert.IsFalse((bool)button1.GetValue(Controls.Validation.EnabledProperty));

            var button2 = page.Children[1];
            Assert.IsInstanceOfType(button2, typeof(Button));
            Assert.IsTrue((bool)button2.GetValue(Controls.Validation.EnabledProperty));

            var button3 = page.Children[2];
            Assert.IsInstanceOfType(button3, typeof(Button));
            Assert.IsTrue((bool)button3.GetValue(Controls.Validation.EnabledProperty));
        }



        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_MarkupControl()
        {
            var markup = @"@viewModel System.Object, mscorlib
<cc:Test1 />";
            var page = CompileMarkup(markup, new Dictionary<string, string>()
            {
                { "test1.dothtml", @"@viewModel System.Object, mscorlib
<dot:Literal Text='aaa' />" }
            });

            Assert.IsInstanceOfType(page, typeof(DotvvmView));
            Assert.IsInstanceOfType(page.Children[0], typeof(DotvvmView));

            var literal = page.Children[0].Children[0];
            Assert.IsInstanceOfType(literal, typeof(Literal));
            Assert.AreEqual("aaa", ((Literal)literal).Text);
        }

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_MarkupControlWithBaseType()
        {
            var markup = @"@viewModel System.Object, mscorlib
<cc:Test2 />";
            var page = CompileMarkup(markup, new Dictionary<string, string>()
            {
                { "test2.dothtml", string.Format("@baseType {0}, {1}\r\n@viewModel System.Object, mscorlib\r\n<dot:Literal Text='aaa' />", typeof(TestControl), typeof(TestControl).Assembly.GetName().Name) }
            });

            Assert.IsInstanceOfType(page, typeof(DotvvmView));
            Assert.IsInstanceOfType(page.Children[0], typeof(TestControl));

            var literal = page.Children[0].Children[0].Children[0];
            Assert.IsInstanceOfType(literal, typeof(Literal));
            Assert.AreEqual("aaa", ((Literal)literal).Text);
        }

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_MarkupControlWithDI()
        {
            var markup = @"@viewModel System.Object, mscorlib
<cc:Test5 />";
            var page = CompileMarkup(markup, new Dictionary<string, string>()
            {
                { "test5.dothtml", $"@baseType {typeof(TestMarkupDIControl)}\n@viewModel System.Object, mscorlib\n<dot:Literal Text='aaa' />" }
            });

            Assert.IsInstanceOfType(page, typeof(DotvvmView));
            Assert.IsInstanceOfType(page.Children[0], typeof(TestMarkupDIControl));

            var control = (TestMarkupDIControl)page.Children[0];
            Assert.IsNotNull(control.config);

            var literal = page.Children[0].Children[0].Children[0];
            Assert.IsInstanceOfType(literal, typeof(Literal));
            Assert.AreEqual("aaa", ((Literal)literal).Text);
        }

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_MarkupControl_InTemplate()
        {
            var markup = string.Format("@viewModel {0}, {1}\r\n", typeof(ViewCompilerTestViewModel).FullName, typeof(ViewCompilerTestViewModel).Assembly.GetName().Name) +
@"<dot:Repeater DataSource=""{value: FirstName}"">
    <ItemTemplate>
        <cc:Test3 />
    </ItemTemplate>
</dot:Repeater>";
            var page = CompileMarkup(markup, new Dictionary<string, string>()
            {
                { "test3.dothtml", "@viewModel System.Char, mscorlib\r\n<dot:Literal Text='aaa' />" }
            });

            Assert.IsInstanceOfType(page, typeof(DotvvmView));
            Assert.IsInstanceOfType(page.Children[0], typeof(Repeater));

            var container = new PlaceHolder();
            ((Repeater)page.Children[0]).ItemTemplate.BuildContent(context, container);

            var literal1 = container.Children[0];
            Assert.IsInstanceOfType(literal1, typeof(RawLiteral));
            Assert.IsTrue(string.IsNullOrWhiteSpace(((RawLiteral)literal1).EncodedText));

            var markupControl = container.Children[1];
            Assert.IsInstanceOfType(markupControl, typeof(DotvvmView));
            Assert.IsInstanceOfType(markupControl.Children[0], typeof(Literal));
            Assert.AreEqual("aaa", ((Literal)markupControl.Children[0]).Text);

            var literal2 = container.Children[2];
            Assert.IsInstanceOfType(literal2, typeof(RawLiteral));
            Assert.IsTrue(string.IsNullOrWhiteSpace(((RawLiteral)literal2).EncodedText));
        }

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_MarkupControl_InTemplate_CacheTest()
        {
            var markup = string.Format("@viewModel {0}, {1}\r\n", typeof(ViewCompilerTestViewModel).FullName, typeof(ViewCompilerTestViewModel).Assembly.GetName().Name) +
@"<dot:Repeater DataSource=""{value: FirstName}"">
    <ItemTemplate>
        <cc:Test4 />
    </ItemTemplate>
</dot:Repeater>";
            var page = CompileMarkup(markup, new Dictionary<string, string>()
            {
                { "test4.dothtml", "@viewModel System.Char, mscorlib\r\n<dot:Literal Text='aaa' />" }
            }, compileTwice: true);

            Assert.IsInstanceOfType(page, typeof(DotvvmView));
            Assert.IsInstanceOfType(page.Children[0], typeof(Repeater));

            var container = new PlaceHolder();
            ((Repeater)page.Children[0]).ItemTemplate.BuildContent(context, container);

            var literal1 = container.Children[0];
            Assert.IsInstanceOfType(literal1, typeof(RawLiteral));
            Assert.IsTrue(string.IsNullOrWhiteSpace(((RawLiteral)literal1).EncodedText));

            var markupControl = container.Children[1];
            Assert.IsInstanceOfType(markupControl, typeof(DotvvmView));
            Assert.IsInstanceOfType(markupControl.Children[0], typeof(Literal));
            Assert.AreEqual("aaa", ((Literal)markupControl.Children[0]).Text);

            var literal2 = container.Children[2];
            Assert.IsInstanceOfType(literal2, typeof(RawLiteral));
            Assert.IsTrue(string.IsNullOrWhiteSpace(((RawLiteral)literal2).EncodedText));
        }



        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_Page_InvalidViewModelClass()
        {
            Assert.ThrowsException<DotvvmCompilationException>(() =>
            {
                var markup = "@viewModel nonexistingclass\r\n{{value: Test}}";
                var page = CompileMarkup(markup);
            });
        }

        [TestMethod]
        public void DefaultViewCompiler_FlagsEnum()
        {
            var markup = @"
@viewModel System.Object
<ff:TestCodeControl Flags='A, B, C' />";
            var page = CompileMarkup(markup);
            Assert.AreEqual(FlaggyEnum.A | FlaggyEnum.B | FlaggyEnum.C, page.GetThisAndAllDescendants().OfType<TestCodeControl>().First().Flags);
        }

        [TestMethod]
        public void DefaultViewCompiler_CustomDependencyInjection()
        {
            var markup = @"
@viewModel System.Object
<ff:TestCustomDependencyInjectionControl />";
            var page = CompileMarkup(markup);
            Assert.IsTrue(page.GetThisAndAllDescendants().OfType<TestCustomDependencyInjectionControl>().First().IsCorrectlyCreated);
        }

        [TestMethod]

        public void ComboBox_ControlUsageValidation()
        {
            // CheckedItems must be a collection of CheckedValues
            var markup = @"
@viewModel System.String
<dot:ComboBox CheckedValue='{value: Length}' CheckedItems='{value: _this}' />";
            Assert.ThrowsException<DotvvmCompilationException>(() => CompileMarkup(markup));
        }

        [TestMethod]
        public void RadioButton_ControlUsageValidation()
        {
            // CheckedValue and CheckedItem must be the same type
            var markup = @"
@viewModel System.String
<dot:RadioButton CheckedValue='{value: _this}' CheckedItem='{value: Length}' />";
            Assert.ThrowsException<DotvvmCompilationException>(() => CompileMarkup(markup));
        }

        [TestMethod]
        public void DefaultViewCompiler_ViewDependencyInjection()
        {
            var markup = @"
@viewModel System.Object
@service config=DotVVM.Framework.Configuration.DotvvmConfiguration
{{resource: config.ApplicationPhysicalPath}}{{resource: config.DefaultCulture}}";
            var page = CompileMarkup(markup);
            var literals = page.GetAllDescendants().OfType<Literal>().ToArray();
            Assert.AreEqual(2, literals.Length);
            Assert.AreEqual(context.Configuration.ApplicationPhysicalPath, literals[0].Text);
            Assert.AreEqual(context.Configuration.DefaultCulture, literals[1].Text);
        }

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_PostbackHandlerResourceRegistration()
        {
            var markup = @"

@viewModel System.Object
<dot:Button Click='{command: 0}'>
    <Postback.Handlers>
        <ff:PostbackHandlerWithRequiredResource />
    </Postback.Handlers>
</dot:Button>
";
            var page = CompileMarkup(markup);
            Assert.IsTrue(context.ResourceManager.RequiredResources.Contains("testscript"));
        }

        [TestMethod]
        public void DefaultViewCompiler_ControlWithDependentProperties()
        {
            var markup = @"
@viewModel System.Collections.Generic.List<string>
<ff:ControlWithCompileDependentProperties OtherProperty='{value: _this.Length}' DataSource='{value: _this}'>
</ff:ControlWithCompileDependentProperties>
";
            var page = CompileMarkup(markup);
	}

        [TestMethod]
        public void DefaultViewCompiler_ExcludedBindingProperty()
        {
            var markup = @"
@viewModel object
<ff:ControlWithCustomBindingProperties SomeProperty='{value: System.Environment.GetCommandLineArgs()}' />
            ";

            var page = CompileMarkup(markup);
            var control = page.GetThisAndAllDescendants().OfType<ControlWithCustomBindingProperties>().Single();
            var assemblies = ((IEnumerable<object>)control.SomeProperty).ToArray();
            Assert.IsNotNull(assemblies);
            var lengthBinding = control.GetValueBinding(ControlWithCustomBindingProperties.SomePropertyProperty).GetProperty<DataSourceLengthBinding>().Binding;
            var lengthValue = BindingHelper.Evaluate((IStaticValueBinding)lengthBinding, control);
            Assert.AreEqual(assemblies.Length, lengthValue);
        }

        [TestMethod]
        public void DefaultViewCompiler_RequiredBindingProperty()
        {
            var markup = @"
@viewModel object
@import AppDomain = System.AppDomain
<ff:ControlWithCustomBindingProperties SomeProperty='{value: _this}' />
            ";
            var ex = Assert.ThrowsException<DotvvmCompilationException>(() => {
                CompileMarkup(markup);
            });
            Assert.IsTrue(ex.ToString().Contains("DotVVM.Framework.Binding.Properties.DataSourceLengthBinding"));
            Assert.IsTrue(ex.ToString().Contains("Can not find collection length from binding '_this'"));
        }

        [TestMethod]
        public void DefaultViewCompiler_InternalControl_Error()
        {
            var markup = @"
@viewModel object
<ff:InternalControl />
            ";
            var ex = Assert.ThrowsException<DotvvmCompilationException>(() => {
                CompileMarkup(markup);
            });
            Assert.IsTrue(ex.ToString().Contains("Control DotVVM.Framework.Tests.Runtime.InternalControl is not publicly accessible."));
            Assert.IsFalse(ex.ToString().Contains("This is most probably bug in the DotVVM framework."));
        }

        // Well, DotvvmProperties work even when they are internal. So I can not add the check in order to remain backwards compatible :/

//         [TestMethod]
//         public void DefaultViewCompiler_InternalDotvvmProperty_Error()
//         {
//             var markup = @"
// @viewModel object
// <ff:PublicControl MyInternalDotvvmProperty=1 />
//             ";
//             // var ex = Assert.ThrowsException<DotvvmCompilationException>(() => {
//             var a = CompileMarkup(markup);
//             var x = a.GetThisAndAllDescendants().OfType<PublicControl>().Single().MyInternalDotvvmProperty;
//             Assert.AreEqual(1, x);
//             // });
//             // Assert.IsTrue(ex.ToString().Contains("Control DotVVM.Framework.Tests.Runtime.InternalControl is not publicly accessible."));
//             // Assert.IsFalse(ex.ToString().Contains("This is most probably bug in the DotVVM framework."));
//         }

        [TestMethod]
        public void DefaultViewCompiler_InternalVirtualProperty_Error()
        {
            var markup = @"
@viewModel object
<ff:PublicControl MyInternalProperty=1 />
            ";
            var ex = Assert.ThrowsException<DotvvmCompilationException>(() => {
                CompileMarkup(markup);
            });
            Assert.IsTrue(ex.ToString().Contains("The control 'DotVVM.Framework.Tests.Runtime.PublicControl' does not have a property 'MyInternalProperty'"));
            Assert.IsFalse(ex.ToString().Contains("This is most probably bug in the DotVVM framework."));
        }

        private DotvvmControl CompileMarkup(string markup, Dictionary<string, string> markupFiles = null, bool compileTwice = false, [CallerMemberName]string fileName = null)
        {
            if (markupFiles == null)
            {
                markupFiles = new Dictionary<string, string>();
            }
            markupFiles[fileName + ".dothtml"] = markup;

            var config = DotvvmTestHelper.CreateConfiguration(services =>
            {
                services.AddSingleton<IMarkupFileLoader>(new FakeMarkupFileLoader(markupFiles));
                services.AddSingleton<CustomControlFactory>((s, t) =>
                    t == typeof(TestCustomDependencyInjectionControl) ? new TestCustomDependencyInjectionControl("") { IsCorrectlyCreated = true } :
                    throw new Exception());
            });
            context = DotvvmTestHelper.CreateContext(config);
            context.Configuration.ApplicationPhysicalPath = Path.GetTempPath();

            context.Configuration.Markup.Controls.Add(new DotvvmControlConfiguration() { TagPrefix = "cc", TagName = "Test1", Src = "test1.dothtml" });
            context.Configuration.Markup.Controls.Add(new DotvvmControlConfiguration() { TagPrefix = "cc", TagName = "Test2", Src = "test2.dothtml" });
            context.Configuration.Markup.Controls.Add(new DotvvmControlConfiguration() { TagPrefix = "cc", TagName = "Test3", Src = "test3.dothtml" });
            context.Configuration.Markup.Controls.Add(new DotvvmControlConfiguration() { TagPrefix = "cc", TagName = "Test4", Src = "test4.dothtml" });
            context.Configuration.Markup.Controls.Add(new DotvvmControlConfiguration() { TagPrefix = "cc", TagName = "Test5", Src = "test5.dothtml" });
            context.Configuration.Markup.AddCodeControls("ff", typeof(TestControl));
            context.Configuration.Markup.AddAssembly(typeof(DefaultViewCompilerTests).Assembly.GetName().Name);

            var controlBuilderFactory = context.Services.GetRequiredService<IControlBuilderFactory>();
            var (_, controlBuilder) = controlBuilderFactory.GetControlBuilder(fileName + ".dothtml");

            var result = controlBuilder.Value.BuildControl(controlBuilderFactory, context.Services);
            if (compileTwice)
            {
                result = controlBuilder.Value.BuildControl(controlBuilderFactory, context.Services);
            }
            result.SetValue(Internal.RequestContextProperty, context);
            return result;
        }

    }

    internal class InternalControl: DotvvmControl
    {

    }

    public class PublicControl: DotvvmControl
    {
        [MarkupOptions()]
        internal int MyInternalProperty { get; set; }


        internal int MyInternalDotvvmProperty
        {
            get { return (int)GetValue(MyInternalDotvvmPropertyProperty); }
            set { SetValue(MyInternalDotvvmPropertyProperty, value); }
        }
        internal static readonly DotvvmProperty MyInternalDotvvmPropertyProperty =
            DotvvmProperty.Register<int, PublicControl>(nameof(MyInternalDotvvmProperty));
    }

    public class PostbackHandlerWithRequiredResource : PostBackHandler
    {
        public PostbackHandlerWithRequiredResource(ResourceManager resources)
        {
            resources.AddStartupScript("testscript", "do_some_stuff()");
        }

        protected internal override string ClientHandlerName => "something";

        protected internal override Dictionary<string, object> GetHandlerOptions()
        {
            return new Dictionary<string, object>();
        }
    }

    public class ViewCompilerTestViewModel
    {
        public string FirstName { get; set; }
    }

    public class TestControl : DotvvmMarkupControl
    {

    }

    public class TestMarkupDIControl : DotvvmMarkupControl
    {
        public readonly DotvvmConfiguration config;

        public TestMarkupDIControl(DotvvmConfiguration configuration)
        {
            this.config = configuration;
        }
    }

    public class TestDIControl : DotvvmControl
    {
        public readonly DotvvmConfiguration config;

        public TestDIControl(DotvvmConfiguration configuration)
        {
            this.config = configuration;
        }
    }

    [Flags]
    public enum FlaggyEnum { A, B, C, D}

    public class TestCodeControl: DotvvmControl
    {
        public FlaggyEnum Flags
        {
            get { return (FlaggyEnum)GetValue(FlagsProperty); }
            set { SetValue(FlagsProperty, value); }
        }

        public static readonly DotvvmProperty FlagsProperty =
            DotvvmProperty.Register<FlaggyEnum, TestCodeControl>(nameof(Flags));
    }

    public delegate DotvvmControl CustomControlFactory(IServiceProvider sp, Type controlType);

    [RequireDependencyInjection(typeof(CustomControlFactory))]
    public class TestCustomDependencyInjectionControl: DotvvmControl
    {
        public bool IsCorrectlyCreated { get; set; } = false;

        public TestCustomDependencyInjectionControl(string something) { }
    }

    public class ControlWithCompileDependentProperties: DotvvmControl
    {
        public IEnumerable<object> DataSource
        {
            get { return (IEnumerable<object>)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }
        public static readonly DotvvmProperty DataSourceProperty =
            DotvvmProperty.Register<IEnumerable<object>, ControlWithCompileDependentProperties>(nameof(DataSource));


        [ControlPropertyBindingDataContextChange("DataSource")]
        [CollectionElementDataContextChange(1)]
        public IValueBinding OtherProperty
        {
            get { return (IValueBinding)GetValue(OtherPropertyProperty); }
            set { SetValue(OtherPropertyProperty, value); }
        }
        public static readonly DotvvmProperty OtherPropertyProperty =
            DotvvmProperty.Register<IValueBinding, ControlWithCompileDependentProperties>(nameof(OtherProperty));
    }

    public class ControlWithCustomBindingProperties : DotvvmControl
    {
        [BindingCompilationRequirements(
            excluded: new [] { typeof(KnockoutExpressionBindingProperty) },
            required: new [] { typeof(DataSourceLengthBinding) }
        )]
        public object SomeProperty
        {
            get { return GetValue(SomePropertyProperty); }
            set { SetValue(SomePropertyProperty, value); }
        }
        public static readonly DotvvmProperty SomePropertyProperty =
            DotvvmProperty.Register<object, ControlWithCustomBindingProperties>(nameof(SomeProperty));
    }
}
