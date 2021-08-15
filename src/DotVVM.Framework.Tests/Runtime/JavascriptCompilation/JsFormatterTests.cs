using DotVVM.Framework.Compilation.Javascript.Ast;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Javascript;

namespace DotVVM.Framework.Tests.Runtime.JavascriptCompilation
{
    [TestClass]
    public class JsFormatterTests
    {
        [TestMethod]
        public void JsFormatter_BinaryOperator()
        {
            Assert.AreEqual("a+5", new JsBinaryExpression(new JsIdentifierExpression("a"), BinaryOperatorType.Plus, new JsLiteral(5)).FormatScript());
        }

        [TestMethod]
        public void JsFormatter_StringLiteral()
        {
            Assert.AreEqual("\"\\\"\"", new JsLiteral("\"").FormatScript());
        }

        [TestMethod]
        public void JsFormatter_MemberAccess()
        {
            Assert.AreEqual("a.b.c", new JsIdentifierExpression("a").Member("b").Member("c").FormatScript());
        }

        [TestMethod]
        public void JsFormatter_Invocation()
        {
            Assert.AreEqual("a.b(4,5)", new JsIdentifierExpression("a").Member("b").Invoke(new JsLiteral(4), new JsLiteral(5)).FormatScript());
        }

        [TestMethod]
        public void JsFormatter_Indexer()
        {
            Assert.AreEqual("a[b]", new JsIdentifierExpression("a").Indexer(new JsIdentifierExpression("b")).FormatScript());
        }

        [TestMethod]
        public void JsFormatter_SymbolicParameter_Global()
        {
            var symbol = new CodeSymbolicParameter();
            Assert.AreEqual("a+global",
                new JsBinaryExpression(new JsMemberAccessExpression(new JsSymbolicParameter(symbol), "a"), BinaryOperatorType.Plus,
                    new JsSymbolicParameter(symbol))
                .FormatParametrizedScript().ToString(o => o == symbol ? CodeParameterAssignment.FromExpression(new JsIdentifierExpression("global"), isGlobalContext: true) :
                                                          throw new Exception()));
        }

        [TestMethod]
        public void JsFormatter_FunctionExpression()
        {
            var expr = new JsFunctionExpression(new[] { new JsIdentifier("a") }, new JsBlockStatement(new JsReturnStatement(new JsBinaryExpression(new JsIdentifierExpression("a"), BinaryOperatorType.Plus, new JsLiteral(2)))));
            Assert.AreEqual("function(a){return a+2;}", expr.FormatScript());
            Assert.AreEqual("function(a) {\n\treturn a + 2;\n}", expr.FormatScript(niceMode: true));
        }

        [TestMethod]
        public void JsFormatter_AssignmentExpression()
        {
            var expr = new JsBinaryExpression(new JsAssignmentExpression(new JsIdentifierExpression("a"), new JsIdentifierExpression("c")), BinaryOperatorType.Equal, new JsIdentifierExpression("b"));
            Assert.AreEqual("(a=c)==b", expr.FormatScript());
            Assert.AreEqual("(a = c) == b", expr.FormatScript(niceMode: true));
        }

        [TestMethod]
        public void JsFormatter_UnaryExpression()
        {
            var expr = new JsBinaryExpression(
                new JsIdentifierExpression("a").Unary(UnaryOperatorType.Increment, isPrefix: false),
                BinaryOperatorType.Plus,
                new JsIdentifierExpression("a").Unary(UnaryOperatorType.Increment, isPrefix: true))
                .Unary(UnaryOperatorType.LogicalNot);
            Assert.AreEqual("!(a++ + ++a)", expr.FormatScript());
            Assert.AreEqual("!(a++ + ++a)", expr.FormatScript(niceMode: true));
        }

        [TestMethod]
        public void JsFormatter_KeywordUnaryExpression()
        {
            var expr = new JsBinaryExpression(
                new JsIdentifierExpression("a").Unary(UnaryOperatorType.TypeOf),
                BinaryOperatorType.Plus,
                new JsLiteral(0).Unary(UnaryOperatorType.Void).Unary(UnaryOperatorType.Minus));
            Assert.AreEqual("typeof a+-void 0", expr.FormatScript());
            Assert.AreEqual("typeof a + -void 0", expr.FormatScript(niceMode: true));
        }

        [TestMethod]
        public void JsFormatter_LessThanOperator()
        {
            var expr = new JsBinaryExpression(
                new JsIdentifierExpression("a"),
                BinaryOperatorType.LessOrEqual,
                new JsIdentifierExpression("b"));
            Assert.AreEqual("a<=b", expr.FormatScript());
            Assert.AreEqual("a <= b", expr.FormatScript(niceMode: true));
        }

        [TestMethod]
        public void JsFormatter_ObjectExpression()
        {
            var expr = new JsObjectExpression(
                new JsObjectProperty("a", new JsObjectExpression(new JsObjectProperty("c", new JsLiteral(2)))),
                new JsObjectProperty("baa", new JsLiteral(null)));
            Assert.AreEqual("{a:{c:2},baa:null}", expr.FormatScript());
            Assert.AreEqual("{\n\ta: {c: 2},\n\tbaa: null\n}", expr.FormatScript(niceMode: true));
        }
        [TestMethod]
        public void JsFormatter_Await()
        {
            var expr = new JsIdentifierExpression("a").Await().Member("x");
            Assert.AreEqual("(await a).x", expr.FormatScript(niceMode: false));
            Assert.AreEqual("(await a).x", expr.FormatScript(niceMode: true));
        }
        [TestMethod]
        public void JsFormatter_AsyncFunction()
        {
            var expr = new JsFunctionExpression(
                new JsIdentifier[0],
                new JsIdentifierExpression("a").Await().Member("x").Return().AsBlock(),
                isAsync: true
            );
            Assert.AreEqual("async function(){return (await a).x;}", expr.FormatScript(niceMode: false));
            Assert.AreEqual("async function() {\n\treturn (await a).x;\n}", expr.FormatScript(niceMode: true));
        }
    }
}
