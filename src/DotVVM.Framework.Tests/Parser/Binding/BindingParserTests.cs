using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using DotVVM.Framework.Runtime.Compilation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BindingParser = DotVVM.Framework.Compilation.Parser.Binding.Parser.BindingParser;

namespace DotVVM.Framework.Tests.Parser.Binding
{
    [TestClass]
    public class BindingParserTests
    {

        [TestMethod]
        public void BindingParser_TrueLiteral_Valid()
        {
            var result = Parse("true");

            Assert.IsInstanceOfType(result, typeof(LiteralExpressionBindingParserNode));
            Assert.AreEqual(true, ((LiteralExpressionBindingParserNode)result).Value);
        }

        [TestMethod]
        public void BindingParser_FalseLiteral_WhiteSpaceOnEnd_Valid()
        {
            var result = Parse("false  \t ");

            Assert.IsInstanceOfType(result, typeof(LiteralExpressionBindingParserNode));
            Assert.AreEqual(false, ((LiteralExpressionBindingParserNode)result).Value);
        }

        [TestMethod]
        public void BindingParser_NullLiteral_WhiteSpaceOnStart_Valid()
        {
            var result = Parse(" null");

            Assert.IsInstanceOfType(result, typeof(LiteralExpressionBindingParserNode));
            Assert.AreEqual(null, ((LiteralExpressionBindingParserNode)result).Value);
        }

        [TestMethod]
        public void BindingParser_SimpleProperty_Arithmetics_Valid()
        {
            var result = Parse("a +b");

            var binaryOperator = (BinaryOperatorBindingParserNode)result;
            Assert.AreEqual("a", ((IdentifierNameBindingParserNode) binaryOperator.FirstExpression).Name);
            Assert.AreEqual("b", ((IdentifierNameBindingParserNode) binaryOperator.SecondExpression).Name);
            Assert.AreEqual(BindingTokenType.AddOperator, binaryOperator.Operator);
        }

        [TestMethod]
        public void BindingParser_MemberAccess_Arithmetics_Valid()
        {
            var result = Parse("a.c - b");

            var binaryOperator = (BinaryOperatorBindingParserNode)result;
            var first = (MemberAccessBindingParserNode)binaryOperator.FirstExpression;
            Assert.AreEqual("a", ((IdentifierNameBindingParserNode)first.TargetExpression).Name);
            Assert.AreEqual("c", first.MemberNameExpression.Name);
            Assert.AreEqual("b", ((IdentifierNameBindingParserNode)binaryOperator.SecondExpression).Name);
            Assert.AreEqual(BindingTokenType.SubtractOperator, binaryOperator.Operator);
        }

        [TestMethod]
        public void BindingParser_NestedMemberAccess_Number_ArithmeticsOperatorPrecendence_Valid()
        {
            var result = Parse("a.c.d * b + 3.14");

            var binaryOperator = (BinaryOperatorBindingParserNode)result;
            Assert.AreEqual(BindingTokenType.AddOperator, binaryOperator.Operator);

            var first = (BinaryOperatorBindingParserNode)binaryOperator.FirstExpression;
            Assert.AreEqual(BindingTokenType.MultiplyOperator, first.Operator);
            var acd = (MemberAccessBindingParserNode)first.FirstExpression;
            var ac = (MemberAccessBindingParserNode)acd.TargetExpression;
            Assert.AreEqual("a", ((IdentifierNameBindingParserNode)ac.TargetExpression).Name);
            Assert.AreEqual("c", ac.MemberNameExpression.Name);
            Assert.AreEqual("d", acd.MemberNameExpression.Name);
            Assert.AreEqual("b", ((IdentifierNameBindingParserNode)first.SecondExpression).Name);

            var second = (LiteralExpressionBindingParserNode) binaryOperator.SecondExpression;
            Assert.AreEqual(3.14, second.Value);
        }

        [TestMethod]
        public void BindingParser_ArithmeticOperatorPrecedence_Parenthesis_Valid()
        {
            var result = Parse("a + b * c - d / (e + 2)");

            var root = (BinaryOperatorBindingParserNode)result;
            Assert.AreEqual(BindingTokenType.AddOperator, root.Operator);

            var a = (IdentifierNameBindingParserNode)root.FirstExpression;
            Assert.AreEqual("a", a.Name);

            var subtract = (BinaryOperatorBindingParserNode)root.SecondExpression;
            Assert.AreEqual(BindingTokenType.SubtractOperator, subtract.Operator);

            var multiply = (BinaryOperatorBindingParserNode)subtract.FirstExpression;
            Assert.AreEqual(BindingTokenType.MultiplyOperator, multiply.Operator);
            Assert.AreEqual("b", ((IdentifierNameBindingParserNode)multiply.FirstExpression).Name);
            Assert.AreEqual("c", ((IdentifierNameBindingParserNode)multiply.SecondExpression).Name);

            var divide = (BinaryOperatorBindingParserNode)subtract.SecondExpression;
            Assert.AreEqual(BindingTokenType.DivideOperator, divide.Operator);
            Assert.AreEqual("d", ((IdentifierNameBindingParserNode)divide.FirstExpression).Name);

            var parenthesis = (ParenthesizedExpressionBindingParserNode) divide.SecondExpression;
            var addition2 = (BinaryOperatorBindingParserNode) parenthesis.InnerExpression;
            Assert.AreEqual(BindingTokenType.AddOperator, addition2.Operator);
            Assert.AreEqual("e", ((IdentifierNameBindingParserNode)addition2.FirstExpression).Name);
            Assert.AreEqual(2, ((LiteralExpressionBindingParserNode)addition2.SecondExpression).Value);
        }

