using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime.JavascriptCompilation
{
    [TestClass]
    public class JsParensInsertionTests
    {
        static JsExpression id(string identifier) => new JsIdentifierExpression(identifier);
        public static void AssertFormatting(string expectedString, JsNode node, bool niceMode = false)
        {
            Assert.AreEqual(expectedString, node.Clone().FormatScript(niceMode));

            foreach (var dd in node.Descendants.OfType<JsExpression>()) {
                var symbol = new JsSymbolicParameter(new CodeSymbolicParameter());
                dd.ReplaceWith(symbol);
                var parametrized = node.Clone().FormatParametrizedScript(niceMode);
                var resolved = parametrized.ToString(o =>
                    o == symbol.Symbol ? CodeParameterAssignment.FromExpression(dd.Clone(), niceMode: niceMode) :
                    throw new Exception());
                Assert.AreEqual(expectedString, resolved, $"Replaced expression: {dd}, pcode: {parametrized.ToDebugString()}");

                var resolved2 = parametrized.AssignParameters(o =>
                    o == symbol.Symbol ? CodeParameterAssignment.FromExpression(dd.Clone(), niceMode: niceMode) :
                    throw new Exception());
                Assert.AreEqual(expectedString, resolved2.ToDefaultString(), $"Replaced expression2: {dd}, pcode: {parametrized.ToDebugString()}");
                symbol.ReplaceWith(dd);
            }
        }

        [TestMethod]
        public void JsParens_SimpleExpression()
        {
            AssertFormatting("a.b(4+b,5)", new JsIdentifierExpression("a").Member("b").Invoke(
                new JsBinaryExpression(new JsLiteral(4), BinaryOperatorType.Plus, new JsIdentifierExpression("b")),
                new JsLiteral(5)));
        }

        [TestMethod]
        public void JsParens_NullCoallesing()
        {
            // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Nullish_coalescing_operator#no_chaining_with_and_or_or_operators
            AssertFormatting("(a&&b)??c", id("a").Binary(BinaryOperatorType.ConditionalAnd, id("b")).Binary(BinaryOperatorType.NullishCoalescing, id("c")));
            AssertFormatting("c??(a&&b)", id("c").Binary(BinaryOperatorType.NullishCoalescing, id("a").Binary(BinaryOperatorType.ConditionalAnd, id("b"))));
            AssertFormatting("a&&(b??c)", id("a").Binary(BinaryOperatorType.ConditionalAnd, id("b").Binary(BinaryOperatorType.NullishCoalescing, id("c"))));
            AssertFormatting("(a??b)&&c", id("a").Binary(BinaryOperatorType.NullishCoalescing, id("b")).Binary(BinaryOperatorType.ConditionalAnd, id("c")));
        }

        [DataTestMethod]
        [DataRow(BinaryOperatorType.Times, BinaryOperatorType.Plus, "a*b+c")]
        [DataRow(BinaryOperatorType.Plus, BinaryOperatorType.Times, "(a+b)*c")]
        [DataRow(BinaryOperatorType.Plus, BinaryOperatorType.Plus, "a+b+c")]
        [DataRow(BinaryOperatorType.Plus, BinaryOperatorType.Minus, "a+b-c")]
        [DataRow(BinaryOperatorType.Minus, BinaryOperatorType.Plus, "a-b+c")]
        [DataRow(BinaryOperatorType.ConditionalAnd, BinaryOperatorType.ConditionalOr, "a&&b||c")]
        [DataRow(BinaryOperatorType.ConditionalOr, BinaryOperatorType.ConditionalAnd, "(a||b)&&c")]
        [DataRow(BinaryOperatorType.BitwiseOr, BinaryOperatorType.ConditionalAnd, "a|b&&c")]
        [DataRow(BinaryOperatorType.BitwiseOr, BinaryOperatorType.BitwiseAnd, "(a|b)&c")]
        [DataRow(BinaryOperatorType.GreaterOrEqual, BinaryOperatorType.ConditionalAnd, "a>=b&&c")]
        [DataRow(BinaryOperatorType.GreaterOrEqual, BinaryOperatorType.ConditionalOr, "a>=b||c")]
        [DataRow(BinaryOperatorType.GreaterOrEqual, BinaryOperatorType.BitwiseAnd, "a>=b&c")]
        [DataRow(BinaryOperatorType.LeftShift, BinaryOperatorType.Plus, "(a<<b)+c")]
        [DataRow(BinaryOperatorType.LeftShift, BinaryOperatorType.Greater, "a<<b>c")]
        [DataRow(BinaryOperatorType.Greater, BinaryOperatorType.Greater, "a>b>c")]
        [DataRow(BinaryOperatorType.Greater, BinaryOperatorType.GreaterOrEqual, "a>b>=c")]
        [DataRow(BinaryOperatorType.Plus, BinaryOperatorType.Sequence, "a+b,c")]
        [DataRow(BinaryOperatorType.Sequence, BinaryOperatorType.Plus, "(a,b)+c")]
        public void JsParens_OperatorPriority(BinaryOperatorType firstOp, BinaryOperatorType secondOp, string expectedJs)
        {
            AssertFormatting(expectedJs,
                new JsIdentifierExpression("a")
                    .Binary(firstOp, new JsIdentifierExpression("b"))
                    .Binary(secondOp, new JsIdentifierExpression("c")));
        }

        [TestMethod]
        public void JsParens_OperatorAssociativity()
        {
            AssertFormatting("a+b+c", new JsBinaryExpression(
                new JsBinaryExpression(new JsIdentifierExpression("a"), BinaryOperatorType.Plus, new JsIdentifierExpression("b")),
                BinaryOperatorType.Plus,
                new JsIdentifierExpression("c")));

            AssertFormatting("c+(a+b)", new JsBinaryExpression(
                new JsIdentifierExpression("c"),
                BinaryOperatorType.Plus,
                new JsBinaryExpression(new JsIdentifierExpression("a"), BinaryOperatorType.Plus, new JsIdentifierExpression("b"))));
        }

        [TestMethod]
        public void JsParens_AssignmentOperator()
        {
            AssertFormatting("a=b=1?2:3", new JsAssignmentExpression(
                new JsIdentifierExpression("a"),
                new JsAssignmentExpression(
                    new JsIdentifierExpression("b"),
                    new JsConditionalExpression(new JsLiteral(1), new JsLiteral(2), new JsLiteral(3)))));
        }
        [TestMethod]
        public void JsParens_AssignmentVsEquals()
        {
            AssertFormatting("(a=b)==null", new JsBinaryExpression(
                new JsAssignmentExpression(
                    new JsIdentifierExpression("a"),
                    new JsIdentifierExpression("b")),
                BinaryOperatorType.Equal,
                new JsIdentifierExpression("null")));
        }

        [TestMethod]
        public void JsParens_MemberAccessOnOperator()
        {
            AssertFormatting("(1+1).a", new JsBinaryExpression(new JsLiteral(1), BinaryOperatorType.Plus, new JsLiteral(1))
                .Member("a"));
        }
        [TestMethod]
        public void JsParens_SequenceInParameters()
        {
            AssertFormatting("a((1,2,3))", 
                new JsIdentifierExpression("a").Invoke(
                    new JsBinaryExpression(new JsBinaryExpression(new JsLiteral(1), BinaryOperatorType.Sequence, new JsLiteral(2)), BinaryOperatorType.Sequence, new JsLiteral(3)))
            );
        }

        [DataTestMethod]
        [DataRow(BinaryOperatorType.Times, "a+b+(a*b)", "a+b+a*b")]
        [DataRow(BinaryOperatorType.Minus, "a+b+(a-b)", "a+b+(a-b)")]
        [DataRow(BinaryOperatorType.BitwiseXOr, "a+b+(a^b)", "a+b+(a^b)")]
        [DataRow(BinaryOperatorType.BitwiseOr, "a+b+(a|b)", "a+b+(a|b)")]
        [DataRow(BinaryOperatorType.BitwiseAnd, "a+b+(a&b)", "a+b+(a&b)")]
        [DataRow(BinaryOperatorType.NullishCoalescing, "a+b+(a??b)", "a+b+(a??b)")]
        [DataRow(BinaryOperatorType.InstanceOf, "a+b+(a instanceof b)", "a+b+(a instanceof b)")]
        public void JsParent_ParametrizedCodeBuilder(BinaryOperatorType binaryOp, string resultParanoidVersion, string resultOptimalVersion)
        {
            Assert.AreEqual(resultParanoidVersion, new ParametrizedCode.Builder { 
                new JsIdentifierExpression("a").FormatParametrizedScript(),
                "+b+",
                new JsBinaryExpression(
                    new JsIdentifierExpression("a"),
                    binaryOp,
                    new JsIdentifierExpression("b")
                ).FormatParametrizedScript()
            }.Build(default).ToDefaultString());

            Assert.AreEqual(resultOptimalVersion, new ParametrizedCode.Builder { 
                new JsIdentifierExpression("a").FormatParametrizedScript(),
                "+b+",
                { new JsBinaryExpression(
                    new JsIdentifierExpression("a"),
                    binaryOp,
                    new JsIdentifierExpression("b")
                ).FormatParametrizedScript(), OperatorPrecedence.Addition }
            }.Build(default).ToDefaultString());
        }
    }
}
