using DotVVM.Framework.Compilation.Javascript.Ast;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Javascript;
using static DotVVM.Framework.Tests.Runtime.JavascriptCompilation.JsParensInsertionTests;

namespace DotVVM.Framework.Tests.Runtime.JavascriptCompilation
{
    [TestClass]
    public class JsFormatterTests
    {
        [TestMethod]
        public void JsFormatter_BinaryOperator()
        {
            AssertFormatting("a+5", new JsBinaryExpression(new JsIdentifierExpression("a"), BinaryOperatorType.Plus, new JsLiteral(5)));
        }

        [TestMethod]
        public void JsFormatter_StringLiteral()
        {
            AssertFormatting("\"\\\"\"", new JsLiteral("\""));
        }

        [TestMethod]
        public void JsFormatter_MemberAccess()
        {
            AssertFormatting("a.b.c", new JsIdentifierExpression("a").Member("b").Member("c"));
        }

        [TestMethod]
        public void JsFormatter_Invocation()
        {
            AssertFormatting("a.b(4,5)", new JsIdentifierExpression("a").Member("b").Invoke(new JsLiteral(4), new JsLiteral(5)));
        }

        [TestMethod]
        public void JsFormatter_Indexer()
        {
            AssertFormatting("a[b]", new JsIdentifierExpression("a").Indexer(new JsIdentifierExpression("b")));
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
            AssertFormatting("function(a){return a+2;}", expr);
            AssertFormatting("function(a) {\n\treturn a + 2;\n}", expr, niceMode: true);
        }

        [TestMethod]
        public void JsFormatter_AssignmentExpression()
        {
            var expr = new JsBinaryExpression(new JsAssignmentExpression(new JsIdentifierExpression("a"), new JsIdentifierExpression("c")), BinaryOperatorType.Equal, new JsIdentifierExpression("b"));
            AssertFormatting("(a=c)==b", expr);
            AssertFormatting("(a = c) == b", expr, niceMode: true);
        }

        [TestMethod]
        public void JsFormatter_NotExpression()
        {
            var expr = new JsIdentifierExpression("a").Unary(UnaryOperatorType.LogicalNot);
            AssertFormatting("!a", expr, niceMode: true);
            AssertFormatting("!a", expr);
        }
        [TestMethod]
        public void JsFormatter_UnaryExpression()
        {
            var expr = new JsBinaryExpression(
                new JsIdentifierExpression("a").Unary(UnaryOperatorType.Increment, isPrefix: false),
                BinaryOperatorType.Plus,
                new JsIdentifierExpression("a").Unary(UnaryOperatorType.Increment, isPrefix: true))
                .Unary(UnaryOperatorType.LogicalNot);
            AssertFormatting("!(a++ + ++a)", expr);
            AssertFormatting("!(a++ + ++a)", expr, niceMode: true);
        }

        [TestMethod]
        public void JsFormatter_KeywordUnaryExpression()
        {
            var expr = new JsBinaryExpression(
                new JsIdentifierExpression("a").Unary(UnaryOperatorType.TypeOf),
                BinaryOperatorType.Plus,
                new JsLiteral(0).Unary(UnaryOperatorType.Void).Unary(UnaryOperatorType.Minus));
            AssertFormatting("typeof a+-void 0", expr);
            AssertFormatting("typeof a + -void 0", expr, niceMode: true);
        }

        [TestMethod]
        public void JsFormatter_LessThanOperator()
        {
            var expr = new JsBinaryExpression(
                new JsIdentifierExpression("a"),
                BinaryOperatorType.LessOrEqual,
                new JsIdentifierExpression("b"));
            AssertFormatting("a<=b", expr);
            AssertFormatting("a <= b", expr, niceMode: true);
        }

        [TestMethod]
        public void JsFormatter_ObjectExpression()
        {
            var expr = new JsObjectExpression(
                new JsObjectProperty("a", new JsObjectExpression(new JsObjectProperty("c", new JsLiteral(2)))),
                new JsObjectProperty("baa", new JsLiteral(null)));
            AssertFormatting("{a:{c:2},baa:null}", expr);
            AssertFormatting("{\n\ta: {c: 2},\n\tbaa: null\n}", expr, niceMode: true);
        }
        [TestMethod]
        public void JsFormatter_Await()
        {
            var expr = new JsIdentifierExpression("a").Await().Member("x");
            AssertFormatting("(await a).x", expr, niceMode: false);
            AssertFormatting("(await a).x", expr, niceMode: true);
        }
        [TestMethod]
        public void JsFormatter_AsyncFunction()
        {
            var expr = new JsFunctionExpression(
                new JsIdentifier[0],
                new JsIdentifierExpression("a").Await().Member("x").Return().AsBlock(),
                isAsync: true
            );
            AssertFormatting("async function(){return (await a).x;}", expr, niceMode: false);
            AssertFormatting("async function() {\n\treturn (await a).x;\n}", expr, niceMode: true);
        }
        [TestMethod]
        public void JsFormatter_AsyncArrowFunction()
        {
            var expr = new JsArrowFunctionExpression(
                new JsIdentifier[0],
                new JsIdentifierExpression("a").Await().Member("x"),
                isAsync: true
            );
            AssertFormatting("async ()=>(await a).x", expr, niceMode: false);
            AssertFormatting("async () => (await a).x", expr, niceMode: true);
        }
        [TestMethod]
        public void JsFormatter_ArrowFunctionAndVariable()
        {
            var expr = new JsArrowFunctionExpression(
                new JsIdentifier[0],
                new JsStatement[] {
                    new JsVariableDefStatement("a", new JsLiteral(1)),
                    new JsVariableDefStatement("b"),
                    new JsIdentifierExpression("a").Member("x").Return(),
                    new JsExpressionStatement(new JsIdentifierExpression("a").Invoke())
                }.AsBlock()
            );
            AssertFormatting("()=>{let a=1;let b;return a.x;a();}", expr, niceMode: false);
            AssertFormatting("() => {\n\tlet a = 1;\n\tlet b;\n\treturn a.x;\n\ta();\n}", expr, niceMode: true);
        }

        [TestMethod]
        public void JsFormatter_2ArrowFunctions()
        {
            var expr = new JsArrowFunctionExpression(
                new JsIdentifier[] { new JsIdentifier("a") },
                new JsArrowFunctionExpression(
                    new JsIdentifier[] { new JsIdentifier("b") },
                    new JsBinaryExpression(
                        new JsIdentifierExpression("a"),
                        BinaryOperatorType.Plus,
                        new JsIdentifierExpression("b")
                    )
                )
            );
            AssertFormatting("(a)=>(b)=>a+b", expr, niceMode: false);
            AssertFormatting("(a) => (b) => a + b", expr, niceMode: true);
        }
    }
}
