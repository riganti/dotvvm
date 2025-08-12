﻿using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Tests.Parser.Binding
{
    [TestClass]
    public class BindingParserTests
    {
        private readonly BindingParserNodeFactory bindingParserNodeFactory = new BindingParserNodeFactory();

        [TestMethod]
        public void BindingParser_TrueLiteral_Valid()
        {
            var result = bindingParserNodeFactory.Parse("true");

            Assert.IsInstanceOfType(result, typeof(LiteralExpressionBindingParserNode));
            Assert.AreEqual(true, ((LiteralExpressionBindingParserNode)result).Value);
        }

        [TestMethod]
        public void BindingParser_FalseLiteral_WhiteSpaceOnEnd_Valid()
        {
            var result = bindingParserNodeFactory.Parse("false  \t ");

            Assert.IsInstanceOfType(result, typeof(LiteralExpressionBindingParserNode));
            Assert.AreEqual(false, ((LiteralExpressionBindingParserNode)result).Value);
        }

        [TestMethod]
        public void BindingParser_NullLiteral_WhiteSpaceOnStart_Valid()
        {
            var result = bindingParserNodeFactory.Parse(" null");

            Assert.IsInstanceOfType(result, typeof(LiteralExpressionBindingParserNode));
            Assert.AreEqual(null, ((LiteralExpressionBindingParserNode)result).Value);
        }

        [TestMethod]
        public void BindingParser_SimpleProperty_Arithmetics_Valid()
        {
            var result = bindingParserNodeFactory.Parse("a +b");

            var binaryOperator = (BinaryOperatorBindingParserNode)result;
            Assert.AreEqual("a", ((IdentifierNameBindingParserNode)binaryOperator.FirstExpression).Name);
            Assert.AreEqual("b", ((IdentifierNameBindingParserNode)binaryOperator.SecondExpression).Name);
            Assert.AreEqual(BindingTokenType.AddOperator, binaryOperator.Operator);
        }

        [TestMethod]
        public void BindingParser_MemberAccess_Arithmetics_Valid()
        {
            var result = bindingParserNodeFactory.Parse("a.c - b");

            var binaryOperator = (BinaryOperatorBindingParserNode)result;
            var first = (MemberAccessBindingParserNode)binaryOperator.FirstExpression;
            Assert.AreEqual("a", ((IdentifierNameBindingParserNode)first.TargetExpression).Name);
            Assert.AreEqual("c", first.MemberNameExpression.Name);
            Assert.AreEqual("b", ((IdentifierNameBindingParserNode)binaryOperator.SecondExpression).Name);
            Assert.AreEqual(BindingTokenType.SubtractOperator, binaryOperator.Operator);
        }

        [TestMethod]
        public void BindingParser_NestedMemberAccess_Number_ArithmeticsOperatorPrecedence_Valid()
        {
            var result = bindingParserNodeFactory.Parse("a.c.d * b + 3.14");

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

            var second = (LiteralExpressionBindingParserNode)binaryOperator.SecondExpression;
            Assert.AreEqual(3.14, second.Value);
        }

        [TestMethod]
        public void BindingParser_ArithmeticOperatorPrecedence_Parenthesis_Valid()
        {
            var result = bindingParserNodeFactory.Parse("a + b * c - d / (e + 2)");

            var root = (BinaryOperatorBindingParserNode)result;
            Assert.AreEqual(BindingTokenType.SubtractOperator, root.Operator);

            var add = (BinaryOperatorBindingParserNode)root.FirstExpression;
            Assert.AreEqual(BindingTokenType.AddOperator, add.Operator);

            var a = (IdentifierNameBindingParserNode)add.FirstExpression;
            Assert.AreEqual("a", a.Name);

            var multiply = (BinaryOperatorBindingParserNode)add.SecondExpression;
            Assert.AreEqual(BindingTokenType.MultiplyOperator, multiply.Operator);
            Assert.AreEqual("b", ((IdentifierNameBindingParserNode)multiply.FirstExpression).Name);
            Assert.AreEqual("c", ((IdentifierNameBindingParserNode)multiply.SecondExpression).Name);

            var divide = (BinaryOperatorBindingParserNode)root.SecondExpression;
            Assert.AreEqual(BindingTokenType.DivideOperator, divide.Operator);
            Assert.AreEqual("d", ((IdentifierNameBindingParserNode)divide.FirstExpression).Name);

            var parenthesis = (ParenthesizedExpressionBindingParserNode)divide.SecondExpression;
            var addition2 = (BinaryOperatorBindingParserNode)parenthesis.InnerExpression;
            Assert.AreEqual(BindingTokenType.AddOperator, addition2.Operator);
            Assert.AreEqual("e", ((IdentifierNameBindingParserNode)addition2.FirstExpression).Name);
            Assert.AreEqual(2, ((LiteralExpressionBindingParserNode)addition2.SecondExpression).Value);
        }

        [TestMethod]
        public void BindingParser_ArithmeticOperatorChain_Valid()
        {
            var result = bindingParserNodeFactory.Parse("a + b + c");

            var root = (BinaryOperatorBindingParserNode)result;
            Assert.AreEqual(BindingTokenType.AddOperator, root.Operator);

            var c = (IdentifierNameBindingParserNode)root.SecondExpression;
            Assert.AreEqual("c", c.Name);

            var add = (BinaryOperatorBindingParserNode)root.FirstExpression;
            Assert.AreEqual(BindingTokenType.AddOperator, add.Operator);

            Assert.AreEqual("a", ((IdentifierNameBindingParserNode)add.FirstExpression).Name);
            Assert.AreEqual("b", ((IdentifierNameBindingParserNode)add.SecondExpression).Name);
        }

        [TestMethod]
        public void BindingParser_BinaryOperatorExclusiveOr_Valid()
        {
            var result = bindingParserNodeFactory.Parse("a ^ b");

            var root = (BinaryOperatorBindingParserNode)result;
            Assert.AreEqual(BindingTokenType.ExclusiveOrOperator, root.Operator);

            var a = (IdentifierNameBindingParserNode)root.FirstExpression;
            var b = (IdentifierNameBindingParserNode)root.SecondExpression;
            Assert.AreEqual("a", a.Name);
            Assert.AreEqual("b", b.Name);
        }


        [TestMethod]
        public void BindingParser_MemberAccess_ArrayIndexer_Chain_Valid()
        {
            var result = bindingParserNodeFactory.Parse("a[b + -1](c).d[e ?? f]");

            var root = (ArrayAccessBindingParserNode)result;

            var ef = (BinaryOperatorBindingParserNode)root.ArrayIndexExpression;
            Assert.AreEqual(BindingTokenType.NullCoalescingOperator, ef.Operator);
            Assert.AreEqual("e", ((IdentifierNameBindingParserNode)ef.FirstExpression).Name);
            Assert.AreEqual("f", ((IdentifierNameBindingParserNode)ef.SecondExpression).Name);

            var d = (MemberAccessBindingParserNode)root.TargetExpression;
            Assert.AreEqual("d", d.MemberNameExpression.Name);

            var functionCall = (FunctionCallBindingParserNode)d.TargetExpression;
            Assert.AreEqual(1, functionCall.ArgumentExpressions.Count);
            Assert.AreEqual("c", ((IdentifierNameBindingParserNode)functionCall.ArgumentExpressions[0]).Name);

            var firstArray = (ArrayAccessBindingParserNode)functionCall.TargetExpression;
            var add = (BinaryOperatorBindingParserNode)firstArray.ArrayIndexExpression;
            Assert.AreEqual(BindingTokenType.AddOperator, add.Operator);
            Assert.AreEqual("b", ((IdentifierNameBindingParserNode)add.FirstExpression).Name);
            Assert.AreEqual(1, ((LiteralExpressionBindingParserNode)((UnaryOperatorBindingParserNode)add.SecondExpression).InnerExpression).Value);
            Assert.AreEqual(BindingTokenType.SubtractOperator, ((UnaryOperatorBindingParserNode)add.SecondExpression).Operator);

            Assert.AreEqual("a", ((IdentifierNameBindingParserNode)firstArray.TargetExpression).Name);
        }

        [TestMethod]
        public void BindingParser_StringLiteral_Valid()
        {
            var result = bindingParserNodeFactory.Parse("\"help\\\"help\"");
            Assert.AreEqual("help\"help", ((LiteralExpressionBindingParserNode)result).Value);
        }

        [TestMethod]
        public void BindingParser_InterpolatedString_BinaryOperationInterpolation()
        {
            var result = bindingParserNodeFactory.Parse("$'{'x' + 'y' + 'z'}'") as InterpolatedStringBindingParserNode;
            Assert.AreEqual("{0}", result.Format);
            Assert.IsFalse(result.HasNodeErrors);
            Assert.AreEqual(1, result.Arguments.Count);
            Assert.AreEqual(typeof(BinaryOperatorBindingParserNode), result.Arguments[0].GetType());
            Assert.AreEqual("\"x\" + \"y\" + \"z\"", result.Arguments[0].ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_InterpolatedString_MultipleInterpolations()
        {
            var result = bindingParserNodeFactory.Parse("$\"Hello {Argument1} with {Argument2}!\"") as InterpolatedStringBindingParserNode;
            Assert.AreEqual("Hello {0} with {1}!", result.Format);
            Assert.IsFalse(result.HasNodeErrors);
            Assert.AreEqual(2, result.Arguments.Count);
            Assert.AreEqual("Argument1", ((SimpleNameBindingParserNode)result.Arguments[0]).Name);
            Assert.AreEqual(9, ((SimpleNameBindingParserNode)result.Arguments[0]).StartPosition);
            Assert.AreEqual("Argument1".Length, ((SimpleNameBindingParserNode)result.Arguments[0]).Length);
            Assert.AreEqual(26, ((SimpleNameBindingParserNode)result.Arguments[1]).StartPosition);
            Assert.AreEqual("Argument2".Length, ((SimpleNameBindingParserNode)result.Arguments[1]).Length);
            Assert.AreEqual("Argument2", ((SimpleNameBindingParserNode)result.Arguments[1]).Name);
        }

        [TestMethod]
        public void BindingParser_InterpolatedString_Expression_WithWhitespaces_StartPositions()
        {
            var result = bindingParserNodeFactory.Parse("$'ABC{ x }'") as InterpolatedStringBindingParserNode;
            Assert.AreEqual(0, result.StartPosition);
            Assert.AreEqual(6, result.Arguments.First().StartPosition /* $' x ' */);
            Assert.AreEqual(3, result.Arguments.First().Tokens.Count);
            Assert.AreEqual(BindingTokenType.WhiteSpace, result.Arguments.First().Tokens.First().Type /* 1 whitespace */);
            Assert.AreEqual(BindingTokenType.Identifier, result.Arguments.First().Tokens.Skip(1).First().Type /* identifier x */);
            Assert.AreEqual(BindingTokenType.WhiteSpace, result.Arguments.First().Tokens.Skip(2).First().Type /* 1 whitespace */);
        }

        [TestMethod]
        public void BindingParser_InterpolatedString_NestedExpressions_StartPositions()
        {
            var result = bindingParserNodeFactory.Parse("$'ABC{$'DEF{'GHI!'}'}'") as InterpolatedStringBindingParserNode;
            Assert.AreEqual(0, result.StartPosition);
            Assert.AreEqual(6, result.Arguments.First().StartPosition /* $'DEF{'GHI!'}' */);
            Assert.AreEqual(12, (result.Arguments.First() as InterpolatedStringBindingParserNode).Arguments.First().StartPosition /* 'GHI!'' */);
        }

        [TestMethod]
        public void BindingParser_InterpolatedString_ComplexNestedExpressions_StartPositions()
        {
            var result = bindingParserNodeFactory.Parse("Method($'ABC{Method($'DEF{'GHI!'}')}')") as FunctionCallBindingParserNode;
            Assert.AreEqual(0, result.StartPosition);

            var interpolationOuter = result.ArgumentExpressions.First() as InterpolatedStringBindingParserNode;
            Assert.AreEqual(7, interpolationOuter.StartPosition);
            var innerMethodCall = interpolationOuter.Arguments.First() as FunctionCallBindingParserNode;
            Assert.AreEqual(13, innerMethodCall.StartPosition);
            var interpolationInner = innerMethodCall.ArgumentExpressions.First() as InterpolatedStringBindingParserNode;
            Assert.AreEqual(20, interpolationInner.StartPosition);
        }

        [TestMethod]
        [DataRow("$'{DateProperty:dd/MM/yyyy}'", "DateProperty:dd/MM/yyyy", "{0:dd/MM/yyyy}")]
        [DataRow("$'{IntProperty:####}'", "IntProperty:####", "{0:####}")]
        public void BindingParser_InterpolatedString_WithFormattingComponent_Valid(string interpolatedString, string interpolation, string formatOptions)
        {
            var result = bindingParserNodeFactory.Parse(interpolatedString) as InterpolatedStringBindingParserNode;
            Assert.IsFalse(result.HasNodeErrors);
            Assert.AreEqual(1, result.Arguments.Count);
            Assert.AreEqual(typeof(FormattedBindingParserNode), result.Arguments.First().GetType());
            Assert.AreEqual(formatOptions, ((FormattedBindingParserNode)result.Arguments.First()).Format);
            Assert.AreEqual(3, ((FormattedBindingParserNode)result.Arguments.First()).StartPosition);
            Assert.AreEqual(interpolation.Length, ((FormattedBindingParserNode)result.Arguments.First()).Length);
        }

        [TestMethod]
        public void BindingParser_StringLiteral_SingleQuotes_Valid()
        {
            var result = bindingParserNodeFactory.Parse("'help\\nhelp'");
            Assert.AreEqual("help\nhelp", ((LiteralExpressionBindingParserNode)result).Value);
        }

        [TestMethod]
        public void BindingParser_ConditionalOperator_Valid()
        {
            var result = bindingParserNodeFactory.Parse("a ? !b : c");
            var condition = (ConditionalExpressionBindingParserNode)result;
            Assert.AreEqual("a", ((IdentifierNameBindingParserNode)condition.ConditionExpression).Name);
            Assert.AreEqual("b", ((IdentifierNameBindingParserNode)((UnaryOperatorBindingParserNode)condition.TrueExpression).InnerExpression).Name);
            Assert.AreEqual(BindingTokenType.NotOperator, ((UnaryOperatorBindingParserNode)condition.TrueExpression).Operator);
            Assert.AreEqual("c", ((IdentifierNameBindingParserNode)condition.FalseExpression).Name);
        }

        [TestMethod]
        public void BindingParser_Empty_Invalid()
        {
            var result = bindingParserNodeFactory.Parse("");
            Assert.IsTrue(((IdentifierNameBindingParserNode)result).HasNodeErrors);
        }

        [TestMethod]
        public void BindingParser_Whitespace_Invalid()
        {
            var result = bindingParserNodeFactory.Parse(" ");
            Assert.IsTrue(((IdentifierNameBindingParserNode)result).HasNodeErrors);
            Assert.AreEqual(0, result.StartPosition);
            Assert.AreEqual(1, result.Length);
        }

        [TestMethod]
        public void BindingParser_Incomplete_Expression()
        {
            var result = bindingParserNodeFactory.Parse(" (a +");
            Assert.IsTrue(((ParenthesizedExpressionBindingParserNode)result).HasNodeErrors);
            Assert.AreEqual(0, result.StartPosition);
            Assert.AreEqual(5, result.Length);

            var inner = (BinaryOperatorBindingParserNode)((ParenthesizedExpressionBindingParserNode)result).InnerExpression;
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
            var result = (LiteralExpressionBindingParserNode)bindingParserNodeFactory.Parse("12");
            Assert.IsInstanceOfType(result.Value, typeof(int));
            Assert.AreEqual(result.Value, 12);
        }

        [TestMethod]
        public void BindingParser_DoubleLiteral_Valid()
        {
            var result = (LiteralExpressionBindingParserNode)bindingParserNodeFactory.Parse("12.45");
            Assert.IsInstanceOfType(result.Value, typeof(double));
            Assert.AreEqual(result.Value, 12.45);
        }

        [TestMethod]
        public void BindingParser_FloatLiteral_Valid()
        {
            var result = (LiteralExpressionBindingParserNode)bindingParserNodeFactory.Parse("42f");
            Assert.IsInstanceOfType(result.Value, typeof(float));
            Assert.AreEqual(result.Value, 42f);
        }

        [TestMethod]
        public void BindingParser_LongLiteral_Valid()
        {
            var result = (LiteralExpressionBindingParserNode)bindingParserNodeFactory.Parse(long.MaxValue.ToString());
            Assert.IsInstanceOfType(result.Value, typeof(long));
            Assert.AreEqual(result.Value, long.MaxValue);
        }

        [TestMethod]
        public void BindingParser_LongForcedLiteral_Valid()
        {
            var result = (LiteralExpressionBindingParserNode)bindingParserNodeFactory.Parse("42L");
            Assert.IsInstanceOfType(result.Value, typeof(long));
            Assert.AreEqual(result.Value, 42L);
        }

        [TestMethod]
        public void BindingParser_MethodInvokeOnValue_Valid()
        {
            var result = (FunctionCallBindingParserNode)bindingParserNodeFactory.Parse("42.ToString()");
            var memberAccess = (MemberAccessBindingParserNode)result.TargetExpression;
            Assert.AreEqual(memberAccess.MemberNameExpression.Name, "ToString");
            Assert.AreEqual(((LiteralExpressionBindingParserNode)memberAccess.TargetExpression).Value, 42);
            Assert.AreEqual(result.ArgumentExpressions.Count, 0);
        }

        [TestMethod]
        public void BindingParser_AssignOperator_Valid()
        {
            var result = (BinaryOperatorBindingParserNode)bindingParserNodeFactory.Parse("a = b");
            Assert.AreEqual(BindingTokenType.AssignOperator, result.Operator);

            var first = (IdentifierNameBindingParserNode)result.FirstExpression;
            Assert.AreEqual("a", first.Name);

            var second = (IdentifierNameBindingParserNode)result.SecondExpression;
            Assert.AreEqual("b", second.Name);
        }

        [TestMethod]
        public void BindingParser_AssignOperator_ValueWithUnaryMinus()
        {
            var result = (BinaryOperatorBindingParserNode)bindingParserNodeFactory.Parse("a=-5");
            Assert.AreEqual(BindingTokenType.AssignOperator, result.Operator);

            var first = (IdentifierNameBindingParserNode)result.FirstExpression;
            Assert.AreEqual("a", first.Name);

            var second = (UnaryOperatorBindingParserNode)result.SecondExpression;
            Assert.AreEqual(BindingTokenType.SubtractOperator, second.Operator);

            var literal = (LiteralExpressionBindingParserNode)second.InnerExpression;
            Assert.AreEqual(5, (int)literal.Value);
        }

        [TestMethod]
        public void BindingParser_AssignOperator_ValueWithUnaryNegation()
        {
            var result = (BinaryOperatorBindingParserNode)bindingParserNodeFactory.Parse("a=!b");
            Assert.AreEqual(BindingTokenType.AssignOperator, result.Operator);

            var first = (IdentifierNameBindingParserNode)result.FirstExpression;
            Assert.AreEqual("a", first.Name);

            var second = (UnaryOperatorBindingParserNode)result.SecondExpression;
            Assert.AreEqual(BindingTokenType.NotOperator, second.Operator);

            var identifier = (IdentifierNameBindingParserNode)second.InnerExpression;
            Assert.AreEqual("b", identifier.Name);
        }

        [TestMethod]
        public void BindingParser_AssignOperator_Incomplete()
        {
            var result = (BinaryOperatorBindingParserNode)bindingParserNodeFactory.Parse("a = ");
            Assert.AreEqual(BindingTokenType.AssignOperator, result.Operator);

            var first = (IdentifierNameBindingParserNode)result.FirstExpression;
            Assert.AreEqual("a", first.Name);

            var second = (IdentifierNameBindingParserNode)result.SecondExpression;
            Assert.IsTrue(second.HasNodeErrors);
        }

        [TestMethod]
        public void BindingParser_AssignOperator_Incomplete1()
        {
            var result = (BinaryOperatorBindingParserNode)bindingParserNodeFactory.Parse("=");
            Assert.AreEqual(BindingTokenType.AssignOperator, result.Operator);

            var first = (IdentifierNameBindingParserNode)result.FirstExpression;
            Assert.IsTrue(first.HasNodeErrors);

            var second = (IdentifierNameBindingParserNode)result.SecondExpression;
            Assert.IsTrue(second.HasNodeErrors);
        }

        [TestMethod]
        public void BindingParser_PlusAssign_Valid()
        {
            var parser = bindingParserNodeFactory.SetupParser("_root.MyCoolProperty += 3");
            var node = parser.ReadExpression();

            Assert.IsTrue(parser.OnEnd());
            Assert.IsInstanceOfType(node, typeof(BinaryOperatorBindingParserNode));

            var binOpNode = node as BinaryOperatorBindingParserNode;

            Assert.IsInstanceOfType(binOpNode.FirstExpression, typeof(MemberAccessBindingParserNode));
            Assert.IsInstanceOfType(binOpNode.SecondExpression, typeof(LiteralExpressionBindingParserNode));
            Assert.AreEqual(BindingTokenType.UnsupportedOperator, binOpNode.Operator);
            Assert.AreEqual("Unsupported operator: +=", binOpNode.NodeErrors[0]);
        }

        [TestMethod]
        public void BindingParser_UnsupportedBinaryOperator_Valid()
        {
            var parser = bindingParserNodeFactory.SetupParser("_root.MyCoolProperty += _this.Number1 * Multiplikator");
            var node = parser.ReadExpression();

            Assert.IsTrue(parser.OnEnd());
            Assert.IsInstanceOfType(node, typeof(BinaryOperatorBindingParserNode));

            var plusAssignNode = node as BinaryOperatorBindingParserNode;

            CheckBinaryOperatorNodeType<MemberAccessBindingParserNode, BinaryOperatorBindingParserNode>(plusAssignNode, BindingTokenType.UnsupportedOperator);

            var multiplyNode = plusAssignNode.SecondExpression as BinaryOperatorBindingParserNode;

            CheckBinaryOperatorNodeType<MemberAccessBindingParserNode, IdentifierNameBindingParserNode>(multiplyNode, BindingTokenType.MultiplyOperator);

            Assert.IsTrue(plusAssignNode.NodeErrors.Any());
        }

        [TestMethod]
        public void BindingParser_UnsupportedUnaryOperators_Valid()
        {
            var parser = bindingParserNodeFactory.SetupParser("MyCoolProperty = ^&Number1 + ^&Number2 * ^&Number3");
            var node = parser.ReadExpression();

            Assert.IsTrue(parser.OnEnd());
            Assert.IsInstanceOfType(node, typeof(BinaryOperatorBindingParserNode));

            var assignNode = node as BinaryOperatorBindingParserNode;

            CheckBinaryOperatorNodeType<IdentifierNameBindingParserNode, BinaryOperatorBindingParserNode>(assignNode, BindingTokenType.AssignOperator);

            var plusNode = assignNode.SecondExpression as BinaryOperatorBindingParserNode;

            CheckBinaryOperatorNodeType<UnaryOperatorBindingParserNode, BinaryOperatorBindingParserNode>(plusNode, BindingTokenType.AddOperator);

            var multiplyNode = plusNode.SecondExpression as BinaryOperatorBindingParserNode;

            CheckBinaryOperatorNodeType<UnaryOperatorBindingParserNode, UnaryOperatorBindingParserNode>(multiplyNode, BindingTokenType.MultiplyOperator);

            var Number1Unary = plusNode.FirstExpression as UnaryOperatorBindingParserNode;
            var Number2Unary = multiplyNode.FirstExpression as UnaryOperatorBindingParserNode;
            var Number3Unary = multiplyNode.SecondExpression as UnaryOperatorBindingParserNode;

            CheckUnaryOperatorNodeType<IdentifierNameBindingParserNode>(Number1Unary, BindingTokenType.UnsupportedOperator);
            CheckUnaryOperatorNodeType<IdentifierNameBindingParserNode>(Number2Unary, BindingTokenType.UnsupportedOperator);
            CheckUnaryOperatorNodeType<IdentifierNameBindingParserNode>(Number3Unary, BindingTokenType.UnsupportedOperator);

            Assert.IsTrue(Number1Unary.NodeErrors.Any());
            Assert.IsTrue(Number2Unary.NodeErrors.Any());
            Assert.IsTrue(Number3Unary.NodeErrors.Any());
        }

        [TestMethod]
        public void BindingParser_BinaryAndUnaryUnsupportedOperators_Valid()
        {
            var parser = bindingParserNodeFactory.SetupParser("MyCoolProperty += ^& Number1");
            var node = parser.ReadExpression();

            Assert.IsTrue(parser.OnEnd());
            Assert.IsInstanceOfType(node, typeof(BinaryOperatorBindingParserNode));

            var plusAssignNode = node as BinaryOperatorBindingParserNode;

            CheckBinaryOperatorNodeType<IdentifierNameBindingParserNode, UnaryOperatorBindingParserNode>(plusAssignNode, BindingTokenType.UnsupportedOperator);

            var Number1Unary = plusAssignNode.SecondExpression as UnaryOperatorBindingParserNode;

            CheckUnaryOperatorNodeType<IdentifierNameBindingParserNode>(Number1Unary, BindingTokenType.UnsupportedOperator);

            Assert.IsTrue(Number1Unary.NodeErrors.Any());
        }

        [TestMethod]
        public void BindingParser_MultiExpression_MemberAccessAndExplicitStrings()
        {
            var parser = bindingParserNodeFactory.SetupParser("_root.MyCoolProperty 'something' \"something else\"");
            var node = parser.ReadMultiExpression();

            Assert.IsTrue(parser.OnEnd());
            Assert.IsTrue(node is MultiExpressionBindingParserNode);

            var multiExpressionNode = node as MultiExpressionBindingParserNode;

            Assert.IsTrue(multiExpressionNode.Expressions.Count == 3);

            Assert.IsTrue(multiExpressionNode.Expressions[0] is MemberAccessBindingParserNode);
            Assert.IsTrue(multiExpressionNode.Expressions[1] is LiteralExpressionBindingParserNode);
            Assert.IsTrue(multiExpressionNode.Expressions[2] is LiteralExpressionBindingParserNode);
        }

        [TestMethod]
        public void BindingParser_MultiExpression_SuperUnfriendlyContent()
        {
            var parser = bindingParserNodeFactory.SetupParser(@"
                    IsCanceled ? '}"" ValueBinding=""{value: Currency}"" HeaderText=""Currency"" />
           
                <dot:GridViewTemplateColumn HeaderText="""" >
                    <ContentTemplate>
                        <dot:RouteLink RouteName=""AdminOrderDetail"" Param -Id=""{ value: Id}"" >
                            <bs:GlyphIcon Icon=""Search"" />
                        </dot:RouteLink>
                    </ContentTemplate>
                </dot:GridViewTemplateColumn>
                <dot:GridViewTemplateColumn HeaderText="""" >
                    <ContentTemplate>
                        <dot:RouteLink RouteName=""OrderPaymentReceipt"" Visible =""{ value:  PaidDate != null}"" Param -OrderId=""{ value: Id}"" >
                            <bs:GlyphIcon Icon=""Download_alt"" />
                        </dot:RouteLink>
                    </ContentTemplate>
                </dot:GridViewTemplateColumn>
            </Columns>
            <EmptyDataTemplate>
                There are no orders to show. &nbsp; :'(
            </EmptyDataTemplate>
        </bs:GridView>
        <dot:DataPager class=""pagination"" DataSet =""{ value: Orders}"" />
    </bs:Container>
</dot:Content>
");
            var node = parser.ReadMultiExpression();

            Assert.IsTrue(parser.OnEnd());
            Assert.IsTrue(node is MultiExpressionBindingParserNode);

            var multiExpressionNode = node as MultiExpressionBindingParserNode;

            Assert.IsTrue(multiExpressionNode.Expressions.Count == 12);
        }

        [TestMethod]
        public void BindingParser_MultiExpression_MemberAccessUnsupportedOperatorAndExplicitStrings()
        {
            var parser = bindingParserNodeFactory.SetupParser("_root.MyCoolProperty += 'something' \"something else\"");
            var node = parser.ReadMultiExpression();

            Assert.IsTrue(parser.OnEnd());
            Assert.IsTrue(node is MultiExpressionBindingParserNode);

            var multiExpressionNode = node as MultiExpressionBindingParserNode;

            Assert.IsTrue(multiExpressionNode.Expressions.Count == 2);

            Assert.IsTrue(multiExpressionNode.Expressions[0] is BinaryOperatorBindingParserNode);
            Assert.IsTrue(multiExpressionNode.Expressions[1] is LiteralExpressionBindingParserNode);
        }

        [TestMethod]
        public void BindingParser_NodeTokenCorrectness_UnsupportedOperators()
        {
            var parser = bindingParserNodeFactory.SetupParser("_this.MyCoolProperty +=  _control.ClientId &^ _root += Comments");
            var node = parser.ReadExpression();

            Assert.IsTrue(parser.OnEnd());
            Assert.IsTrue(node is BinaryOperatorBindingParserNode);

            var plusEqualsExp1 = node as BinaryOperatorBindingParserNode;

            var myCoolPropertyExp = plusEqualsExp1.FirstExpression as MemberAccessBindingParserNode;
            var andCarretExp = plusEqualsExp1.SecondExpression as BinaryOperatorBindingParserNode;

            var clientIdExp = andCarretExp.FirstExpression as MemberAccessBindingParserNode;
            var plusEqualsExp2 = andCarretExp.SecondExpression as BinaryOperatorBindingParserNode;

            var rootExp = plusEqualsExp2.FirstExpression as IdentifierNameBindingParserNode;
            var commentsExp = plusEqualsExp2.SecondExpression as IdentifierNameBindingParserNode;

            //_this.MyCoolProperty +=  _control.ClientId &^ _root += Comments//
            CheckTokenTypes(plusEqualsExp1.Tokens, new BindingTokenType[] {
                BindingTokenType.Identifier,
                BindingTokenType.Dot,
                BindingTokenType.Identifier,
                BindingTokenType.WhiteSpace,
                BindingTokenType.UnsupportedOperator,
                BindingTokenType.WhiteSpace,
                BindingTokenType.Identifier,
                BindingTokenType.Dot,
                BindingTokenType.Identifier,
                BindingTokenType.WhiteSpace,
                BindingTokenType.UnsupportedOperator,
                BindingTokenType.WhiteSpace,
                BindingTokenType.Identifier,
                BindingTokenType.WhiteSpace,
                BindingTokenType.UnsupportedOperator,
                BindingTokenType.WhiteSpace,
                BindingTokenType.Identifier
            });

            //_this.MyCoolProperty //
            CheckTokenTypes(myCoolPropertyExp.Tokens, new BindingTokenType[] {
                BindingTokenType.Identifier,
                BindingTokenType.Dot,
                BindingTokenType.Identifier,
                BindingTokenType.WhiteSpace
            });

            //  _control.ClientId &^ _root += Comments//
            CheckTokenTypes(andCarretExp.Tokens, new BindingTokenType[] {
                BindingTokenType.WhiteSpace,
                BindingTokenType.Identifier,
                BindingTokenType.Dot,
                BindingTokenType.Identifier,
                BindingTokenType.WhiteSpace,
                BindingTokenType.UnsupportedOperator,
                BindingTokenType.WhiteSpace,
                BindingTokenType.Identifier,
                BindingTokenType.WhiteSpace,
                BindingTokenType.UnsupportedOperator,
                BindingTokenType.WhiteSpace,
                BindingTokenType.Identifier
            });

            //  _control.ClientId //
            CheckTokenTypes(clientIdExp.Tokens, new BindingTokenType[] {
                BindingTokenType.WhiteSpace,
                BindingTokenType.Identifier,
                BindingTokenType.Dot,
                BindingTokenType.Identifier,
                BindingTokenType.WhiteSpace
            });

            // _root += Comments//
            CheckTokenTypes(plusEqualsExp2.Tokens, new BindingTokenType[] {
                BindingTokenType.WhiteSpace,
                BindingTokenType.Identifier,
                BindingTokenType.WhiteSpace,
                BindingTokenType.UnsupportedOperator,
                BindingTokenType.WhiteSpace,
                BindingTokenType.Identifier
            });

            // _root //
            CheckTokenTypes(rootExp.Tokens, new BindingTokenType[] {
                BindingTokenType.WhiteSpace,
                BindingTokenType.Identifier,
                BindingTokenType.WhiteSpace,
            });

            // Comments//
            CheckTokenTypes(commentsExp.Tokens, new BindingTokenType[] {
                BindingTokenType.WhiteSpace,
                BindingTokenType.Identifier
            });
        }

        [TestMethod]
        public void BindingParser_GenericExpression_SimpleList()
        {
            var parser = bindingParserNodeFactory.SetupParser("System.Collections.Generic.List<string>.Enumerator");
            var node = parser.ReadExpression();

            var memberAccess = node as MemberAccessBindingParserNode;
            Assert.IsNotNull(memberAccess);
            var target = memberAccess.TargetExpression as GenericTypeReferenceBindingParserNode;
            var enumerator = memberAccess.MemberNameExpression;
            Assert.IsNotNull(target);
            Assert.AreEqual("Enumerator", enumerator?.Name);

            Assert.AreEqual("System.Collections.Generic.List", target.Type.ToDisplayString());
            Assert.AreEqual(1, target.Arguments.Count);
            Assert.AreEqual("string", target.Arguments[0].CastTo<ActualTypeReferenceBindingParserNode>().Type.ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_GenericExpression_Dictionary()
        {
            var parser = bindingParserNodeFactory.SetupParser("System.Collections.Generic.Dictionary<string, int>.ValueCollection");
            var node = parser.ReadExpression();

            var memberAccess = node.CastTo<MemberAccessBindingParserNode>();
            var target = memberAccess.TargetExpression.CastTo<GenericTypeReferenceBindingParserNode>();
            var valueCollection = memberAccess.MemberNameExpression.CastTo<IdentifierNameBindingParserNode>();

            Assert.AreEqual("System.Collections.Generic.Dictionary", target.Type.ToDisplayString());
            Assert.AreEqual("ValueCollection", valueCollection.Name);

            var arg0 = target.Arguments[0].CastTo<ActualTypeReferenceBindingParserNode>();
            var arg1 = target.Arguments[1].CastTo<ActualTypeReferenceBindingParserNode>();

            Assert.AreEqual("string", arg0?.Type.ToDisplayString());
            Assert.AreEqual("int", arg1?.Type.ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_GenericExpression_DictionaryTupleInside()
        {
            var originalString = "System.Collections.Generic.Dictionary<Tuple<bool, bool>, Tuple<string, int>>.ValueCollection";
            var parser = bindingParserNodeFactory.SetupParser(originalString);
            var node = parser.ReadExpression();

            var memberAccess = node as MemberAccessBindingParserNode;
            Assert.IsNotNull(memberAccess);
            var target = memberAccess.TargetExpression as GenericTypeReferenceBindingParserNode;
            var valueCollection = memberAccess.MemberNameExpression;
            Assert.IsNotNull(target);
            Assert.IsNotNull(valueCollection);

            Assert.IsTrue(string.Equals(originalString, node.ToDisplayString()));
        }

        [TestMethod]
        public void BindingParser_GenericExpression_DictionaryTupleInside_Invalid()
        {
            var originalString = "System.Collections.Generic.Dictionary<Tuple<bool, bool>, Tuple<string, int>.ValueCollection";
            var parser = bindingParserNodeFactory.SetupParser(originalString);
            var node = parser.ReadExpression();

            //expecting  ...Dictionary(LessThan)Tuple... because reading generic type failed and it could not read (comma) 
            //so ended at the end of binary expression
            Assert.IsTrue(string.Equals("System.Collections.Generic.Dictionary < Tuple<bool, bool>", node.ToDisplayString()));

            parser = bindingParserNodeFactory.SetupParser(originalString);
            var multi = parser.ReadMultiExpression() as MultiExpressionBindingParserNode;


            Assert.IsTrue(multi.Expressions.Count == 4);
            Assert.IsTrue(multi.Expressions[0] is BinaryOperatorBindingParserNode);
            //Then there is whitespace, comma it doesn't matter much how those are parsed just that they are eaten away
            Assert.IsTrue(multi.Expressions[3] is MemberAccessBindingParserNode);

            //With multiple expressions we are able to eat the evil extra tokens and finish the expression 
            //Expression Tuple<string, int>.ValueCollection is parsed correctly
            Assert.IsTrue(string.Equals(multi.Expressions[0].ToDisplayString(), "System.Collections.Generic.Dictionary < Tuple<bool, bool>"));
            Assert.IsTrue(string.Equals(multi.Expressions[1].ToDisplayString(), ""));
            Assert.IsTrue(string.Equals(multi.Expressions[2].ToDisplayString(), ","));
            Assert.IsTrue(string.Equals(multi.Expressions[3].ToDisplayString(), "Tuple<string, int>.ValueCollection"));
        }

        [TestMethod]
        public void BindingParser_GenericExpression_JustComparison()
        {
            var originalString = "System.Collections.Generic.Dictionary<Tuple.Count&&Meep>Squeee";
            var parser = bindingParserNodeFactory.SetupParser(originalString);
            var node = parser.ReadExpression();

            //Just comparison no generics or anything
            Assert.IsTrue(node is BinaryOperatorBindingParserNode);
            Assert.IsTrue(string.Equals(originalString, node.ToDisplayString().Replace(" ", "")));
        }

        [TestMethod]
        public void BindingParser_GenericExpression_MultipleInside()
        {
            var originalString = "System.Collections.Generic.Dictionary<Generic.List<Generic.List<Generic.Set<Generic.List<System.String>>>>>";
            var parser = bindingParserNodeFactory.SetupParser(originalString);
            var node = parser.ReadExpression();

            Assert.IsInstanceOfType(node, typeof(TypeOrFunctionReferenceBindingParserNode));
            Assert.IsTrue(string.Equals(originalString, node.ToDisplayString()));
        }

        [TestMethod]
        public void BindingParser_GenericExpression_MemberAccessInsteadOfType_Invalid()
        {
            var originalString = "System.Collections.Generic.Dictionary<Generic.List<int>.Items[0].Delf()>";
            var parser = bindingParserNodeFactory.SetupParser(originalString);
            var node = parser.ReadExpression();

            Assert.AreEqual(originalString, node.ToDisplayString().Replace(" ", ""));

            //OK display string's the same but is the tree OK?
            var firstComparison = node.CastTo<BinaryOperatorBindingParserNode>()
                .FirstExpression.CastTo<BinaryOperatorBindingParserNode>();

            Assert.AreEqual("Delf",
                firstComparison.SecondExpression.CastTo<FunctionCallBindingParserNode>()
                .TargetExpression.CastTo<MemberAccessBindingParserNode>()
                .MemberNameExpression.CastTo<IdentifierNameBindingParserNode>().Name);

            Assert.IsTrue(node.CastTo<BinaryOperatorBindingParserNode>().SecondExpression
                .As<IdentifierNameBindingParserNode>().Name == "");
        }

        [TestMethod]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type[], Domain.Company.Product", "Domain.Company.Product.DotVVM.Feature.Type[]", "Domain.Company.Product")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type[], Product", "Domain.Company.Product.DotVVM.Feature.Type[]", "Product")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type<string>[], Domain.Company.Product", "Domain.Company.Product.DotVVM.Feature.Type<string>[]", "Domain.Company.Product")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type<string>[], Product", "Domain.Company.Product.DotVVM.Feature.Type<string>[]", "Product")]
        public void BindingParser_ArrayType_AssemblyQualifiedName_ValidAssemblyName(string binding, string type, string assembly)
        {
            var parser = bindingParserNodeFactory.SetupParser(binding);
            var node = parser.ReadDirectiveTypeName() as AssemblyQualifiedNameBindingParserNode;

            Assert.IsNotNull(node, "expected qualified name node.");
            AssertNode(node, binding, 0, binding.Length);

            var arrayNode = node.TypeName as ArrayTypeReferenceBindingParserNode;
            var assemblyNode = node.AssemblyName;

            Assert.IsNotNull(assemblyNode, "expected assembly name node.");
            AssertNode(assemblyNode, assembly, type.Length + 2, assembly.Length);

            Assert.IsNotNull(arrayNode, "Expected array type reference");
            AssertNode(arrayNode, type, 0, type.Length);
        }

        [TestMethod]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type[]")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type<string>[]")]
        public void BindingParser_ArrayType_ValidAssemblyName(string binding)
        {
            var parser = bindingParserNodeFactory.SetupParser(binding);
            var array = parser.ReadDirectiveTypeName() as ArrayTypeReferenceBindingParserNode;

            Assert.IsNotNull(array, "Expected array type reference");
            Assert.IsFalse(array.HasNodeErrors);
        }

        [TestMethod]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, Domain.Company.Product", "Domain.Company.Product")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, Product", "Product")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, My-Assembly-Name", "My-Assembly-Name")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, Domain.Company.Product<int>", "Domain.Company.Product<int>")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, Domain.Company<int>.Product", "Domain.Company<int>.Product")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, Domain<int>.Company.Product", "Domain<int>.Company.Product")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, Product<int>", "Product<int>")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, Product<int>   ", "Product<int>")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, my assembly name  ", "my assembly name")]

        public void BindingParser_AssemblyQualifiedName_ValidAssemblyName(string binding, string assemblyName)
        {
            var parser = bindingParserNodeFactory.SetupParser(binding);
            var node = parser.ReadDirectiveTypeName() as AssemblyQualifiedNameBindingParserNode;

            var controlSting = binding.TrimEnd();

            Assert.IsFalse(node.AssemblyName.HasNodeErrors);
            AssertNode(node, controlSting, 0, binding.Length);
            AssertNode(node.AssemblyName, assemblyName, 44, assemblyName.Length);
        }

        [TestMethod]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type,  ")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, ,,,,,,,,,")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, this/is/apparently/invalid/for/some/reason")]
        public void BindingParser_AssemblyQualifiedName_InvalidAssemblyName(string binding)
        {
            var parser = bindingParserNodeFactory.SetupParser(binding);
            var node = parser.ReadDirectiveTypeName() as AssemblyQualifiedNameBindingParserNode;
            Assert.IsTrue(node.AssemblyName.HasNodeErrors);
        }

        [TestMethod]
        [DataRow("(arg) => Method(arg)", DisplayName = "Simple implicit single-parameter lambda expression with parentheses.")]
        [DataRow("arg => Method(arg)", DisplayName = "Simple implicit single-parameter lambda expression without parentheses.")]
        [DataRow("  arg    =>   Method   (   arg  )", DisplayName = "Simple lambda with various whitespaces.")]
        [DataRow("arg =>Method(arg)", DisplayName = "Simple lambda with various whitespaces.")]
        [DataRow("arg=>Method(arg)", DisplayName = "Simple lambda with various whitespaces.")]
        public void BindingParser_Lambda_NoTypeInfo_SingleParameter(string expression)
        {
            var parser = bindingParserNodeFactory.SetupParser(expression);
            var node = parser.ReadExpression();

            var lambda = node.CastTo<LambdaBindingParserNode>();
            var body = lambda.BodyExpression;
            var parameters = lambda.ParameterExpressions;

            Assert.AreEqual(1, parameters.Count);
            Assert.IsNull(parameters[0].Type);
            Assert.AreEqual("arg", parameters[0].Name.ToDisplayString());
            Assert.AreEqual("Method(arg)", body.ToDisplayString());
        }

        [TestMethod]
        [DataRow("_ => Method()", DisplayName = "Simple implicit single-parameter lambda expression without parentheses")]
        public void BindingParser_Lambda_NoTypeInfo_SingleIgnoredParameter(string expression)
        {
            var parser = bindingParserNodeFactory.SetupParser(expression);
            var node = parser.ReadExpression();

            var lambda = node.CastTo<LambdaBindingParserNode>();
            var body = lambda.BodyExpression;
            var parameters = lambda.ParameterExpressions;

            Assert.AreEqual(1, parameters.Count);
            Assert.IsNull(parameters[0].Type);
            Assert.AreEqual("_", parameters[0].Name.ToDisplayString());
            Assert.AreEqual("Method()", body.ToDisplayString());
        }

        [TestMethod]
        [DataRow("() => Method()", DisplayName = "Simple implicit zero-parameter lambda expression")]
        public void BindingParser_Lambda_NoParameters(string expression)
        {
            var parser = bindingParserNodeFactory.SetupParser(expression);
            var node = parser.ReadExpression();

            var lambda = node.CastTo<LambdaBindingParserNode>();
            var body = lambda.BodyExpression;
            var parameters = lambda.ParameterExpressions;

            Assert.AreEqual(0, parameters.Count);
            Assert.AreEqual("Method()", body.ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_Lambda_NoTypeInfo_MultipleParameters()
        {
            var parser = bindingParserNodeFactory.SetupParser("(arg1, arg2) => Method(arg1, arg2)");
            var node = parser.ReadExpression();

            var lambda = node.CastTo<LambdaBindingParserNode>();
            var body = lambda.BodyExpression;
            var parameters = lambda.ParameterExpressions;

            Assert.AreEqual(2, parameters.Count);
            Assert.IsNull(parameters[0].Type);
            Assert.IsNull(parameters[1].Type);
            Assert.AreEqual("arg1", parameters[0].Name.ToDisplayString());
            Assert.AreEqual("arg2", parameters[1].Name.ToDisplayString());
            Assert.AreEqual("Method(arg1, arg2)", body.ToDisplayString());
        }

        [TestMethod]
        [DataRow("(string arg) => Method(arg)", "string")]
        [DataRow("(float arg) => Method(arg)", "float")]
        [DataRow("(decimal arg) => Method(arg)", "decimal")]
        [DataRow("(System.Collections.Generic.List<int>.Subtype arg) => Method(arg)", "System.Collections.Generic.List<int>.Subtype")]
        public void BindingParser_Lambda_WithTypeInfo_SingleParameter(string expr, string type)
        {
            var parser = bindingParserNodeFactory.SetupParser(expr);
            var node = parser.ReadExpression();

            var lambda = node.CastTo<LambdaBindingParserNode>();
            var body = lambda.BodyExpression;
            var parameters = lambda.ParameterExpressions;

            Assert.AreEqual(1, parameters.Count);

            AssertNode(parameters[0].Type, type, 1, type.Length + 1);
            AssertNode(parameters[0].Name, "arg", type.Length + 2, 3);
            AssertNode(body, "Method(arg)", type.Length + 10, 11);
        }

        [TestMethod]
        [DataRow("(arg1, arg2) Method", DisplayName = "Missing lambda operator")]
        [DataRow("(arg1, arg2)", DisplayName = "Missing lambda operator")]
        [DataRow("string arg => Method(arg)", DisplayName = "Use parenthesis when explicitely defining parameter types")]
        public void BindingParser_Lambda_InvalidLambdaDeclaration(string expression)
        {
            var parser = bindingParserNodeFactory.SetupParser(expression);
            var node = parser.ReadExpression().EnumerateChildNodes().SkipWhile(n => n as LambdaBindingParserNode == null).FirstOrDefault();
            Assert.IsNull(node);
        }

        [TestMethod]
        public void BindingParser_MultiblockExpression_TreeCorrect()
        {
            var originalString = "StringProp = StringProp + 1; SetStringProp2(StringProp + 7); StringProp = 5; StringProp2 + 4 + StringProp";
            var parser = bindingParserNodeFactory.SetupParser(originalString);
            var node = parser.ReadExpression();

            var lastExpressionIdentifier = node.As<BlockBindingParserNode>()?.SecondExpression
                ?.As<BlockBindingParserNode>()?.SecondExpression
                ?.As<BlockBindingParserNode>()?.SecondExpression
                ?.As<BinaryOperatorBindingParserNode>()?.FirstExpression
                ?.As<BinaryOperatorBindingParserNode>()?.FirstExpression
                ?.As<IdentifierNameBindingParserNode>();

            Assert.IsNotNull(lastExpressionIdentifier, "Expected path was not found in the expression tree.");
            Assert.AreEqual("StringProp2", lastExpressionIdentifier.Name);

            Assert.AreEqual(originalString, node.ToDisplayString());
        }

        [DataTestMethod]
        [DataRow("A();!IsDisplayed", DisplayName = "Multiblock expression, Operator bunching - no whitespaces")]
        [DataRow("A(); !IsDisplayed", DisplayName = "Multiblock expression, Operator bunching - with whitespaces")]
        public void BindingParser_MultiblockExpression_LotOfOperators_Valid(string bindingExpression)
        {
            var parser = bindingParserNodeFactory.SetupParser(bindingExpression);
            var node = parser.ReadExpression();

            var firstExpression =
                node.As<BlockBindingParserNode>()
                ?.FirstExpression.As<FunctionCallBindingParserNode>();

            var secondExpression = node.As<BlockBindingParserNode>()
                ?.SecondExpression.As<UnaryOperatorBindingParserNode>();

            Assert.IsNotNull(firstExpression, "Expected path was not found in the expression tree.");
            Assert.IsNotNull(secondExpression, "Expected path was not found in the expression tree.");

            Assert.AreEqual(0, node.StartPosition);
            Assert.AreEqual(node.EndPosition, secondExpression.EndPosition);
            Assert.AreEqual(firstExpression.EndPosition + 1, secondExpression.StartPosition);


            //display string does not really deal with whitespace tokens, we don't care about those.
            //Just making sure the expression syntax itself remains the same
            Assert.AreEqual(SkipWhitespaces(bindingExpression), SkipWhitespaces(node.ToDisplayString()));
        }

        [DataTestMethod]
        [DataRow("StringProp = StringProp + 1;", 0, DisplayName = "Multiblock expression, bare semicolon at the end.")]
        [DataRow("StringProp = StringProp + 1; ", 1, DisplayName = "Multiblock expression, semicolon and whitespace at the end.")]
        public void BindingParser_MultiblockExpression_SemicolonEnd_Invalid(string bindingExpression, int lastBlockExpectedLength)
        {
            var parser = bindingParserNodeFactory.SetupParser(bindingExpression);
            var node = parser.ReadExpression();

            var firstExpression =
                node.As<BlockBindingParserNode>()
                ?.FirstExpression.As<BinaryOperatorBindingParserNode>();

            var secondExpression = node.As<BlockBindingParserNode>()
                ?.SecondExpression.As<VoidBindingParserNode>();

            Assert.IsNotNull(firstExpression, "Expected path was not found in the expression tree.");
            Assert.IsNotNull(secondExpression, "Expected path was not found in the expression tree.");

            Assert.AreEqual(0, node.StartPosition);
            Assert.AreEqual(node.EndPosition, secondExpression.EndPosition);
            Assert.AreEqual(firstExpression.EndPosition + 1, secondExpression.StartPosition);
            Assert.AreEqual(lastBlockExpectedLength, secondExpression.Length);


            //display string does not really deal with whitespace tokens, we don't care about those.
            //Just making sure the expression syntax itself remains the same
            Assert.AreEqual(bindingExpression.Trim(), node.ToDisplayString().Trim());
        }

        [DataTestMethod]
        [DataRow("StringProp = StringProp + 1;;test", 0, DisplayName = "Multiblock expression, bare semicolon in the middle.")]
        [DataRow("StringProp = StringProp + 1; ;test", 1, DisplayName = "Multiblock expression, semicolon and whitespace in the middle.")]
        public void BindingParser_MultiblockExpression_EmptyBlockMiddle_Invalid(string bindingExpression, int voidBlockExpectedLength)
        {
            var parser = bindingParserNodeFactory.SetupParser(bindingExpression);
            var node = parser.ReadExpression();

            var firstExpression =
                node.As<BlockBindingParserNode>()
                ?.FirstExpression.As<BinaryOperatorBindingParserNode>();

            var secondBlock =
                node.As<BlockBindingParserNode>()
                ?.SecondExpression.As<BlockBindingParserNode>();

            var middleExpression = secondBlock
                ?.FirstExpression.As<VoidBindingParserNode>();

            var lastExpression = secondBlock
                ?.SecondExpression.As<BindingParserNode>();

            Assert.IsNotNull(firstExpression, "Expected path was not found in the expression tree.");
            Assert.IsNotNull(middleExpression, "Expected path was not found in the expression tree.");
            Assert.IsNotNull(lastExpression, "Expected path was not found in the expression tree.");

            Assert.AreEqual(firstExpression.EndPosition + 1, middleExpression.StartPosition);
            Assert.AreEqual(middleExpression.EndPosition + 1, lastExpression.StartPosition);
            Assert.AreEqual(node.EndPosition, lastExpression.EndPosition);
            Assert.AreEqual(voidBlockExpectedLength, middleExpression.Length);

        }

        [DataTestMethod]
        [DataRow("var x=A(); !x", "x", DisplayName = "Variable (var) expression")]
        [DataRow("var var=A(); !var", "var", DisplayName = "Variable (var) expression, name=var")]
        [DataRow("var x = A(); !x", "x", DisplayName = "Variable (var) expression with whitespaces")]
        public void BindingParser_VariableExpression_Simple(string bindingExpression, string variableName)
        {
            var parser = bindingParserNodeFactory.SetupParser(bindingExpression);
            var node = parser.ReadExpression().CastTo<BlockBindingParserNode>();

            var firstExpression =
                node.FirstExpression.As<FunctionCallBindingParserNode>();

            var secondExpression = node.SecondExpression.As<UnaryOperatorBindingParserNode>();

            Assert.IsNotNull(firstExpression, "Expected path was not found in the expression tree.");
            Assert.IsNotNull(secondExpression, "Expected path was not found in the expression tree.");

            Assert.AreEqual(0, node.StartPosition);
            Assert.AreEqual(node.EndPosition, secondExpression.EndPosition);
            Assert.IsNotNull(node.Variable);
            Assert.AreEqual(variableName, node.Variable.Name);
            Assert.AreEqual(firstExpression.EndPosition + 1, secondExpression.StartPosition);

            Assert.AreEqual(SkipWhitespaces(bindingExpression), SkipWhitespaces(node.ToDisplayString()));
        }

        [DataTestMethod]
        [DataRow("StringProp = StringProp + 1;;test;test2", 0, DisplayName = "Multiblock expression, bare semicolon among four blocks.")]
        [DataRow("StringProp = StringProp + 1; ;test;test2", 1, DisplayName = "Multiblock expression, semicolon and whitespace among four blocks.")]
        public void BindingParser_MultiblockExpression_EmptyBlockFourBlocks_Invalid(string bindingExpression, int voidBlockExpectedLength)
        {
            var parser = bindingParserNodeFactory.SetupParser(bindingExpression);
            var node = parser.ReadExpression();

            var firstExpression =
                node.As<BlockBindingParserNode>()
                ?.FirstExpression.As<BinaryOperatorBindingParserNode>();

            var secondBlock =
                node.As<BlockBindingParserNode>()
                ?.SecondExpression.As<BlockBindingParserNode>();

            var voidExpression = secondBlock
                ?.FirstExpression.As<VoidBindingParserNode>();

            var thirdBlock =
                secondBlock
                ?.SecondExpression.As<BlockBindingParserNode>();

            var test = thirdBlock
                ?.FirstExpression.As<IdentifierNameBindingParserNode>();

            var test2 = thirdBlock
               ?.SecondExpression.As<IdentifierNameBindingParserNode>();

            Assert.IsNotNull(firstExpression, "Expected path was not found in the expression tree.");
            Assert.IsNotNull(voidExpression, "Expected path was not found in the expression tree.");
            Assert.IsNotNull(test, "Expected path was not found in the expression tree.");
            Assert.IsNotNull(test2, "Expected path was not found in the expression tree.");

            Assert.AreEqual(firstExpression.EndPosition + 1, voidExpression.StartPosition);
            Assert.AreEqual(voidExpression.EndPosition + 1, test.StartPosition);
            Assert.AreEqual(test.EndPosition + 1, test2.StartPosition);
            Assert.AreEqual(test2.EndPosition, node.EndPosition);
            Assert.AreEqual(voidBlockExpectedLength, voidExpression.Length);

            Assert.AreEqual(SkipWhitespaces(bindingExpression), SkipWhitespaces(node.ToDisplayString()));
        }

        [TestMethod]
        public void BindingParser_VariableExpression_3Vars()
        {
            var parser = bindingParserNodeFactory.SetupParser("var a = 1; var b = 2; var c = 3; a+b+c");
            var node1 = parser.ReadExpression().CastTo<BlockBindingParserNode>();
            var node2 = node1.SecondExpression.CastTo<BlockBindingParserNode>();
            var node3 = node2.SecondExpression.CastTo<BlockBindingParserNode>();

            Assert.AreEqual(0, node1.StartPosition);
            Assert.AreEqual(node1.EndPosition, node2.EndPosition);
            Assert.AreEqual(node1.EndPosition, node3.EndPosition);
            Assert.IsNotNull(node1.Variable);
            Assert.IsNotNull(node2.Variable);
            Assert.IsNotNull(node3.Variable);
            Assert.AreEqual("a", node1.Variable.Name);

            Assert.AreEqual("var a = 1; var b = 2; var c = 3; a + b + c", node1.ToDisplayString());
            Assert.AreEqual("var b = 2; var c = 3; a + b + c", node2.ToDisplayString());
            Assert.AreEqual("var c = 3; a + b + c", node3.ToDisplayString());
            Assert.AreEqual("a + b + c", node3.SecondExpression.ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_MinimalPropertyDeclaration()
        {
            var parser = bindingParserNodeFactory.SetupParser("System.String MyProperty");
            var declaration = parser.ReadPropertyDirectiveValue();

            var root = declaration.CastTo<PropertyDeclarationBindingParserNode>();
            var type = root.PropertyType.CastTo<TypeReferenceBindingParserNode>();
            var name = root.Name.CastTo<SimpleNameBindingParserNode>();

            AssertNode(root, "System.String MyProperty", 0, 24);
            AssertNode(type, "System.String", 0, 14);
            AssertNode(name, "MyProperty", 14, 10);
        }

        [TestMethod]
        public void BindingParser_PropertyHalfWrittenAttributes()
        {
            var parser = bindingParserNodeFactory.SetupParser("string MyProperty, , DotVVM., DotVVM.Fra = , DotVVM.Framework.Controls.MarkupOptionsAttribute.Required = t");
            var declaration = parser.ReadPropertyDirectiveValue();

            var root = declaration.CastTo<PropertyDeclarationBindingParserNode>();
            var type = root.PropertyType.CastTo<TypeReferenceBindingParserNode>();
            var name = root.Name.CastTo<SimpleNameBindingParserNode>();

            AssertNode(type, "string", 0, 7);
            AssertNode(name, "MyProperty", 7, 10);

            Assert.AreEqual(4, root.Attributes.Count);

            var emptyAttribute = root.Attributes[0].CastTo<SimpleNameBindingParserNode>();
            AssertNode(emptyAttribute, "", 19, 0, hasErrors: true);

            var dotvvmNode = root.Attributes[1].CastTo<MemberAccessBindingParserNode>();
            AssertNode(dotvvmNode, "DotVVM.", 21, 7);

            var dotvvmFraNode = root.Attributes[2].CastTo<BinaryOperatorBindingParserNode>();
            AssertNode(dotvvmFraNode, "DotVVM.Fra = ", 30, 13);

            var longNode = root.Attributes[3].CastTo<BinaryOperatorBindingParserNode>();
            AssertNode(longNode, "DotVVM.Framework.Controls.MarkupOptionsAttribute.Required = t", 45, 61);
        }

        [TestMethod]
        public void BindingParser_PropertyTypeHalfWritten()
        {
            var parser = bindingParserNodeFactory.SetupParser("System.");
            var declaration = parser.ReadPropertyDirectiveValue();

            var root = declaration.CastTo<PropertyDeclarationBindingParserNode>();
            var type = root.PropertyType.CastTo<ActualTypeReferenceBindingParserNode>();
            var memberAccess = type.Type.CastTo<MemberAccessBindingParserNode>();
            var target = memberAccess.TargetExpression;
            var name = memberAccess.MemberNameExpression;

            AssertNode(type, "System.", 0, 7);
            AssertNode(target, "System", 0, 6);
            AssertNode(name, "", 7, 0, hasErrors: true);
        }

        [TestMethod]
        public void BindingParser_NullablePropertyDeclaration()
        {
            var parser = bindingParserNodeFactory.SetupParser("System.String? MyProperty");
            var declaration = parser.ReadPropertyDirectiveValue();

            var root = declaration.CastTo<PropertyDeclarationBindingParserNode>();
            var nullable = root.PropertyType.CastTo<NullableTypeReferenceBindingParserNode>();
            var type = nullable.InnerType.CastTo<ActualTypeReferenceBindingParserNode>();
            var name = root.Name.CastTo<SimpleNameBindingParserNode>();

            AssertNode(root, "System.String? MyProperty", 0, 25);
            AssertNode(nullable, "System.String?", 0, 14);
            AssertNode(type, "System.String", 0, 13);
            AssertNode(name, "MyProperty", 14, 11);
        }

        [TestMethod]
        public void BindingParser_ComplexTypePropertyDeclaration()
        {
            var parser = bindingParserNodeFactory.SetupParser("System.Func<System.String?, int?, string, IdentifierNameBindingParserNode?[]>? MyProperty");
            var declaration = parser.ReadPropertyDirectiveValue();

            var root = declaration.CastTo<PropertyDeclarationBindingParserNode>();
            var nullableFunc = root.PropertyType.CastTo<NullableTypeReferenceBindingParserNode>();
            var funcGeneric = nullableFunc.InnerType.CastTo<GenericTypeReferenceBindingParserNode>();
            var func = funcGeneric.Type.CastTo<ActualTypeReferenceBindingParserNode>();

            var arg1Nullable = funcGeneric.Arguments[0].CastTo<NullableTypeReferenceBindingParserNode>();
            var arg2Nullable = funcGeneric.Arguments[1].CastTo<NullableTypeReferenceBindingParserNode>();
            var arg4Array = funcGeneric.Arguments[3].CastTo<ArrayTypeReferenceBindingParserNode>();

            var arg1 = arg1Nullable.InnerType.CastTo<ActualTypeReferenceBindingParserNode>();
            var arg2 = arg2Nullable.InnerType.CastTo<ActualTypeReferenceBindingParserNode>();
            var arg3 = funcGeneric.Arguments[2].CastTo<ActualTypeReferenceBindingParserNode>();
            var arg4Nullable = arg4Array.ElementType.CastTo<NullableTypeReferenceBindingParserNode>();

            var arg4 = arg4Nullable.InnerType.CastTo<ActualTypeReferenceBindingParserNode>();

            AssertNode(root, "System.Func<System.String?, int?, string, IdentifierNameBindingParserNode?[]>? MyProperty", 0, 89);
            AssertNode(nullableFunc, "System.Func<System.String?, int?, string, IdentifierNameBindingParserNode?[]>?", 0, 78);
            AssertNode(funcGeneric, "System.Func<System.String?, int?, string, IdentifierNameBindingParserNode?[]>", 0, 77);
            AssertNode(func, "System.Func", 0, 11);
            AssertNode(root.Name, "MyProperty", 78, 11);

            AssertNode(arg1Nullable, "System.String?", 12, 14);
            AssertNode(arg2Nullable, "int?", 28, 4);
            AssertNode(arg3, "string", 34, 6);
            AssertNode(arg4Array, "IdentifierNameBindingParserNode?[]", 42, 34);

            AssertNode(arg1, "System.String", 12, 13);
            AssertNode(arg2, "int", 28, 3);
            AssertNode(arg4Nullable, "IdentifierNameBindingParserNode?", 42, 32);

            AssertNode(arg4, "IdentifierNameBindingParserNode", 42, 31);
        }

        [TestMethod]
        public void BindingParser_InitializedPropertyDeclaration()
        {
            var parser = bindingParserNodeFactory.SetupParser("System.String MyProperty = \"Test\"");
            var declaration = parser.ReadPropertyDirectiveValue();

            var root = declaration.CastTo<PropertyDeclarationBindingParserNode>();
            var type = root.PropertyType.CastTo<TypeReferenceBindingParserNode>();
            var name = root.Name.CastTo<SimpleNameBindingParserNode>();
            var init = root.Initializer.CastTo<LiteralExpressionBindingParserNode>();

            AssertNode(root, "System.String MyProperty = \"Test\"", 0, 33);
            AssertNode(type, "System.String", 0, 14);
            AssertNode(name, "MyProperty", 14, 11);
            AssertNode(init, "\"Test\"", 27, 6);
        }

        [TestMethod]
        public void BindingParser_InitializedAttributedPropertyDeclaration()
        {
            var parser = bindingParserNodeFactory.SetupParser("System.String MyProperty = \"Test\", MarkupOptions.AllowHardCodedValue = false, MarkupOptions.Required = true");
            var declaration = parser.ReadPropertyDirectiveValue();

            var root = declaration.CastTo<PropertyDeclarationBindingParserNode>();
            var type = root.PropertyType.CastTo<TypeReferenceBindingParserNode>();
            var name = root.Name.CastTo<SimpleNameBindingParserNode>();
            var init = root.Initializer.CastTo<LiteralExpressionBindingParserNode>();
            var attributes = root.Attributes;
            Assert.AreEqual(2, attributes.Count);

            var att1 = root.Attributes[0].CastTo<BinaryOperatorBindingParserNode>();
            var att2 = root.Attributes[1].CastTo<BinaryOperatorBindingParserNode>();

            AssertNode(root, "System.String MyProperty = \"Test\", MarkupOptions.AllowHardCodedValue = False, MarkupOptions.Required = True", 0, 107);
            AssertNode(type, "System.String", 0, 14);
            AssertNode(name, "MyProperty", 14, 11);
            AssertNode(init, "\"Test\"", 27, 6);
            AssertNode(att1, "MarkupOptions.AllowHardCodedValue = False", 35, 41);
            AssertNode(att2, "MarkupOptions.Required = True", 78, 29);
        }

        [TestMethod]
        public void BindingParser_AttributedPropertyDeclaration()
        {
            var parser = bindingParserNodeFactory.SetupParser("System.String MyProperty, MarkupOptions.AllowHardCodedValue = false, MarkupOptions.Required = true");
            var declaration = parser.ReadPropertyDirectiveValue();

            var root = declaration.CastTo<PropertyDeclarationBindingParserNode>();
            var type = root.PropertyType.CastTo<ActualTypeReferenceBindingParserNode>();
            var name = root.Name.CastTo<SimpleNameBindingParserNode>();
            var attributes = root.Attributes;

            Assert.AreEqual(2, attributes.Count);
            Assert.IsNull(root.Initializer);

            var att1 = root.Attributes[0].CastTo<BinaryOperatorBindingParserNode>();
            var att2 = root.Attributes[1].CastTo<BinaryOperatorBindingParserNode>();

            AssertNode(root, "System.String MyProperty, MarkupOptions.AllowHardCodedValue = False, MarkupOptions.Required = True", 0, 98);
            AssertNode(type, "System.String", 0, 14);
            AssertNode(name, "MyProperty", 14, 10);
            AssertNode(att1, "MarkupOptions.AllowHardCodedValue = False", 26, 41);
            AssertNode(att2, "MarkupOptions.Required = True", 69, 29);
        }

        [TestMethod]
        public void BindingParser_AttributedArrayInitializedPropertyDeclaration()
        {
            var parser = bindingParserNodeFactory.SetupParser("Namespace.Enum[] MyProperty = [ Namespace.Enum.Value1, Namespace.Enum.Value2, Namespace.Enum.Value3 ], MarkupOptions.AllowHardCodedValue = false, MarkupOptions.Required = true");
            var declaration = parser.ReadPropertyDirectiveValue();

            var root = declaration.CastTo<PropertyDeclarationBindingParserNode>();
            var type = root.PropertyType.CastTo<ArrayTypeReferenceBindingParserNode>();
            var elementType = type.ElementType.CastTo<ActualTypeReferenceBindingParserNode>();
            var name = root.Name.CastTo<SimpleNameBindingParserNode>();
            var attributes = root.Attributes;

            Assert.AreEqual(2, attributes.Count);
            Assert.IsNotNull(root.Initializer);

            var att1 = root.Attributes[0].CastTo<BinaryOperatorBindingParserNode>();
            var att2 = root.Attributes[1].CastTo<BinaryOperatorBindingParserNode>();

            var element1Initializer = root.Initializer.CastTo<ArrayInitializerExpression>().ElementInitializers[0].CastTo<MemberAccessBindingParserNode>();
            var element2Initializer = root.Initializer.CastTo<ArrayInitializerExpression>().ElementInitializers[1].CastTo<MemberAccessBindingParserNode>();
            var element3Initializer = root.Initializer.CastTo<ArrayInitializerExpression>().ElementInitializers[2].CastTo<MemberAccessBindingParserNode>();

            AssertNode(root, "Namespace.Enum[] MyProperty = [ Namespace.Enum.Value1, Namespace.Enum.Value2, Namespace.Enum.Value3 ], MarkupOptions.AllowHardCodedValue = False, MarkupOptions.Required = True", 0, 175);
            AssertNode(type, "Namespace.Enum[]", 0, 16);
            AssertNode(elementType, "Namespace.Enum", 0, 14);
            AssertNode(name, "MyProperty", 16, 12);

            AssertNode(element1Initializer, "Namespace.Enum.Value1", 32, 21);
            AssertNode(element2Initializer, "Namespace.Enum.Value2", 55, 21);
            AssertNode(element3Initializer, "Namespace.Enum.Value3", 78, 22);

            AssertNode(att1, "MarkupOptions.AllowHardCodedValue = False", 103, 41);
            AssertNode(att2, "MarkupOptions.Required = True", 146, 29);
        }

        [TestMethod]
        public void BindingParser_GenericTypePropertyDeclaration()
        {
            var parser = bindingParserNodeFactory.SetupParser("IDictionary<int, System.String> MyProperty");
            var declaration = parser.ReadPropertyDirectiveValue();

            var root = declaration.CastTo<PropertyDeclarationBindingParserNode>();
            var genericType = root.PropertyType.CastTo<GenericTypeReferenceBindingParserNode>();
            var name = root.Name.CastTo<SimpleNameBindingParserNode>();

            var type = genericType.Type.CastTo<TypeReferenceBindingParserNode>();
            var arg1 = genericType.Arguments[0].CastTo<TypeReferenceBindingParserNode>();
            var arg2 = genericType.Arguments[1].CastTo<TypeReferenceBindingParserNode>();

            AssertNode(root, "IDictionary<int, System.String> MyProperty", 0, 42);
            AssertNode(name, "MyProperty", 31, 11);
            AssertNode(genericType, "IDictionary<int, System.String>", 0, 31);
            AssertNode(type, "IDictionary", 0, 11);
            AssertNode(arg1, "int", 12, 3);
            AssertNode(arg2, "System.String", 17, 13);
        }

        [TestMethod]
        public void BindingParser_GenericMethodCall_SimpleName()
        {
            var source = "GetType<string>(StringProp)";
            var parser = bindingParserNodeFactory.SetupParser(source);
            var root = parser.ReadExpression().As<FunctionCallBindingParserNode>();

            var generic = root.TargetExpression.As<GenericNameBindingParserNode>();
            var typeArgument = generic.TypeArguments[0].As<TypeReferenceBindingParserNode>();

            AssertNode(root, source, 0, source.Length);
            AssertNode(generic, "GetType<string>", 0, source.Length - 12);
            AssertNode(typeArgument, "string", 8, 6);
        }

        [TestMethod]
        public void BindingParser_GenericMethodCall_MemberAccessName()
        {
            var source = "service.GetType<string?>(StringProp)";

            var parser = bindingParserNodeFactory.SetupParser(source);
            var root = parser.ReadExpression().As<FunctionCallBindingParserNode>();

            var memberAccess = root.TargetExpression.As<MemberAccessBindingParserNode>();
            var generic = memberAccess.MemberNameExpression.As<GenericNameBindingParserNode>();
            var typeArgument = generic.TypeArguments[0].As<TypeReferenceBindingParserNode>();

            AssertNode(root, source, 0, source.Length);
            AssertNode(memberAccess, "service.GetType<string?>", 0, source.Length - 12);
            AssertNode(generic, "GetType<string?>", 8, source.Length - 20);

            AssertNode(typeArgument, "string?", 16, 7);

        }

        [TestMethod]
        public void BindingParser_GenericMethodCall_MultipleGenericArguments()
        {
            var source = "_this.Modal.GetType<string?, System.String>(StringProp)";

            var parser = bindingParserNodeFactory.SetupParser(source);
            var root = parser.ReadExpression().As<FunctionCallBindingParserNode>();

            var memberAccess1 = root.TargetExpression.As<MemberAccessBindingParserNode>();
            var memberAccess2 = memberAccess1.TargetExpression.As<MemberAccessBindingParserNode>();
            var generic = memberAccess1.MemberNameExpression.As<GenericNameBindingParserNode>();
            var typeArgument1 = generic.TypeArguments[0].As<TypeReferenceBindingParserNode>();
            var typeArgument2 = generic.TypeArguments[1].As<TypeReferenceBindingParserNode>();

            AssertNode(root, source, 0, source.Length);
            AssertNode(memberAccess1, "_this.Modal.GetType<string?, System.String>", 0, source.Length - 12);
            AssertNode(memberAccess2, "_this.Modal", 0, 11);
            AssertNode(generic, "GetType<string?, System.String>", 12, 31);

            AssertNode(typeArgument1, "string?", 20, 7);
            AssertNode(typeArgument2, "System.String", 29, 13);
        }

        private static void AssertNode(BindingParserNode node, string expectedDisplayString, int start, int length, bool hasErrors = false)
            => AssertEx.BindingNode(node, expectedDisplayString, start, length, hasErrors);

        private static string SkipWhitespaces(string str) => string.Join("", str.Where(c => !char.IsWhiteSpace(c)));

        private static void CheckTokenTypes(IEnumerable<BindingToken> bindingTokens, IEnumerable<BindingTokenType> expectedTokenTypes)
        {
            var actualTypes = bindingTokens.Select(t => t.Type);

            Assert.IsTrue(Enumerable.SequenceEqual(actualTypes, expectedTokenTypes));
        }

        private static void CheckUnaryOperatorNodeType<TInnerExpression>(UnaryOperatorBindingParserNode node, BindingTokenType operatorType)
           where TInnerExpression : BindingParserNode
        {
            Assert.AreEqual(operatorType, node.Operator);
            Assert.IsInstanceOfType(node.InnerExpression, typeof(TInnerExpression));
        }

        private static void CheckBinaryOperatorNodeType<TLeft, TRight>(BinaryOperatorBindingParserNode node, BindingTokenType operatorType)
            where TLeft : BindingParserNode
            where TRight : BindingParserNode
        {
            Assert.AreEqual(operatorType, node.Operator);
            Assert.IsInstanceOfType(node.FirstExpression, typeof(TLeft));
            Assert.IsInstanceOfType(node.SecondExpression, typeof(TRight));
        }

        [TestMethod]
        public void BindingParser_NewExpression_SimpleClass_Valid()
        {
            var result = bindingParserNodeFactory.Parse("new MyClass()");

            Assert.IsInstanceOfType(result, typeof(ConstructorCallBindingParserNode));
            var constructorCall = (ConstructorCallBindingParserNode)result;
            
            Assert.IsInstanceOfType(constructorCall.TypeExpression, typeof(SimpleNameBindingParserNode));
            Assert.AreEqual("MyClass", ((SimpleNameBindingParserNode)constructorCall.TypeExpression).Name);
            Assert.AreEqual(0, constructorCall.ArgumentExpressions.Count);
        }

        [TestMethod]
        public void BindingParser_NewExpression_WithArguments_Valid()
        {
            var result = bindingParserNodeFactory.Parse("new MyClass(arg1, 42, \"test\")");

            Assert.IsInstanceOfType(result, typeof(ConstructorCallBindingParserNode));
            var constructorCall = (ConstructorCallBindingParserNode)result;
            
            Assert.IsInstanceOfType(constructorCall.TypeExpression, typeof(SimpleNameBindingParserNode));
            Assert.AreEqual("MyClass", ((SimpleNameBindingParserNode)constructorCall.TypeExpression).Name);
            Assert.AreEqual(3, constructorCall.ArgumentExpressions.Count);
            
            // Check first argument (identifier)
            Assert.IsInstanceOfType(constructorCall.ArgumentExpressions[0], typeof(IdentifierNameBindingParserNode));
            Assert.AreEqual("arg1", ((IdentifierNameBindingParserNode)constructorCall.ArgumentExpressions[0]).Name);
            
            // Check second argument (number literal)
            Assert.IsInstanceOfType(constructorCall.ArgumentExpressions[1], typeof(LiteralExpressionBindingParserNode));
            Assert.AreEqual(42, ((LiteralExpressionBindingParserNode)constructorCall.ArgumentExpressions[1]).Value);
            
            // Check third argument (string literal)
            Assert.IsInstanceOfType(constructorCall.ArgumentExpressions[2], typeof(LiteralExpressionBindingParserNode));
            Assert.AreEqual("test", ((LiteralExpressionBindingParserNode)constructorCall.ArgumentExpressions[2]).Value);
        }

        [TestMethod]
        public void BindingParser_NewExpression_FullyQualifiedType_Valid()
        {
            var result = bindingParserNodeFactory.Parse("new System.DateTime(2023, 12, 25)");

            Assert.IsInstanceOfType(result, typeof(ConstructorCallBindingParserNode));
            var constructorCall = (ConstructorCallBindingParserNode)result;
            
            Assert.IsInstanceOfType(constructorCall.TypeExpression, typeof(MemberAccessBindingParserNode));
            Assert.AreEqual(3, constructorCall.ArgumentExpressions.Count);
            Assert.AreEqual("new System.DateTime(2023, 12, 25)", constructorCall.ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_NewExpression_GenericType_Valid()
        {
            var result = bindingParserNodeFactory.Parse("new List<string>()");

            Assert.IsInstanceOfType(result, typeof(ConstructorCallBindingParserNode));
            var constructorCall = (ConstructorCallBindingParserNode)result;
            
            Assert.IsInstanceOfType(constructorCall.TypeExpression, typeof(TypeOrFunctionReferenceBindingParserNode));
            Assert.AreEqual(0, constructorCall.ArgumentExpressions.Count);
            Assert.AreEqual("new List<string>()", constructorCall.ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_NewExpression_WithoutParentheses_SyntaxError()
        {
            var result = bindingParserNodeFactory.Parse("new MyClass");

            Assert.IsInstanceOfType(result, typeof(ConstructorCallBindingParserNode));
            var constructorCall = (ConstructorCallBindingParserNode)result;
            
            Assert.IsInstanceOfType(constructorCall.TypeExpression, typeof(SimpleNameBindingParserNode));
            Assert.AreEqual("MyClass", ((SimpleNameBindingParserNode)constructorCall.TypeExpression).Name);
            Assert.AreEqual(0, constructorCall.ArgumentExpressions.Count);
            
            // Should have a parsing error
            Assert.IsTrue(result.HasNodeErrors);
            Assert.IsTrue(result.NodeErrors.Any(e => e.Contains("Constructor call must have parentheses")));
        }

        [TestMethod]
        public void BindingParser_NewExpression_NestedInExpression_Valid()
        {
            var result = bindingParserNodeFactory.Parse("value + new MyClass(42)");

            Assert.IsInstanceOfType(result, typeof(BinaryOperatorBindingParserNode));
            var binaryOp = (BinaryOperatorBindingParserNode)result;
            
            Assert.AreEqual(BindingTokenType.AddOperator, binaryOp.Operator);
            Assert.IsInstanceOfType(binaryOp.FirstExpression, typeof(IdentifierNameBindingParserNode));
            Assert.IsInstanceOfType(binaryOp.SecondExpression, typeof(ConstructorCallBindingParserNode));
            
            var constructorCall = (ConstructorCallBindingParserNode)binaryOp.SecondExpression;
            Assert.AreEqual("MyClass", ((SimpleNameBindingParserNode)constructorCall.TypeExpression).Name);
            Assert.AreEqual(1, constructorCall.ArgumentExpressions.Count);
        }

        [TestMethod]
        public void BindingParser_NewExpression_AsMethodArgument_Valid()
        {
            var result = bindingParserNodeFactory.Parse("SomeMethod(new MyClass(), 42)");

            Assert.IsInstanceOfType(result, typeof(FunctionCallBindingParserNode));
            var functionCall = (FunctionCallBindingParserNode)result;
            
            Assert.AreEqual(2, functionCall.ArgumentExpressions.Count);
            Assert.IsInstanceOfType(functionCall.ArgumentExpressions[0], typeof(ConstructorCallBindingParserNode));
            
            var constructorCall = (ConstructorCallBindingParserNode)functionCall.ArgumentExpressions[0];
            Assert.AreEqual("MyClass", ((SimpleNameBindingParserNode)constructorCall.TypeExpression).Name);
        }

        [TestMethod]
        public void BindingParser_TypeInferredConstructorCall_Valid()
        {
            var result = bindingParserNodeFactory.Parse("new(1, 2, 3)");

            Assert.IsInstanceOfType(result, typeof(TypeInferredConstructorCallBindingParserNode));
            var constructorCall = (TypeInferredConstructorCallBindingParserNode)result;
            
            Assert.AreEqual(3, constructorCall.ArgumentExpressions.Count);
            Assert.AreEqual("new(1, 2, 3)", constructorCall.ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_TypeInferredConstructorCall_NoArguments_Valid()
        {
            var result = bindingParserNodeFactory.Parse("new()");

            Assert.IsInstanceOfType(result, typeof(TypeInferredConstructorCallBindingParserNode));
            var constructorCall = (TypeInferredConstructorCallBindingParserNode)result;
            
            Assert.AreEqual(0, constructorCall.ArgumentExpressions.Count);
            Assert.AreEqual("new()", constructorCall.ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_ArrayConstruction_WithSize_Valid()
        {
            var result = bindingParserNodeFactory.Parse("new int[5]");

            Assert.IsInstanceOfType(result, typeof(ArrayConstructionBindingParserNode));
            var arrayConstruction = (ArrayConstructionBindingParserNode)result;
            
            Assert.IsNotNull(arrayConstruction.ElementTypeExpression);
            Assert.IsNotNull(arrayConstruction.SizeExpression);
            Assert.IsNull(arrayConstruction.InitializerExpressions);
            Assert.AreEqual("new int[5]", arrayConstruction.ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_ArrayConstruction_WithInitializers_Valid()
        {
            var result = bindingParserNodeFactory.Parse("new int[] { 1, 2, 3 }");

            Assert.IsInstanceOfType(result, typeof(ArrayConstructionBindingParserNode));
            var arrayConstruction = (ArrayConstructionBindingParserNode)result;
            
            Assert.IsNotNull(arrayConstruction.ElementTypeExpression);
            Assert.IsNull(arrayConstruction.SizeExpression);
            Assert.IsNotNull(arrayConstruction.InitializerExpressions);
            Assert.AreEqual(3, arrayConstruction.InitializerExpressions.Count);
            Assert.AreEqual("new int[] { 1, 2, 3 }", arrayConstruction.ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_ArrayConstruction_TypeInferred_Valid()
        {
            var result = bindingParserNodeFactory.Parse("new[] { 1, 2, 3 }");

            Assert.IsInstanceOfType(result, typeof(ArrayConstructionBindingParserNode));
            var arrayConstruction = (ArrayConstructionBindingParserNode)result;
            
            Assert.IsNull(arrayConstruction.ElementTypeExpression);
            Assert.IsNull(arrayConstruction.SizeExpression);
            Assert.IsNotNull(arrayConstruction.InitializerExpressions);
            Assert.AreEqual(3, arrayConstruction.InitializerExpressions.Count);
            Assert.AreEqual("new[] { 1, 2, 3 }", arrayConstruction.ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_ArrayConstruction_EmptyInitializers_Valid()
        {
            var result = bindingParserNodeFactory.Parse("new int[] { }");

            Assert.IsInstanceOfType(result, typeof(ArrayConstructionBindingParserNode));
            var arrayConstruction = (ArrayConstructionBindingParserNode)result;
            
            Assert.IsNotNull(arrayConstruction.ElementTypeExpression);
            Assert.IsNull(arrayConstruction.SizeExpression);
            Assert.IsNotNull(arrayConstruction.InitializerExpressions);
            Assert.AreEqual(0, arrayConstruction.InitializerExpressions.Count);
            Assert.AreEqual("new int[] {  }", arrayConstruction.ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_ArrayConstruction_MultidimensionalArray_SyntaxError()
        {
            var result = bindingParserNodeFactory.Parse("new int[5, 3]");

            Assert.IsInstanceOfType(result, typeof(ArrayConstructionBindingParserNode));
            var arrayConstruction = (ArrayConstructionBindingParserNode)result;
            
            // Should have a parsing error
            Assert.IsTrue(result.HasNodeErrors);
            Assert.IsTrue(result.NodeErrors.Any(e => e.Contains("Multi-dimensional arrays are not supported")));
        }

        [TestMethod]
        public void BindingParser_ArrayConstruction_EmptyBracketsWithoutInitializer_SyntaxError()
        {
            var result = bindingParserNodeFactory.Parse("new int[]");

            Assert.IsInstanceOfType(result, typeof(ArrayConstructionBindingParserNode));
            var arrayConstruction = (ArrayConstructionBindingParserNode)result;
            
            // Should have a parsing error
            Assert.IsTrue(result.HasNodeErrors);
            Assert.IsTrue(result.NodeErrors.Any(e => e.Contains("must be followed by initializer list")));
        }

        [TestMethod]
        public void BindingParser_ArrayConstruction_NestedInExpression_Valid()
        {
            var result = bindingParserNodeFactory.Parse("SomeMethod(new int[] { 1, 2, 3 })");

            Assert.IsInstanceOfType(result, typeof(FunctionCallBindingParserNode));
            var functionCall = (FunctionCallBindingParserNode)result;
            
            Assert.AreEqual(1, functionCall.ArgumentExpressions.Count);
            Assert.IsInstanceOfType(functionCall.ArgumentExpressions[0], typeof(ArrayConstructionBindingParserNode));
            
            var arrayConstruction = (ArrayConstructionBindingParserNode)functionCall.ArgumentExpressions[0];
            Assert.AreEqual(3, arrayConstruction.InitializerExpressions!.Count);
        }
    }
}
