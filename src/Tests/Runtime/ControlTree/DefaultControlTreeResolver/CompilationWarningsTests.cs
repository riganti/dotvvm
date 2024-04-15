using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Controls.Infrastructure;


namespace DotVVM.Framework.Tests.Runtime.ControlTree
{
    [TestClass]
    public class CompilationWarningsTests : DefaultControlTreeResolverTestsBase
    {
        public (string warning, string tokens)[] GetWarnings(string dothtml)
        {
            var tree = ParseSource(dothtml, checkErrors: false);
            return tree.DothtmlNode
                       .EnumerateNodes()
                       .SelectMany(n => n.NodeWarnings.Select(w => (w, string.Join(" ", n.Tokens.Select(t => t.Text)))))
                       .ToArray();
        }

        [TestMethod]
        public void CompilationWarning_ValidationTargetPrimitiveType()
        {
            var warnings = GetWarnings($$"""
                @viewModel {{typeof(TestViewModel)}}
                <div Validation.Target={value: BoolProp}></div>
            """);
            Assert.AreEqual(1, warnings.Length);
            StringAssert.Contains(warnings[0].warning, "Validation.Target should be bound to a complex object instead of 'bool'");
            Assert.AreEqual("BoolProp", warnings[0].tokens.Trim());
        }

        [TestMethod]
        public void CompilationWarning_JsComponentNoModules()
        {
            var warnings = GetWarnings($$"""
                @viewModel {{typeof(TestViewModel)}}
                <js:Test />
            """);
            Assert.AreEqual(1, warnings.Length);
            StringAssert.Contains(warnings[0].warning, "This view does not have any view modules registered");
            Assert.AreEqual("Test", warnings[0].tokens.Trim());
        }

        [TestMethod]
        public void CompilationWarning_JsComponentFine()
        {
            XAssert.Empty(GetWarnings($$"""
                @viewModel {{typeof(TestViewModel)}}
                @js dotvvm.internal
                <js:Test />
            """));
            XAssert.Empty(GetWarnings($$"""
                @viewModel {{typeof(TestViewModel)}}
                <js:Test Global />
            """));
        }

        [DataTestMethod]
        [DataRow("TestViewModel2")]
        [DataRow("VMArray")]
        [DataRow("0")]
        public void CompilationWarning_ValidationTargetPrimitiveType_Negative(string property)
        {
            var warnings = GetWarnings($$"""
                @viewModel {{typeof(TestViewModel)}}
                <div Validation.Target={value: {{property}}}></div>
            """);
            XAssert.Empty(warnings);
        }

        [TestMethod]
        public void DefaultViewCompiler_NonExistentPropertyWarning()
        {
           var markup = $@"
@viewModel System.Boolean
<dot:Button TestProperty=AA Visble={{value: false}} normal-attribute=AA Click={{command: 0}} />
";
            var button = ParseSource(markup)
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Button));

            var elementNode = (DothtmlElementNode)button.DothtmlNode;
            var attribute1 = elementNode.Attributes.Single(a => a.AttributeName == "TestProperty");
            var attribute2 = elementNode.Attributes.Single(a => a.AttributeName == "normal-attribute");
            var attribute3 = elementNode.Attributes.Single(a => a.AttributeName == "Visble");

