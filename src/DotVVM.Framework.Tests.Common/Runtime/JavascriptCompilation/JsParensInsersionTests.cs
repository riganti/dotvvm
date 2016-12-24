using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.Runtime.JavascriptCompilation
{
    [TestClass]
    public class JsParensInsersionTests
    {
        [TestMethod]
        public void JsParens_SimpleExpression()
        {
            Assert.AreEqual("a.b(4+b,5)", new JsIdentifierExpression("a").Member("b").Invoke(
                new JsBinaryExpression(new JsLiteral(4), BinaryOperatorType.Plus, new JsIdentifierExpression("b")),
                new JsLiteral(5)).FormatScript());
        }

        [TestMethod]
        public void JsParens_OperatorPriority()
        {
            Assert.AreEqual("a*b+c", new JsBinaryExpression(
                new JsBinaryExpression(new JsIdentifierExpression("a"), BinaryOperatorType.Times, new JsIdentifierExpression("b")),
                BinaryOperatorType.Plus,
                new JsIdentifierExpression("c")).FormatScript());
            Assert.AreEqual("(a+b)*c", new JsBinaryExpression(
                new JsBinaryExpression(new JsIdentifierExpression("a"), BinaryOperatorType.Plus, new JsIdentifierExpression("b")),
                BinaryOperatorType.Times,
                new JsIdentifierExpression("c")).FormatScript());
        }

        [TestMethod]
        public void JsParens_OperatorAsociativity()
        {
            Assert.AreEqual("a+b+c", new JsBinaryExpression(
                new JsBinaryExpression(new JsIdentifierExpression("a"), BinaryOperatorType.Plus, new JsIdentifierExpression("b")),
                BinaryOperatorType.Plus,
                new JsIdentifierExpression("c")).FormatScript());

            Assert.AreEqual("c+(a+b)", new JsBinaryExpression(
                new JsIdentifierExpression("c"),
                BinaryOperatorType.Plus,
                new JsBinaryExpression(new JsIdentifierExpression("a"), BinaryOperatorType.Plus, new JsIdentifierExpression("b"))).FormatScript());
        }

        [TestMethod]
        public void JsParens_MemberAccessOnOperator()
        {
            Assert.AreEqual("(1+1).a", new JsBinaryExpression(new JsLiteral(1), BinaryOperatorType.Plus, new JsLiteral(1))
                .Member("a").FormatScript());
        }
    }
}
