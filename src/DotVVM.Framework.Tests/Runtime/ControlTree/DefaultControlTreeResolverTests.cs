using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Utils;
using System.Linq.Expressions;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Compilation;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Testing;

namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    [TestClass]
    public class DefaultControlTreeResolverTests
    {
        private DotvvmConfiguration configuration;

        [TestInitialize()]
        public void TestInit()
        {
            configuration = DotvvmTestHelper.CreateConfiguration();
            configuration.Markup.AddCodeControls("cc", typeof(ClassWithInnerElementProperty));
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
            Assert.IsTrue(directiveNode.NodeErrors.First().Contains("Could not resolve type"));
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

        private static string GetParsingError(IBinding binding)
        {
            var ex = binding.GetProperty(typeof(ParsedExpressionBindingProperty), ErrorHandlingMode.ReturnException) as Exception;
            if (ex == null) return null;
            var errors = new List<BindingCompilationException>();
            ex.ForInnerExceptions<BindingCompilationException>(e => errors.Add(e));
            if (errors.Any()) return string.Join("; ", errors.Select(e => e.Message));
            else return ex.ToString();
        }

        [TestMethod]
        public void ResolvedTree_SingleControlWithBinding_BindingError_MissingViewModelDirective()
        {
            var root = ParseSource(@"<dot:Button Text='{value: Test}' />");

            var control = root.Content.First();
            var textBinding = (ResolvedPropertyBinding)control.Properties[ButtonBase.TextProperty];
            var error = GetParsingError(textBinding.Binding.Binding);
            Assert.IsNotNull(error);
            Assert.IsTrue(error.Contains("Could not resolve identifier"));

            Assert.AreEqual(root, control.Parent);
            Assert.AreEqual(control, textBinding.Parent);
            Assert.AreEqual(textBinding, textBinding.Binding.Parent);
        }

        [TestMethod]
        public void ResolvedTree_SingleControlWithBinding_BindingError_MissingViewModelDirectiveThisId()
        {
            var root = ParseSource(@"<dot:Button Text='{value: _this.Test}' />");

            var control = root.Content.First();
            var textBinding = (ResolvedPropertyBinding)control.Properties[ButtonBase.TextProperty];
            var error = GetParsingError(textBinding.Binding.Binding);
            Assert.IsNotNull(error);
            Assert.IsTrue(error.Contains("Type of '_this' could not be resolved."));

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
            var error = GetParsingError(textBinding.Binding.Binding);
            Assert.IsNotNull(error);
            Assert.IsTrue(error.Contains("Could not resolve identifier"));

            Assert.AreEqual(root, control.Parent);
            Assert.AreEqual(control, textBinding.Parent);
            Assert.AreEqual(textBinding, textBinding.Binding.Parent);
        }

        [TestMethod]
        public void ResolvedTree_SingleControlWithBinding_ValidBinding_UnknownViewModelInKnownOne()
        {
            var root = ParseSource(@"@viewModel System.String
<dot:Repeater DataSource='{value: inkaalid}'><dot:Button Text='{value: _parent.Substring(0, 3)}' /></dot:Repeater>");

            var control = root.Content.First().Properties.Values.OfType<ResolvedPropertyTemplate>().First().Content.First();
            var textBinding = (ResolvedPropertyBinding)control.Properties[ButtonBase.TextProperty];
            textBinding.Binding.GetExpression();
        }

        [TestMethod]
        public void ResolvedTree_SingleControlWithBinding_ValidBinding()
        {
            var root = ParseSource(@"@viewModel System.String, mscorlib
<dot:Button Text='{value: Length}' />");

            var control = root.Content.First();
            var textBinding = (ResolvedPropertyBinding)control.Properties[ButtonBase.TextProperty];
            textBinding.Binding.GetExpression();
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
            var attribute = control.GetHtmlAttribute("class") as IAbstractPropertyValue;
            Assert.AreEqual("active", attribute.Value);

            Assert.AreEqual(root, control.Parent);
        }

        [TestMethod]
        public void ResolvedTree_SingleControl_HtmlAttributeWithBinding()
        {
            var root = ParseSource(@"@viewModel System.String, mscorlib
<dot:Button class='{value: Length}' />");

            var control = root.Content.First();
            var attribute = ((ResolvedPropertyBinding)control.GetHtmlAttribute("class"));
            attribute.Binding.GetExpression();
            Assert.AreEqual(typeof(int), ResolvedTypeDescriptor.ToSystemType(attribute.Binding.ResultType));

            Assert.AreEqual(root, control.Parent);
            Assert.AreEqual(attribute, attribute.Binding.Parent);
            Assert.AreEqual(control, attribute.Parent);
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
            dataSource.Binding.GetExpression();

            var itemTemplate = (ResolvedPropertyTemplate)control.Properties[Repeater.ItemTemplateProperty];
            var button = itemTemplate.Content.FirstOrDefault(c => c.Metadata.Type == typeof(Button));

            var text = (ResolvedPropertyBinding)button.Properties[ButtonBase.TextProperty];
            text.Binding.GetExpression();

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
            dataSource.Binding.GetExpression();

            var itemTemplate = (ResolvedPropertyTemplate)control.Properties[Repeater.ItemTemplateProperty];
            var button = itemTemplate.Content.FirstOrDefault(c => c.Metadata.Type == typeof(Button));

            var text = (ResolvedPropertyBinding)button.Properties[ButtonBase.TextProperty];
            text.Binding.GetExpression();

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
            Assert.AreEqual(true, GetParsingError(dataSource.Binding.Binding)?.Contains("resolve identifier"));

            var itemTemplate = (ResolvedPropertyTemplate)control.Properties[Repeater.ItemTemplateProperty];
            var button = itemTemplate.Content.FirstOrDefault(c => c.Metadata.Type == typeof(Button));

            var text = (ResolvedPropertyBinding)button.Properties[ButtonBase.TextProperty];
            Assert.IsNotNull(GetParsingError(text.Binding.Binding));

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
            Assert.AreEqual(0, control.Properties.OfType<GroupedDotvvmProperty>().Where(a => a.PropertyGroup.Prefixes.Contains("")).Count());
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
            Assert.IsTrue((div.Properties[DotvvmBindableObject.DataContextProperty] as ResolvedPropertyBinding).Binding.Errors.HasErrors);

        }

        [TestMethod]
        public void ResolvedTree_BaseType_Invalid()
        {
            var root = ParseSource(@"
@baseType someBullshitttt
@viewModel object
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
        public void ResolvedTree_UnescapedAttributeValue()
        {
            var root = ParseSource(@"
@viewModel object
<div onclick='ahoj &gt; lao' />
 ");
            var column = root.Content.First(t => t.Metadata.Name == nameof(HtmlGenericControl));
            var attribute = (column.GetHtmlAttribute("onclick") as ResolvedPropertyValue);
            Assert.AreEqual(attribute.Value, "ahoj > lao");
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

        [TestMethod]
        public void ResolvedTree_MergedAttributeValues()
        {
            var root = ParseSource(@"
@viewModel System.Object
<div class='a' class='b' />");
            var value = root.Content.First(n => n.Metadata.Type == typeof(HtmlGenericControl)).GetHtmlAttribute("class");
            Assert.AreEqual("a b", value.CastTo<ResolvedPropertyValue>().Value);
        }


        [TestMethod]
        public void ResolvedTree_MergedAttributeValueAndBinding()
        {
            var root = ParseSource(@"
@viewModel System.String
<div class='a' class='{value: _this}' />");
            var value = root.Content.First(n => n.Metadata.Type == typeof(HtmlGenericControl)).GetHtmlAttribute("class");
            Assert.IsInstanceOfType(value, typeof(ResolvedPropertyBinding));
            var expression = value.CastTo<ResolvedPropertyBinding>().Binding.GetExpression();
            Assert.AreEqual("((\"a\" + \" \") + _this)", expression.ToString());
        }

        [TestMethod]
        public void ResolvedTree_MergedAttributeValueAndResourceBinding()
        {
            var root = ParseSource(@"
@viewModel System.String
<div class='a' class='{resource: _this}' />");
            var value = root.Content.First(n => n.Metadata.Type == typeof(HtmlGenericControl)).GetHtmlAttribute("class");
            Assert.IsInstanceOfType(value, typeof(ResolvedPropertyBinding));
            var expression = value.CastTo<ResolvedPropertyBinding>().Binding.GetExpression();
            Assert.AreEqual("((\"a\" + \" \") + _this)", expression.ToString());
        }

        [TestMethod]
        public void ResolvedTree_MergedAttributeBindings()
        {
            var root = ParseSource(@"
@viewModel System.String
<div class='{value: _this}' class='{value: _this}' />");
            var value = root.Content.First(n => n.Metadata.Type == typeof(HtmlGenericControl)).GetHtmlAttribute("class");
            Assert.IsInstanceOfType(value, typeof(ResolvedPropertyBinding));
            var expression = value.CastTo<ResolvedPropertyBinding>().Binding.GetExpression();
            Assert.AreEqual("((_this + \" \") + _this)", expression.ToString());
        }


        [TestMethod]
        public void ResolvedTree_HtmlPrefixedAttributes()
        {
            var root = ParseSource(@"
@viewModel object
<div html:id='val' />");
            var attr = root.Content.First(n => n.Metadata.Type == typeof(HtmlGenericControl)).GetHtmlAttribute("id");
            Assert.AreEqual("val", attr.CastTo<ResolvedPropertyValue>().Value);
        }

        [TestMethod]
        public void ResolvedTree_RoleView_MultipleRoles()
        {
            var root = ParseSource(@"
@viewModel object
<dot:RoleView Roles='a, b, c, d, e, f'");
            var roles = root.Content.First(n => n.Metadata.Type == typeof(RoleView)).Properties[RoleView.RolesProperty].CastTo<ResolvedPropertyValue>().Value;
            Assert.IsInstanceOfType(roles, typeof(string[]));
            Assert.IsTrue(roles.CastTo<string[]>().SequenceEqual(new[] { "a", "b", "c", "d", "e", "f" }));
        }


        [TestMethod]
        public void ResolvedTree_InnerElementProperty_String()
        {
            var root = ParseSource(@"
@viewModel object
<dot:Button>
    <PostBack.Handlers>
        <cc:ClassWithInnerElementProperty> AHOJ </cc:ClassWithInnerElementProperty>
    </PostBack.Handlers>
</dot:Button>");
            var control = root.Content.First(n => n.Metadata.Type == typeof(Button))
                .Properties[PostBack.HandlersProperty].CastTo<ResolvedPropertyControlCollection>().Controls
                .First(c => c.Metadata.Type == typeof(ClassWithInnerElementProperty));
            Assert.AreEqual(0, control.Content.Count);
            Assert.AreEqual(" AHOJ ", control.Properties[ClassWithInnerElementProperty.PropertyProperty].CastTo<ResolvedPropertyValue>().Value);
        }

        [TestMethod]
        public void ResolvedTree_InnerElementProperty_WhitespaceString()
        {
            var root = ParseSource(@"
@viewModel object
<dot:Button>
    <PostBack.Handlers>
        <cc:ClassWithInnerElementProperty><Property>   </Property></cc:ClassWithInnerElementProperty>
    </PostBack.Handlers>
</dot:Button>");
            var control = root.Content.First(n => n.Metadata.Type == typeof(Button))
                .Properties[PostBack.HandlersProperty].CastTo<ResolvedPropertyControlCollection>().Controls
                .First(c => c.Metadata.Type == typeof(ClassWithInnerElementProperty));
            Assert.AreEqual(0, control.Content.Count);
            Assert.AreEqual("   ", control.Properties[ClassWithInnerElementProperty.PropertyProperty].CastTo<ResolvedPropertyValue>().Value);
        }

        [TestMethod]
        public void ResolvedTree_Invalid_Content()
        {
            var root = ParseSource(@"
@viewModel object
<dot:Button>
    <PostBack.Handlers>
        <cc:ClassWithoutInnerElementProperty> AHOJ </cc:ClassWithoutInnerElementProperty>
    </PostBack.Handlers>
</dot:Button>");
            var control = root.Content.First(n => n.Metadata.Type == typeof(Button))
                .Properties[PostBack.HandlersProperty].CastTo<ResolvedPropertyControlCollection>().Controls
                .First(c => c.Metadata.Type == typeof(ClassWithoutInnerElementProperty));
            Assert.AreEqual(0, control.Content.Count);
        }


        [TestMethod]
        public void ResolvedTree_InnerElementProperty_Controls()
        {
            var root = ParseSource(@"
@viewModel object
<cc:ClassWithDefaultDotvvmControlContent>some text</cc:ClassWithDefaultDotvvmControlContent>");
            var control = root.Content.First(n => n.Metadata.Type == typeof(ClassWithDefaultDotvvmControlContent));
            Assert.AreEqual(0, control.Content.Count);
            Assert.AreEqual(1, control.Properties[ClassWithDefaultDotvvmControlContent.PropertyProperty].CastTo<ResolvedPropertyControlCollection>().Controls.Count);
        }

        [TestMethod]
        public void ResolvedTree_InnerElementProperty_WhitespaceControls()
        {
            var root = ParseSource(@"
@viewModel object
<cc:ClassWithDefaultDotvvmControlContent>


                       </cc:ClassWithDefaultDotvvmControlContent>");
            var control = root.Content.First(n => n.Metadata.Type == typeof(ClassWithDefaultDotvvmControlContent));
            Assert.AreEqual(0, control.Content.Count);
            Assert.IsFalse(control.Properties.ContainsKey(ClassWithDefaultDotvvmControlContent.PropertyProperty));
        }

        [TestMethod]
        public void ResolvedTree_InnerElementVirtualProperty_Controls()
        {
            var root = ParseSource(@"
@viewModel object
<cc:ClassWithDefaultDotvvmControlContent_NoDotvvmProperty>some text</cc:ClassWithDefaultDotvvmControlContent_NoDotvvmProperty>");
            var control = root.Content.First(n => n.Metadata.Type == typeof(ClassWithDefaultDotvvmControlContent_NoDotvvmProperty));
            Assert.AreEqual(0, control.Content.Count);
            Assert.IsTrue(control.Properties.Any(p => p.Value is ResolvedPropertyControlCollection));
        }

        [TestMethod]
        public void ResolvedTree_GridViewWithColumns()
        {
            var root = ParseSource(@"
@viewModel System.Collections.Generic.List<string>
<dot:GridView DataSource='{value: _this}'>
    <dot:GridViewTextColumn HeaderText='' ValueBinding='{value: _this}' />

</dot:GridView>");
            var control = root.Content.First(n => n.Metadata.Type == typeof(GridView));
            Assert.AreEqual(0, control.Content.Count);
            Assert.AreEqual(1, control.Properties[GridView.ColumnsProperty].CastTo<ResolvedPropertyControlCollection>().Controls.Count);
        }


        [TestMethod]
        public void ResolvedTree_ViewModel_GenericType()
        {
            var root = ParseSource(@"@viewModel System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.String, System.Int32>>");
            Assert.AreEqual(typeof(List<Dictionary<string, int>>), root.DataContextTypeStack.DataContextType);
        }

        [TestMethod]
        public void ResolvedTree_ViewModel_InvalidAssemblyQualified()
        {
            var root = ParseSource(@"@viewModel System.String, whatever");
            Assert.IsTrue(root.Directives.Any(d => d.Value.Any(dd => dd.DothtmlNode.HasNodeErrors)));
            Assert.AreEqual(typeof(UnknownTypeSentinel), root.DataContextTypeStack.DataContextType);
        }

        private ResolvedBinding[] GetLiteralBindings(ResolvedContentNode node) =>
            (from c in node.Content.SelectRecursively(c => c.Content)
            where c.Metadata.Type == typeof(Literal)
            let text = c.Properties.GetValueOrDefault(Literal.TextProperty)
            where text is ResolvedPropertyBinding
            select ((ResolvedPropertyBinding)text).Binding).ToArray();

   [TestMethod]
        public void ResolvedTree_ContentDataContextChange()
        {
            var root = ParseSource(@"@viewModel System.String
<cc:ControlWithContentDataContext>
    {{value: _this}}
    {{value: _parent}}
</cc:ControlWithContentDataContext>");
            var types = GetLiteralBindings(root)
                .Select(l => l.ResultType)
                .Select(ResolvedTypeDescriptor.ToSystemType)
                .ToArray();
            Assert.AreEqual(typeof(int), types[0]);
            Assert.AreEqual(typeof(string), types[1]);
        }

        [TestMethod]
        public void ResolvedTree_CustomBindingResolverInDataContext()
        {
            // Demonstrates usage of binding property post-processor registered by DataContext change inside one control.
            // The post-processor just replaces 'abc' binding with 'def'
            var root = ParseSource(@"@viewModel System.String
<cc:ControlWithSpecialBindingsInside>
    {{value: 'abc'}}
    {{value: 'll'}}
</cc:ControlWithSpecialBindingsInside>

{{value: 'abc'}}
");
            var literals =
                (from binding in GetLiteralBindings(root)
                 let expression = binding.GetExpression()
                 let constantExpression = ((ConstantExpression)expression)
                 select constantExpression.Value).ToArray();

            Assert.AreEqual("def", literals[0]);
            Assert.AreEqual("ll", literals[1]);
            Assert.AreEqual("abc", literals[2]);

        }


        [TestMethod]
        public void DefaultViewCompiler_ControlUsageValidator()
        {
            ResolvedControl[] getControls(string controlName)
            {
                var markup = $@"
@viewModel System.Boolean
<cc:{controlName} />
<cc:{controlName} Visible='{{value: _this}}' />
<cc:{controlName} Visible='{{value: _this}}' class='lol' />
";
                return ParseSource(markup)
                    .Content.SelectRecursively(c => c.Content)
                    .Where(c => c.Metadata.Name == controlName)
                    .ToArray();
            }

            var control1 = getControls(nameof(ControlWithValidationRules));
            Assert.IsTrue(control1[0].DothtmlNode.HasNodeErrors);
            Assert.IsTrue(control1[1].DothtmlNode.HasNodeErrors);
            Assert.IsFalse(control1[2].DothtmlNode.HasNodeErrors);

            var control2 = getControls(nameof(ControlWithInheritedRules));
            Assert.IsTrue(control2[0].DothtmlNode.HasNodeErrors);
            Assert.IsTrue(control2[1].DothtmlNode.HasNodeErrors);
            Assert.IsFalse(control2[2].DothtmlNode.HasNodeErrors);


            var control3 = getControls(nameof(ControlWithOverriddenRules));
            Assert.IsFalse(control3[0].DothtmlNode.HasNodeErrors);
            Assert.IsFalse(control3[1].DothtmlNode.HasNodeErrors);
            Assert.IsFalse(control3[2].DothtmlNode.HasNodeErrors);
        }

        private ResolvedTreeRoot ParseSource(string markup, string fileName = "default.dothtml", bool checkErrors = false) =>
            DotvvmTestHelper.ParseResolvedTree(markup, fileName, this.configuration, checkErrors);

    }

    public class DefaultControlResolverTestViewModel
    {
        public List<string> Items { get; set; }
    }
    [ControlMarkupOptions(DefaultContentProperty = nameof(Property))]
    public class ClassWithInnerElementProperty : PostBackHandler
    {
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public string Property
        {
            get { return (string)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }
        public static readonly DotvvmProperty PropertyProperty
            = DotvvmProperty.Register<string, ClassWithInnerElementProperty>(c => c.Property, null);

        protected internal override string ClientHandlerName => null;

        protected internal override Dictionary<string, object> GetHandlerOptions()
        {
            throw new NotImplementedException();
        }
    }

    public class ClassWithoutInnerElementProperty : PostBackHandler
    {
        [MarkupOptions(MappingMode = MappingMode.Attribute)]
        public string Property
        {
            get { return (string)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }
        public static readonly DotvvmProperty PropertyProperty
            = DotvvmProperty.Register<string, ClassWithoutInnerElementProperty>(c => c.Property, null);

        protected internal override string ClientHandlerName => null;

        protected internal override Dictionary<string, object> GetHandlerOptions()
        {
            throw new NotImplementedException();
        }
    }

    [ControlMarkupOptions(DefaultContentProperty = nameof(Property))]
    public class ClassWithDefaultDotvvmControlContent: HtmlGenericControl
    {
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public List<DotvvmControl> Property
        {
            get { return (List<DotvvmControl>)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }
        public static readonly DotvvmProperty PropertyProperty
            = DotvvmProperty.Register<List<DotvvmControl>, ClassWithDefaultDotvvmControlContent>(c => c.Property, null);
    }

    [ControlMarkupOptions(DefaultContentProperty = nameof(Property))]
    public class ClassWithDefaultDotvvmControlContent_NoDotvvmProperty: HtmlGenericControl
    {
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public List<DotvvmControl> Property { get; set; }
    }

    [DataContextChanger]
    public class ControlWithContentDataContext : DotvvmControl
    {
        public class DataContextChanger : DataContextChangeAttribute
        {
            public override int Order => 0;

            public override ITypeDescriptor GetChildDataContextType(ITypeDescriptor dataContext, IDataContextStack controlContextStack, IAbstractControl control, IPropertyDescriptor property = null)
            {
                return new ResolvedTypeDescriptor(typeof(int));
            }

            public override Type GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, DotvvmBindableObject control, DotvvmProperty property = null)
            {
                return typeof(int);
            }
        }
    }

    [DataContextChanger]
    public class ControlWithSpecialBindingsInside : DotvvmControl
    {
        public class DataContextChanger : DataContextStackManipulationAttribute
        {
            public override IDataContextStack ChangeStackForChildren(IDataContextStack original, IAbstractControl control, IPropertyDescriptor property, Func<IDataContextStack, ITypeDescriptor, IDataContextStack> createNewFrame)
            {
                return DataContextStack.Create(ResolvedTypeDescriptor.ToSystemType(original.DataContextType), (DataContextStack)original.Parent,
                    bindingPropertyResolvers: new Delegate[]{
                        new Func<ParsedExpressionBindingProperty, ParsedExpressionBindingProperty>(e => {
                            if (e.Expression.NodeType == ExpressionType.Constant && (string)((ConstantExpression)e.Expression).Value == "abc") return new ParsedExpressionBindingProperty(Expression.Constant("def"));
                            else return e;
                        })
                    });
            }

            public override DataContextStack ChangeStackForChildren(DataContextStack original, DotvvmBindableObject obj, DotvvmProperty property, Func<DataContextStack, Type, DataContextStack> createNewFrame)
            {
                return DataContextStack.Create(original.DataContextType, original.Parent,
                    bindingPropertyResolvers: new Delegate[]{
                        new Func<ParsedExpressionBindingProperty, ParsedExpressionBindingProperty>(e => {
                            if (e.Expression.NodeType == ExpressionType.Constant && (string)((ConstantExpression)e.Expression).Value == "abc") return new ParsedExpressionBindingProperty(Expression.Constant("def"));
                            else return e;
                        })
                    });
            }
        }
    }

    public class ControlWithValidationRules : HtmlGenericControl
    {
        [ControlUsageValidator]
        public static IEnumerable<ControlUsageError> Validate1(ResolvedControl control)
        {
            if (!control.Properties.ContainsKey(VisibleProperty))
                yield return new ControlUsageError($"The Visible property is required");
        }

        [ControlUsageValidator]
        public static IEnumerable<string> Validate2(DothtmlElementNode control)
        {
            if (control.Attributes.Count != 2)
                yield return $"The control has to have exactly two attributes";
        }
    }

    public class ControlWithInheritedRules : ControlWithValidationRules
    {
    }

    public class ControlWithOverriddenRules : ControlWithValidationRules
    {
        [ControlUsageValidator(Override = true)]
        public static IEnumerable<ControlUsageError> Validate(ResolvedControl control)
        {
            yield break;
        }
    }
}
