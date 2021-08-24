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
        public void JsParens_OperatorPriority()
        {
            AssertFormatting("a*b+c", new JsBinaryExpression(
                new JsBinaryExpression(new JsIdentifierExpression("a"), BinaryOperatorType.Times, new JsIdentifierExpression("b")),
                BinaryOperatorType.Plus,
                new JsIdentifierExpression("c")));
            AssertFormatting("(a+b)*c", new JsBinaryExpression(
                new JsBinaryExpression(new JsIdentifierExpression("a"), BinaryOperatorType.Plus, new JsIdentifierExpression("b")),
                BinaryOperatorType.Times,
                new JsIdentifierExpression("c")));
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

        [TestMethod]
        public void JsParent_ParametrizedCodeBuilder()
        {
            Assert.AreEqual("a+b+(a*b)", new ParametrizedCode.Builder { 
                new JsIdentifierExpression("a").FormatParametrizedScript(),
                "+b+",
                new JsBinaryExpression(
                    new JsIdentifierExpression("a"),
                    BinaryOperatorType.Times,
                    new JsIdentifierExpression("b")
                ).FormatParametrizedScript()
            }.Build(default).ToDefaultString());
        }
    }
}