        [TestMethod]
        public void BindingParser_ArithmeticOperatorChain_Valid()
        {
            var result = Parse("a + b + c");

            var root = (BinaryOperatorBindingParserNode)result;
            Assert.AreEqual(BindingTokenType.AddOperator, root.Operator);

            var a = (IdentifierNameBindingParserNode)root.FirstExpression;
            Assert.AreEqual("a", a.Name);

            var add = (BinaryOperatorBindingParserNode)root.SecondExpression;
            Assert.AreEqual(BindingTokenType.AddOperator, add.Operator);

            Assert.AreEqual("b", ((IdentifierNameBindingParserNode)add.FirstExpression).Name);
            Assert.AreEqual("c", ((IdentifierNameBindingParserNode)add.SecondExpression).Name);
        }


        [TestMethod]
        public void BindingParser_MemberAccess_ArrayIndexer_Chain_Valid()
        {
            var result = Parse("a[b + -1](c).d[e ?? f]");

            var root = (ArrayAccessBindingParserNode)result;

            var ef = (BinaryOperatorBindingParserNode) root.ArrayIndexExpression;
            Assert.AreEqual(BindingTokenType.NullCoalescingOperator, ef.Operator);
            Assert.AreEqual("e", ((IdentifierNameBindingParserNode)ef.FirstExpression).Name);
            Assert.AreEqual("f", ((IdentifierNameBindingParserNode)ef.SecondExpression).Name);

            var d = (MemberAccessBindingParserNode) root.TargetExpression;
            Assert.AreEqual("d", d.MemberNameExpression.Name);

            var functionCall = (FunctionCallBindingParserNode) d.TargetExpression;
            Assert.AreEqual(1, functionCall.ArgumentExpressions.Count);
            Assert.AreEqual("c", ((IdentifierNameBindingParserNode)functionCall.ArgumentExpressions[0]).Name);

            var firstArray = (ArrayAccessBindingParserNode) functionCall.TargetExpression;
            var add = (BinaryOperatorBindingParserNode) firstArray.ArrayIndexExpression;
            Assert.AreEqual(BindingTokenType.AddOperator, add.Operator);
            Assert.AreEqual("b", ((IdentifierNameBindingParserNode)add.FirstExpression).Name);
            Assert.AreEqual(1, ((LiteralExpressionBindingParserNode)((UnaryOperatorBindingParserNode)add.SecondExpression).InnerExpression).Value);
            Assert.AreEqual(BindingTokenType.SubtractOperator, ((UnaryOperatorBindingParserNode)add.SecondExpression).Operator);

            Assert.AreEqual("a", ((IdentifierNameBindingParserNode)firstArray.TargetExpression).Name);
        }

        [TestMethod]
        public void BindingParser_StringLiteral_Valid()
        {
            var result = Parse("\"help\\\"help\"");
            Assert.AreEqual("help\"help", ((LiteralExpressionBindingParserNode)result).Value);
        }

        [TestMethod]
        public void BindingParser_StringLiteral_SingleQuotes_Valid()
        {
            var result = Parse("'help\\nhelp'");
            Assert.AreEqual("help\nhelp", ((LiteralExpressionBindingParserNode)result).Value);
        }

        [TestMethod]
        public void BindingParser_ConditionalOperator_Valid()
        {
            var result = Parse("a ? !b : c");
            var condition = (ConditionalExpressionBindingParserNode) result;
            Assert.AreEqual("a", ((IdentifierNameBindingParserNode)condition.ConditionExpression).Name);
            Assert.AreEqual("b", ((IdentifierNameBindingParserNode)((UnaryOperatorBindingParserNode)condition.TrueExpression).InnerExpression).Name);
            Assert.AreEqual(BindingTokenType.NotOperator, ((UnaryOperatorBindingParserNode)condition.TrueExpression).Operator);
            Assert.AreEqual("c", ((IdentifierNameBindingParserNode)condition.FalseExpression).Name);
        }

        [TestMethod]
        public void BindingParser_Empty_Invalid()
        {
            var result = Parse("");
            Assert.IsTrue(((IdentifierNameBindingParserNode)result).HasNodeErrors);
        }