            Assert.AreEqual(0, attribute2.AttributeNameNode.NodeWarnings.Count(), attribute2.AttributeNameNode.NodeWarnings.StringJoin(", "));
            Assert.AreEqual("HTML attribute name 'TestProperty' should not contain uppercase letters. Did you intent to use a DotVVM property instead?", attribute1.AttributeNameNode.NodeWarnings.Single());
            Assert.AreEqual("HTML attribute name 'Visble' should not contain uppercase letters. Did you mean Visible, or another DotVVM property?", attribute3.AttributeNameNode.NodeWarnings.Single());
        }

        [TestMethod]
        public void DefaultViewCompiler_NonExistentPropertyWarning_PrefixedGroup()
        {
           var markup = $@"
@viewModel System.Boolean
<dot:HierarchyRepeater ItemClass=AA ItemIncludeInPage=false />
";
            var repeater = ParseSource(markup)
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(HierarchyRepeater));

            var elementNode = (DothtmlElementNode)repeater.DothtmlNode;
            var attribute1 = elementNode.Attributes.Single(a => a.AttributeName == "ItemClass");
            var attribute2 = elementNode.Attributes.Single(a => a.AttributeName == "ItemIncludeInPage");

            Assert.AreEqual(0, attribute1.AttributeNameNode.NodeWarnings.Count(), attribute1.AttributeNameNode.NodeWarnings.StringJoin(", "));
            Assert.AreEqual("HTML attribute name 'IncludeInPage' should not contain uppercase letters. Did you intent to use a DotVVM property instead?", XAssert.Single(attribute2.AttributeNameNode.NodeWarnings));
        }

        [TestMethod]
        public void DefaultViewCompiler_NonExistentPropertyWarning_InnerElement()
        {
           var markup = $@"
@viewModel bool
<dot:Repeater>
    <EmptyDataTemplate>empty</EmptyDataTemplate>
    <SepratrorTemplate> --- </SepratrorTemplate>
    <TextBox Text=AA />
    test
    <SeparatorTemplate> ---- </SeparatorTemplate>
</dot:Repeater>
";
            var repeater = ParseSource(markup)
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Repeater));

            var elementNode = (DothtmlElementNode)repeater.DothtmlNode;
            var correctTemplateElement = elementNode.Content.OfType<DothtmlElementNode>().Single(e => e.TagName == "EmptyDataTemplate");
            var mistakeTemplateElement = elementNode.Content.OfType<DothtmlElementNode>().Single(e => e.TagName == "SepratrorTemplate");
            var mistakeTextBoxElement = elementNode.Content.OfType<DothtmlElementNode>().Single(e => e.TagName == "TextBox");
            var lateTemplate = elementNode.Content.OfType<DothtmlElementNode>().Single(e => e.TagName == "SeparatorTemplate");

            XAssert.Empty(correctTemplateElement.TagNameNode.NodeWarnings);
            Assert.AreEqual("HTML element name 'SepratrorTemplate' should not contain uppercase letters. Did you mean SeparatorTemplate, or another DotVVM property?", XAssert.Single(mistakeTemplateElement.TagNameNode.NodeWarnings));
            Assert.AreEqual("HTML element name 'TextBox' should not contain uppercase letters. Did you mean dot:CheckBox, dot:ListBox, dot:TextBox, or another DotVVM control?", XAssert.Single(mistakeTextBoxElement.TagNameNode.NodeWarnings));
            Assert.AreEqual("This element looks like an inner element property Repeater.SeparatorTemplate, but it isn't, because it is prefixed by other content ('<SepratrorTemplate>')).", XAssert.Single(lateTemplate.TagNameNode.NodeWarnings));
        }

        [TestMethod]
        public void DefaultViewCompiler_DisallowedContentControlType()
        {
           var markup = $@"
@viewModel System.Collections.Generic.IEnumerable<string>
<dot:GridView DataSource={{value: _this}}>
    <EmtyDataTemplate>empty</EmtyDataTemplate>
    <dot:GridViewTemplateColumn>test</dot:GridViewTemplateColumn>
    <dot:TextBox Text={{value: _this}} />
</dot:GridView>
";
            var repeater = ParseSource(markup)
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(GridView));

            var elementNode = (DothtmlElementNode)repeater.DothtmlNode;
            var fine = elementNode.Content.OfType<DothtmlElementNode>().Single(e => e.TagName == "GridViewTemplateColumn");
            var unallowedType = elementNode.Content.OfType<DothtmlElementNode>().Single(e => e.TagName == "TextBox");
            var mistypedTemplate = elementNode.Content.OfType<DothtmlElementNode>().Single(e => e.TagName == "EmtyDataTemplate");

            XAssert.Empty(fine.TagNameNode.NodeWarnings);
            XAssert.Empty(fine.TagNameNode.NodeErrors);

            Assert.AreEqual("Control type DotVVM.Framework.Controls.TextBox can't be used in a property of type DotVVM.Framework.Controls.GridViewColumn.", XAssert.Single(unallowedType.TagNameNode.NodeErrors));
            Assert.AreEqual("Control type DotVVM.Framework.Controls.HtmlGenericControl can't be used in a property of type DotVVM.Framework.Controls.GridViewColumn. Did you mean EmptyDataTemplate, or another DotVVM property?", XAssert.Single(mistypedTemplate.TagNameNode.NodeErrors));
            Assert.AreEqual("HTML element name 'EmtyDataTemplate' should not contain uppercase letters. Did you mean EmptyDataTemplate, or another DotVVM property?", XAssert.Single(mistypedTemplate.TagNameNode.NodeWarnings));
        }


        [TestMethod]
        public void DefaultViewCompiler_UnsupportedCallSite_ResourceBinding_Warning()
        {
            var markup = @"
@viewModel System.DateTime
{{resource: _this.ToBrowserLocalTime()}}
";
            var literal = ParseSource(markup)
                .Content.SelectRecursively(c => c.Content)
                .Single(c => c.Metadata.Type == typeof(Literal));

            Assert.AreEqual(1, literal.DothtmlNode.NodeWarnings.Count());
            Assert.AreEqual("Evaluation of method \"ToBrowserLocalTime\" on server-side may yield unexpected results.", literal.DothtmlNode.NodeWarnings.First());
        }

        [TestMethod]
        public void DefaultViewCompiler_DotvvmView_Used_As_Control_Warning()
        {
            var files = new FakeMarkupFileLoader();
            files.MarkupFiles["TestControl.dothtml"] = """
                @viewModel object
                test
            """;
            var config = DotvvmTestHelper.CreateConfiguration(s => {
                s.AddSingleton<IMarkupFileLoader>(files);
            });
            config.Markup.AddMarkupControl("cc", "TestControl", "TestControl.dothtml");

            var markup = DotvvmTestHelper.ParseResolvedTree("""
                @viewModel object
                <cc:TestControl Styles.Tag=this />
            """, configuration: config);
            var control = markup.Content.SelectRecursively(c => c.Content).Single(c => c.Properties.ContainsKey(Styles.TagProperty));
            Assert.AreEqual(typeof(DotvvmView), control.Metadata.Type);
            var element = (DothtmlElementNode)control.DothtmlNode;
            XAssert.Contains("The markup control <cc:TestControl> has a baseType DotvvmView", element.TagNameNode.NodeWarnings.First());
            Assert.AreEqual(1, element.TagNameNode.NodeWarnings.Count());
        }
    }

}
