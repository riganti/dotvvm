using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    [TestClass]
    public class DefaultControlTreeResolverTests
    {
        private DotvvmConfiguration configuration;
        private IControlTreeResolver controlTreeResolver;

        [TestInitialize()]
        public void TestInit()
        {
            configuration = DotvvmConfiguration.CreateDefault();
            controlTreeResolver = configuration.ServiceLocator.GetService<IControlTreeResolver>();
        }

        [TestMethod]
        public void ResolvedTree_MissingViewModelDirective()
        {
            var root = ParseSource(@"");

            Assert.IsTrue(root.DothtmlNode.HasNodeErrors);
            Assert.IsTrue(root.DothtmlNode.NodeErrors.First().Contains("missing"));
        }

        [TestMethod]
        public void ResolvedTree_UnknownViewModelType()
        {
            var root = ParseSource(@"@viewModel invalid
");

            var directiveNode = ((DothtmlRootNode)root.DothtmlNode).Directives.First();
            Assert.IsTrue(directiveNode.HasNodeErrors);
            Assert.IsTrue(directiveNode.NodeErrors.First().Contains("not found"));
        }

        [TestMethod]
        public void ResolvedTree_WhiteSpaceLiteral()
        {
            var root = ParseSource(@"     ");

            var control = root.Content.First();
            Assert.AreEqual(typeof(RawLiteral), control.Metadata.Type);

            Assert.AreEqual(root, control.Parent);
        }

        [TestMethod]
        public void ResolvedTree_SingleControl()
        {
            var root = ParseSource(@"<dot:Button />");

            var control = root.Content.First();
            Assert.AreEqual(typeof(Button), control.Metadata.Type);

            Assert.AreEqual(root, control.Parent);
        }

        [TestMethod]
        public void ResolvedTree_SingleControlWithBinding_BindingError_MissingViewModelDirective()
        {
            var root = ParseSource(@"<dot:Button Text='{value: Test}' />");

            var control = root.Content.First();
            var textBinding = (ResolvedPropertyBinding)control.Properties[ButtonBase.TextProperty];
            Assert.IsNotNull(textBinding.Binding.ParsingError);
            Assert.IsTrue(textBinding.Binding.ParsingError.Message.Contains("couldn't be evaluated"));

            Assert.AreEqual(root, control.Parent);
            Assert.AreEqual(control, textBinding.Parent);
            Assert.AreEqual(textBinding, textBinding.Binding.Parent);
        }

        [TestMethod]
        public void ResolvedTree_SingleControlWithBinding_ValidBinding_UnknownViewModel()
        {
            var root = ParseSource(@"@viewModel invalid
<dot:Button Text='{value: Test}' />");

            var control = root.Content.First();
            var textBinding = (ResolvedPropertyBinding)control.Properties[ButtonBase.TextProperty];
            Assert.IsNotNull(textBinding.Binding.ParsingError);
            Assert.IsTrue(textBinding.Binding.ParsingError.Message.Contains("couldn't be evaluated"));

            Assert.AreEqual(root, control.Parent);
            Assert.AreEqual(control, textBinding.Parent);
            Assert.AreEqual(textBinding, textBinding.Binding.Parent);
        }

        [TestMethod]
        public void ResolvedTree_SingleControlWithBinding_ValidBinding()
        {
            var root = ParseSource(@"@viewModel System.String, mscorlib
<dot:Button Text='{value: Length}' />");

            var control = root.Content.First();
            var textBinding = (ResolvedPropertyBinding)control.Properties[ButtonBase.TextProperty];
            Assert.IsNull(textBinding.Binding.ParsingError);
            Assert.AreEqual(typeof(int), ResolvedTypeDescriptor.ToSystemType(textBinding.Binding.ResultType));

            Assert.AreEqual(root, control.Parent);
            Assert.AreEqual(control, textBinding.Parent);
            Assert.AreEqual(textBinding, textBinding.Binding.Parent);
        }

        [TestMethod]
        public void ResolvedTree_SingleControl_HtmlAttribute()
        {
            var root = ParseSource(@"@viewModel System.String, mscorlib
<dot:Button class=active />");

            var control = root.Content.First();
            Assert.AreEqual("active", control.HtmlAttributes["class"]);

            Assert.AreEqual(root, control.Parent);
        }

        [TestMethod]
        public void ResolvedTree_SingleControl_HtmlAttributeWithBinding()
        {
            var root = ParseSource(@"@viewModel System.String, mscorlib
<dot:Button class='{value: Length}' />");

            var control = root.Content.First();
            var binding = (ResolvedBinding)control.HtmlAttributes["class"];
            Assert.IsNull(binding.ParsingError);
            Assert.AreEqual(typeof(int), ResolvedTypeDescriptor.ToSystemType(binding.ResultType));

            Assert.AreEqual(root, control.Parent);
            Assert.AreEqual(control, binding.Parent);
        }

        [TestMethod]
        public void ResolvedTree_SingleControlWithStaticValue_String()
        {
            var root = ParseSource(@"@viewModel System.String, mscorlib
<dot:Button Text='text' />");

            var control = root.Content.First();
            var textValue = (ResolvedPropertyValue)control.Properties[ButtonBase.TextProperty];
            Assert.AreEqual("text", textValue.Value);

            Assert.AreEqual(root, control.Parent);
            Assert.AreEqual(control, textValue.Parent);
        }

        [TestMethod]
        public void ResolvedTree_SingleControlWithStaticValue_Enum()
        {
            var root = ParseSource(@"@viewModel System.String, mscorlib
<dot:Button ButtonTagName='Input' />");

            var control = root.Content.First();
            var textValue = (ResolvedPropertyValue)control.Properties[Button.ButtonTagNameProperty];
            Assert.AreEqual(ButtonTagName.input, textValue.Value);

            Assert.AreEqual(root, control.Parent);
            Assert.AreEqual(control, textValue.Parent);
        }

        [TestMethod]
        public void ResolvedTree_UnknownElement()
        {
            var root = ParseSource(@"@viewModel System.String, mscorlib
<dot:xxxButton />");

            var control = root.Content.First();
            Assert.AreEqual(typeof(HtmlGenericControl), control.Metadata.Type);
            Assert.AreEqual(1, control.ConstructorParameters.Length);
            Assert.AreEqual("dot:xxxButton", control.ConstructorParameters[0]);
            Assert.IsTrue(control.DothtmlNode.HasNodeErrors);
            Assert.IsTrue(control.DothtmlNode.NodeErrors.First().Contains("could not be resolved"));

            Assert.AreEqual(root, control.Parent);
        }

        [TestMethod]
        public void ResolvedTree_ElementProperty()
        {
            var root = ParseSource(@"@viewModel " + typeof(DefaultControlResolverTestViewModel).AssemblyQualifiedName + @"
<dot:Repeater DataSource='{value: Items}'>
    <ItemTemplate>
        <dot:Button Text='{value: _this}' />
    </ItemTemplate>
</dot:Repeater>");

            var control = root.Content.First();
            Assert.AreEqual(typeof(Repeater), control.Metadata.Type);

            var dataSource = (ResolvedPropertyBinding)control.Properties[ItemsControl.DataSourceProperty];
            Assert.IsNull(dataSource.Binding.ParsingError);

            var itemTemplate = (ResolvedPropertyTemplate)control.Properties[Repeater.ItemTemplateProperty];
            var button = itemTemplate.Content.FirstOrDefault(c => c.Metadata.Type == typeof(Button));

            var text = (ResolvedPropertyBinding)button.Properties[ButtonBase.TextProperty];
            Assert.IsNull(text.Binding.ParsingError);

            Assert.AreEqual(root, control.Parent);
            Assert.AreEqual(control, dataSource.Parent);
            Assert.AreEqual(dataSource, dataSource.Binding.Parent);
            Assert.AreEqual(control, itemTemplate.Parent);
            Assert.AreEqual(itemTemplate, button.Parent);
            Assert.AreEqual(button, text.Parent);
            Assert.AreEqual(text, text.Binding.Parent);
        }

        [TestMethod]
        public void ResolvedTree_DefaultElementProperty()
        {
            var root = ParseSource(@"@viewModel " + typeof(DefaultControlResolverTestViewModel).AssemblyQualifiedName + @"
<dot:Repeater DataSource='{value: Items}'>
    <dot:Button Text='{value: _this}' />
</dot:Repeater>");

            var control = root.Content.First();
            Assert.AreEqual(typeof(Repeater), control.Metadata.Type);

            var dataSource = (ResolvedPropertyBinding)control.Properties[ItemsControl.DataSourceProperty];
            Assert.IsNull(dataSource.Binding.ParsingError);

            var itemTemplate = (ResolvedPropertyTemplate)control.Properties[Repeater.ItemTemplateProperty];
            var button = itemTemplate.Content.FirstOrDefault(c => c.Metadata.Type == typeof(Button));

            var text = (ResolvedPropertyBinding)button.Properties[ButtonBase.TextProperty];
            Assert.IsNull(text.Binding.ParsingError);

            Assert.AreEqual(root, control.Parent);
            Assert.AreEqual(control, dataSource.Parent);
            Assert.AreEqual(dataSource, dataSource.Binding.Parent);
            Assert.AreEqual(control, itemTemplate.Parent);
            Assert.AreEqual(itemTemplate, button.Parent);
            Assert.AreEqual(button, text.Parent);
            Assert.AreEqual(text, text.Binding.Parent);
        }

        [TestMethod]
        public void ResolvedTree_Binding_DataContextChange_InvalidType()
        {
            var root = ParseSource(@"@viewModel " + typeof(DefaultControlResolverTestViewModel).AssemblyQualifiedName + @"
<dot:Repeater DataSource='{value: Items2}'>
    <dot:Button Text='{value: _this}' />
</dot:Repeater>");

            var control = root.Content.First();
            Assert.AreEqual(typeof(Repeater), control.Metadata.Type);

            var dataSource = (ResolvedPropertyBinding)control.Properties[ItemsControl.DataSourceProperty];
            Assert.IsNotNull(dataSource.Binding.ParsingError);

            var itemTemplate = (ResolvedPropertyTemplate)control.Properties[Repeater.ItemTemplateProperty];
            var button = itemTemplate.Content.FirstOrDefault(c => c.Metadata.Type == typeof(Button));

            var text = (ResolvedPropertyBinding)button.Properties[ButtonBase.TextProperty];
            Assert.IsNull(text.Binding.ParsingError);

            Assert.AreEqual(root, control.Parent);
            Assert.AreEqual(control, dataSource.Parent);
            Assert.AreEqual(dataSource, dataSource.Binding.Parent);
            Assert.AreEqual(control, itemTemplate.Parent);
            Assert.AreEqual(itemTemplate, button.Parent);
            Assert.AreEqual(button, text.Parent);
            Assert.AreEqual(text, text.Binding.Parent);
        }

        [TestMethod]
        public void ResolvedTree_AttachedProperty()
        {
            var root = ParseSource(@"
@viewModel System.Object

<div Events.Click='{command: GetHashCode()}' />
");
            var control = root.Content.First(c => c.Metadata.Name == nameof(HtmlGenericControl));
            ResolvedPropertySetter clickProp;
            Assert.IsTrue(control.Properties.TryGetValue(Events.ClickProperty, out clickProp));
            Assert.IsInstanceOfType(clickProp, typeof(ResolvedPropertyBinding));
        }

        [TestMethod]
        public void ResolvedTree_GridViewColumns_InvalidItem()
        {
            var root = ParseSource(@"
@viewModel System.Collections.IEnumerable

<dot:GridView DataSource='{value: _this}'>
    <Columns>
        <span data-hh='error' />
    </Columns>
</dot:GridView>
");
            var gridView = root.Content.First(r => r.Metadata.Name == "GridView");
            IAbstractPropertySetter colsProp;
            Assert.IsTrue(gridView.TryGetProperty(GridView.ColumnsProperty, out colsProp));
            var cols = ((ResolvedPropertyControlCollection)colsProp).Controls;
            Assert.AreEqual(0, cols.Count); // span should not be added
            Assert.IsTrue(gridView.DothtmlNode.EnumerateNodes().Any(n => n.HasNodeErrors));
        }

        [TestMethod]
        public void ResolvedTree_ControlContent_Invalid()
        {
            var root = ParseSource(@"
@viewModel System.Collections.IEnumerable

<dot:ValidationSummary>
    <span />
</dot:ValidationSummary>
");
            var control = root.Content.First(r => r.Metadata.Name == nameof(ValidationSummary));
            Assert.AreEqual(0, control.Content.Count);
            Assert.IsTrue(control.DothtmlNode.EnumerateNodes().Any(n => n.HasNodeErrors));
        }

        [TestMethod]
        public void ResolvedTree_HtmlAttributes_Invalid()
        {
            var root = ParseSource(@"
@viewModel System.Collections.IEnumerable

<dot:RequiredResource Name='ggg11' html:class='jshfsjhfkj'>
");
            var control = root.Content.First(r => r.Metadata.Name == nameof(RequiredResource));
            Assert.AreEqual(0, control.HtmlAttributes?.Count ?? 0);
            Assert.IsTrue(((DothtmlElementNode)control.DothtmlNode).Attributes.Any(a => a.HasNodeErrors));
        }

        [TestMethod]
        public void ResolvedTree_BindingHierarchy_Invalid()
        {
            var root = ParseSource(@"
@viewModel System.Object
<div DataContext='{value: Property123}'>
    {{value: AnotherProperty}}
</div>
");
            var div = root.Content.First(r => r.Metadata.Name == nameof(HtmlGenericControl));
            Assert.IsTrue((div.Properties[DotvvmBindableObject.DataContextProperty] as ResolvedPropertyBinding).Binding.ParsingError != null);

        }

        [TestMethod]
        public void ResolvedTree_BaseType_Invalid()
        {
            var root = ParseSource(@"
@baseType someBullshitttt
<span />
");
            Assert.IsTrue(((DothtmlRootNode)root.DothtmlNode).Directives.First().HasNodeErrors);
        }

        [TestMethod]
        public void ResolvedTree_DefaultContentProperty_BindingInside()
        {
            var root = ParseSource(@"
@viewModel System.Object
<dot:GridViewTemplateColumn HeaderText='Amount'>
    <dot:Literal Text='Text123' FormatString = 'n0' /> {{value: _this}}
</dot:GridViewTemplateColumn>
 ");
            var column = root.Content.First(t => t.Metadata.Name == nameof(GridViewTemplateColumn));
            Assert.IsFalse(column.DothtmlNode.HasNodeErrors, column.DothtmlNode.NodeErrors.FirstOrDefault());
            var template = (column.Properties.FirstOrDefault(p => p.Key.Name == nameof(GridViewTemplateColumn.ContentTemplate)).Value as ResolvedPropertyTemplate)?.Content;
            Assert.IsTrue(template.Any(n => n.DothtmlNode is DothtmlBindingNode));
            Assert.IsTrue(template.Any(n => n.DothtmlNode is DothtmlElementNode && n.Metadata.Name == "Literal"));
        }

        [TestMethod]
        public void ResolvedTree_ImplicitBooleanValue()
        {
            var root = ParseSource(@"
@viewModel System.Object
<dot:CheckBox Checked />
 ");
            var checkBox = root.Content.First(t => t.Metadata.Name == nameof(CheckBox));
            Assert.IsFalse(checkBox.DothtmlNode.HasNodeErrors, checkBox.DothtmlNode.NodeErrors.FirstOrDefault());
            Assert.AreEqual(true, (checkBox.Properties.FirstOrDefault(p => p.Key.Name == nameof(CheckBox.Checked)).Value as ResolvedPropertyValue)?.Value);
        }

        private ResolvedTreeRoot ParseSource(string markup, string fileName = "default.dothtml")
        {
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(markup));

            var parser = new DothtmlParser();
            var tree = parser.Parse(tokenizer.Tokens);

            return (ResolvedTreeRoot)controlTreeResolver.ResolveTree(tree, fileName);
        }

    }

    public class DefaultControlResolverTestViewModel
    {
        public List<string> Items { get; set; }
    }
}