        [TestMethod]
        public void BindingParser_Whitespace_Invalid()
        {
            var result = Parse(" ");
            Assert.IsTrue(((IdentifierNameBindingParserNode)result).HasNodeErrors);
            Assert.AreEqual(0, result.StartPosition);
            Assert.AreEqual(1, result.Length);
        }

        [TestMethod]
        public void BindingParser_Incomplete_Expression()
        {
            var result = Parse(" (a +");
            Assert.IsTrue(((ParenthesizedExpressionBindingParserNode)result).HasNodeErrors);
            Assert.AreEqual(0, result.StartPosition);
            Assert.AreEqual(5, result.Length);

            var inner = (BinaryOperatorBindingParserNode)((ParenthesizedExpressionBindingParserNode) result).InnerExpression;
            Assert.AreEqual(BindingTokenType.AddOperator, inner.Operator);
            Assert.AreEqual("a", ((IdentifierNameBindingParserNode)inner.FirstExpression).Name);
            Assert.AreEqual("", ((IdentifierNameBindingParserNode)inner.SecondExpression).Name);
            Assert.IsTrue(inner.SecondExpression.HasNodeErrors);
            Assert.AreEqual(2, inner.FirstExpression.Length);
            Assert.AreEqual(0, inner.SecondExpression.Length);
        }

        [TestMethod]
        public void BindingParser_IntLiteral_Valid()
        {
            var result = (LiteralExpressionBindingParserNode)Parse("12");
            Assert.IsInstanceOfType(result.Value, typeof(int));
            Assert.AreEqual(result.Value, 12);
        }

        [TestMethod]
        public void BindingParser_DoubleLiteral_Valid()
        {
            var result = (LiteralExpressionBindingParserNode)Parse("12.45");
            Assert.IsInstanceOfType(result.Value, typeof(double));
            Assert.AreEqual(result.Value, 12.45);
        }

        [TestMethod]
        public void BindingParser_FloatLiteral_Valid()
        {
            var result = (LiteralExpressionBindingParserNode)Parse("42f");
            Assert.IsInstanceOfType(result.Value, typeof(float));
            Assert.AreEqual(result.Value, 42f);
        }

        [TestMethod]
        public void BindingParser_LongLiteral_Valid()
        {
            var result = (LiteralExpressionBindingParserNode)Parse(long.MaxValue.ToString());
            Assert.IsInstanceOfType(result.Value, typeof(long));
            Assert.AreEqual(result.Value, long.MaxValue);
        }

        [TestMethod]
        public void BindingParser_LongForcedLiteral_Valid()
        {
            var result = (LiteralExpressionBindingParserNode)Parse("42L");
            Assert.IsInstanceOfType(result.Value, typeof(long));
            Assert.AreEqual(result.Value, 42L);
        }

        [TestMethod]
        public void BindingParser_MethodInvokeOnValue_Valid()
        {
            var result = (FunctionCallBindingParserNode)Parse("42.ToString()");
            var memberAccess = (MemberAccessBindingParserNode)result.TargetExpression;
            Assert.AreEqual(memberAccess.MemberNameExpression.Name, "ToString");
            Assert.AreEqual(((LiteralExpressionBindingParserNode)memberAccess.TargetExpression).Value, 42);
            Assert.AreEqual(result.ArgumentExpressions.Count, 0);
        }

        [TestMethod]
        public void BindingParser_AssignOperator_Valid()
        {
            var result = (BinaryOperatorBindingParserNode)Parse("a = b");
            Assert.AreEqual(BindingTokenType.AssignOperator, result.Operator);

            var first = (IdentifierNameBindingParserNode)result.FirstExpression;
            Assert.AreEqual("a", first.Name);

            var second = (IdentifierNameBindingParserNode)result.SecondExpression;
            Assert.AreEqual("b", second.Name);
        }

        [TestMethod]
        public void BindingParser_AssignOperator_Incomplete()
        {
            var result = (BinaryOperatorBindingParserNode)Parse("a = ");
            Assert.AreEqual(BindingTokenType.AssignOperator, result.Operator);

            var first = (IdentifierNameBindingParserNode)result.FirstExpression;
            Assert.AreEqual("a", first.Name);

            var second = (IdentifierNameBindingParserNode)result.SecondExpression;
            Assert.IsTrue(second.HasNodeErrors);
        }

        [TestMethod]
        public void BindingParser_AssignOperator_Incomplete1()
        {
            var result = (BinaryOperatorBindingParserNode)Parse("=");
            Assert.AreEqual(BindingTokenType.AssignOperator, result.Operator);

            var first = (IdentifierNameBindingParserNode)result.FirstExpression;
            Assert.IsTrue(first.HasNodeErrors);

            var second = (IdentifierNameBindingParserNode)result.SecondExpression;
            Assert.IsTrue(second.HasNodeErrors);
        }


        private static BindingParserNode Parse(string expression)
        {
            var tokenizer = new BindingTokenizer();
            tokenizer.Tokenize(new StringReader(expression));
            var parser = new BindingParser();
            parser.Tokens = tokenizer.Tokens;
            return parser.ReadExpression();
        }
    }
}
