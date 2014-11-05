using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redwood.Framework.Binding;
using Redwood.Framework.Configuration;
using Redwood.Framework.Controls;
using Redwood.Framework.Parser;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Tests.Runtime
{
    [TestClass]
    public class DefaultViewCompilerTests
    {

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_ElementWithAttributeProperty()
        {
            var markup = @"test <rw:Literal Text='test' />";
            var page = CompileMarkup(markup);

            Assert.IsInstanceOfType(page, typeof(RedwoodView));
            Assert.AreEqual(2, page.Children.Count);

            Assert.IsInstanceOfType(page.Children[0], typeof(Literal));
            Assert.AreEqual("test ", ((Literal)page.Children[0]).Text);
            Assert.IsInstanceOfType(page.Children[1], typeof(Literal));
            Assert.AreEqual("test", ((Literal)page.Children[1]).Text);
        }

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_ElementWithBindingProperty()
        {
            var markup = @"test <rw:Literal Text='{value: FirstName}' />";
            var page = CompileMarkup(markup);

            Assert.IsInstanceOfType(page, typeof(RedwoodView));
            Assert.AreEqual(2, page.Children.Count);

            Assert.IsInstanceOfType(page.Children[0], typeof(Literal));
            Assert.AreEqual("test ", ((Literal)page.Children[0]).Text);
            Assert.IsInstanceOfType(page.Children[1], typeof(Literal));
            
            var binding = ((Literal)page.Children[1]).GetBinding(Literal.TextProperty) as ValueBindingExpression;
            Assert.IsNotNull(binding);
            Assert.AreEqual("FirstName", binding.Expression);
        }

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_BindingInText()
        {
            var markup = @"test {{value: FirstName}}";
            var page = CompileMarkup(markup);

            Assert.IsInstanceOfType(page, typeof(RedwoodView));
            Assert.AreEqual(2, page.Children.Count);

            Assert.IsInstanceOfType(page.Children[0], typeof(Literal));
            Assert.AreEqual("test ", ((Literal)page.Children[0]).Text);
            Assert.IsInstanceOfType(page.Children[1], typeof(Literal));

            var binding = ((Literal)page.Children[1]).GetBinding(Literal.TextProperty) as ValueBindingExpression;
            Assert.IsNotNull(binding);
            Assert.AreEqual("FirstName", binding.Expression);
        }

        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_NestedControls()
        {
            var markup = @"<rw:Placeholder>test <rw:Literal /></rw:Placeholder>";
            var page = CompileMarkup(markup);

            Assert.IsInstanceOfType(page, typeof(RedwoodView));
            Assert.AreEqual(1, page.Children.Count);

            Assert.IsInstanceOfType(page.Children[0], typeof(Placeholder));

            Assert.AreEqual(2, page.Children[0].Children.Count);
            Assert.IsTrue(page.Children[0].Children.All(c => c is Literal));
            Assert.AreEqual("test ", ((Literal)page.Children[0].Children[0]).Text);
            Assert.AreEqual("", ((Literal)page.Children[0].Children[1]).Text);
        }


        [TestMethod]
        public void DefaultViewCompiler_CodeGeneration_Template()
        {
            var markup = @"<rw:Repeater>
    <ItemTemplate>
        <p>This is a test</p>
    </ItemTemplate>
</rw:Repeater>";
            var page = CompileMarkup(markup);

            Assert.IsInstanceOfType(page, typeof(RedwoodView));
            Assert.AreEqual(1, page.Children.Count);

            Assert.IsInstanceOfType(page.Children[0], typeof(Repeater));

            RedwoodControl placeholder = new Placeholder();
            ((Repeater)page.Children[0]).ItemTemplate.BuildContent(placeholder);
            placeholder = placeholder.Children[0];
            
            Assert.AreEqual(3, placeholder.Children.Count);
            Assert.IsTrue(string.IsNullOrWhiteSpace(((Literal)placeholder.Children[0]).Text));
            Assert.AreEqual("p", ((HtmlGenericControl)placeholder.Children[1]).TagName);
            Assert.AreEqual("This is a test", ((Literal)placeholder.Children[1].Children[0]).Text);
            Assert.IsTrue(string.IsNullOrWhiteSpace(((Literal)placeholder.Children[2]).Text));
        }





        private static RedwoodControl CompileMarkup(string markup)
        {
            var compiler = new DefaultViewCompiler(new DefaultControlResolver(new RedwoodConfiguration()));
            var factory = compiler.CompileView(new StringReader(markup), "file", "assembly", "ns", "c");
            return factory();
        }
    }
}
