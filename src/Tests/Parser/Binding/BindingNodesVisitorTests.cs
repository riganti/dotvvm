using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Tests.Parser.Binding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Parser.Binding
{
    [TestClass]
    public class BindingNodesVisitorTests
    {
        private readonly BindingParserNodeFactory bindingParserNodeFactory = new BindingParserNodeFactory();

        [TestMethod]
        public void BindingParserNodeVisitor_Parse_AllVisitsImplemented()
        {
            var tree = bindingParserNodeFactory.Parse("Aaaa<bbb,ccc>.ddd<eee>.fff + 3*4 - ggg(hhh<iii>())");

            var testVisitor = new TestGenericVisitor();
            var result = testVisitor.Visit(tree);

            Assert.AreEqual("Yes", result);
        }

        [TestMethod]
        public void BindingParserNodeVisitor_ParseDirectiveTypeName_AllVisitsImplemented()
        {
            var tree = bindingParserNodeFactory.ParseDirectiveTypeName("Aaaa<bbb,ccc>.ddd<eee>.fff, eee");

            var testVisitor = new TestGenericVisitor();
            var result = testVisitor.Visit(tree);

            Assert.AreEqual("Yes", result);
        }

        [TestMethod]
        public void BindingParserNodeVisitor_ParseImportDirective_AllVisitsImplemented()
        {
            var tree = bindingParserNodeFactory.ParseImportDirective("alias = Aaaa<bbb,ccc>.ddd<eee>.fff");

            var testVisitor = new TestGenericVisitor();
            var result = testVisitor.Visit(tree);

            Assert.AreEqual("Yes", result);
        }

        [TestMethod]
        public void BindingParserNodeVisitor_ParseMultiExpression_AllVisitsImplemented()
        {
            var tree = bindingParserNodeFactory.ParseMultiExpression("Aaaa<bbb,ccc>.ddd<eee>.fff + 3*4 - ggg(hhh<iii>()) :: jjj() ++ kkk.hhh");

            var testVisitor = new TestGenericVisitor();
            var result = testVisitor.Visit(tree);

            Assert.AreEqual("Yes", result);
        }

        [TestMethod]
        public void BindingParserNodeVisitor_ParsePropertyDeclarationDirectiveInitializer_AllVisitsImplemented()
        {
            var tree = bindingParserNodeFactory.ParseArrayInitializer("[ [ \"Test\", \"Test2\" ], \"Test2\", 10 ]");

            var testVisitor = new TestGenericVisitor();
            var result = testVisitor.Visit(tree);

            Assert.AreEqual("Yes", result);
        }

        private class TestGenericVisitor : BindingParserNodeVisitor<string>
        {
            protected override string DefaultVisit(BindingParserNode node)
            {
                return "Yes";
            }
        }
    }
}
