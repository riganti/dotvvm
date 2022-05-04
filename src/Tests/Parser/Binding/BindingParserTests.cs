using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using DotVVM.Framework.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BindingParser = DotVVM.Framework.Compilation.Parser.Binding.Parser.BindingParser;
using a = System.Collections.Generic.Dictionary<string, int>.ValueCollection;
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
        public void BindingParser_MultipleUnsupportedBinaryOperators_Valid()
        {
            var parser = bindingParserNodeFactory.SetupParser("_root.MyCoolProperty += _this.Number1 + Number2^_parent0.Exponent * Multiplikator");
            var node = parser.ReadExpression();

            Assert.IsTrue(parser.OnEnd());
            Assert.IsInstanceOfType(node, typeof(BinaryOperatorBindingParserNode));

            var plusAssignNode = node as BinaryOperatorBindingParserNode;

            CheckBinaryOperatorNodeType<MemberAccessBindingParserNode, BinaryOperatorBindingParserNode>(plusAssignNode, BindingTokenType.UnsupportedOperator);

            var caretNode = plusAssignNode.SecondExpression as BinaryOperatorBindingParserNode;

            CheckBinaryOperatorNodeType<BinaryOperatorBindingParserNode, BinaryOperatorBindingParserNode>(caretNode, BindingTokenType.UnsupportedOperator);

            var plusNode = caretNode.FirstExpression as BinaryOperatorBindingParserNode;

            CheckBinaryOperatorNodeType<MemberAccessBindingParserNode, IdentifierNameBindingParserNode>(plusNode, BindingTokenType.AddOperator);

            var multiplyNode = caretNode.SecondExpression as BinaryOperatorBindingParserNode;

            CheckBinaryOperatorNodeType<MemberAccessBindingParserNode, IdentifierNameBindingParserNode>(multiplyNode, BindingTokenType.MultiplyOperator);

            Assert.IsTrue(caretNode.NodeErrors.Any());
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

            Assert.IsTrue(node is TypeOrFunctionReferenceBindingParserNode);
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
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, Domain.Company.Product")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, Product")]
        public void BindingParser_AssemblyQualifiedName_ValidAssemblyName(string binding)
        {
            var parser = bindingParserNodeFactory.SetupParser(binding);
            var node = parser.ReadDirectiveTypeName() as AssemblyQualifiedNameBindingParserNode;
            Assert.IsFalse(node.AssemblyName.HasNodeErrors);
        }

        [TestMethod]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, Domain.Company.Product<int>")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, Domain.Company<int>.Product")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, Domain<int>.Company.Product")]
        [DataRow("Domain.Company.Product.DotVVM.Feature.Type, Product<int>")]
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
        [DataRow("(System.Collections.Generic.List<int> arg) => Method(arg)", "System.Collections.Generic.List<int>")]
        public void BindingParser_Lambda_WithTypeInfo_SingleParameter(string expr, string type)
        {
            var parser = bindingParserNodeFactory.SetupParser(expr);
            var node = parser.ReadExpression();

            var lambda = node.CastTo<LambdaBindingParserNode>();
            var body = lambda.BodyExpression;
            var parameters = lambda.ParameterExpressions;

            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual(type, parameters[0].Type.ToDisplayString());
            Assert.AreEqual("arg", parameters[0].Name.ToDisplayString());
            Assert.AreEqual("Method(arg)", body.ToDisplayString());
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

            Assert.AreEqual("System.String MyProperty", root.ToDisplayString());
            Assert.AreEqual("System.String", type.ToDisplayString());
            Assert.AreEqual("MyProperty", name.ToDisplayString());
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

            Assert.AreEqual("System.String MyProperty = \"Test\"", root.ToDisplayString());
            Assert.AreEqual("System.String", type.ToDisplayString());
            Assert.AreEqual("MyProperty", name.ToDisplayString());
            Assert.AreEqual("\"Test\"", init.ToDisplayString());
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

            Assert.AreEqual("System.String MyProperty = \"Test\", MarkupOptions.AllowHardCodedValue = False, MarkupOptions.Required = True", root.ToDisplayString());
            Assert.AreEqual("System.String", type.ToDisplayString());
            Assert.AreEqual("MyProperty", name.ToDisplayString());
            Assert.AreEqual("\"Test\"", init.ToDisplayString());
            Assert.AreEqual("MarkupOptions.AllowHardCodedValue = False", att1.ToDisplayString());
            Assert.AreEqual("MarkupOptions.Required = True", att2.ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_AttributedPropertyDeclaration()
        {
            var parser = bindingParserNodeFactory.SetupParser("System.String MyProperty, MarkupOptions.AllowHardCodedValue = false, MarkupOptions.Required = true");
            var declaration = parser.ReadPropertyDirectiveValue();

            var root = declaration.CastTo<PropertyDeclarationBindingParserNode>();
            var type = root.PropertyType.CastTo<TypeReferenceBindingParserNode>();
            var name = root.Name.CastTo<SimpleNameBindingParserNode>();
            var attributes = root.Attributes;

            Assert.AreEqual(2, attributes.Count);
            Assert.IsNull(root.Initializer);

            var att1 = root.Attributes[0].CastTo<BinaryOperatorBindingParserNode>();
            var att2 = root.Attributes[1].CastTo<BinaryOperatorBindingParserNode>();

            Assert.AreEqual("System.String MyProperty, MarkupOptions.AllowHardCodedValue = False, MarkupOptions.Required = True", root.ToDisplayString());
            Assert.AreEqual("System.String", type.ToDisplayString());
            Assert.AreEqual("MyProperty", name.ToDisplayString());
            Assert.AreEqual("MarkupOptions.AllowHardCodedValue = False", att1.ToDisplayString());
            Assert.AreEqual("MarkupOptions.Required = True", att2.ToDisplayString());
        }

        [TestMethod]
        public void BindingParser_AttributedArrayInitializedPropertyDeclaration()
        {
            var parser = bindingParserNodeFactory.SetupParser("Namespace.Enum[] MyProperty = [ Namespace.Enum.Value1, Namespace.Enum.Value2, Namespace.Enum.Value3 ], MarkupOptions.AllowHardCodedValue = false, MarkupOptions.Required = true");
            var declaration = parser.ReadPropertyDirectiveValue();

            var root = declaration.CastTo<PropertyDeclarationBindingParserNode>();
            var type = root.PropertyType.CastTo<TypeReferenceBindingParserNode>();
            var name = root.Name.CastTo<SimpleNameBindingParserNode>();
            var attributes = root.Attributes;

            Assert.AreEqual(2, attributes.Count);
            Assert.IsNotNull(root.Initializer);

            var att1 = root.Attributes[0].CastTo<BinaryOperatorBindingParserNode>();
            var att2 = root.Attributes[1].CastTo<BinaryOperatorBindingParserNode>();

            var element1Initializer = root.Initializer.CastTo<ArrayInitializerExpression>().ElementInitializers[0].CastTo<MemberAccessBindingParserNode>();
            var element2Initializer = root.Initializer.CastTo<ArrayInitializerExpression>().ElementInitializers[1].CastTo<MemberAccessBindingParserNode>();
            var element3Initializer = root.Initializer.CastTo<ArrayInitializerExpression>().ElementInitializers[2].CastTo<MemberAccessBindingParserNode>();

            Assert.AreEqual("Namespace.Enum[] MyProperty = [ Namespace.Enum.Value1, Namespace.Enum.Value2, Namespace.Enum.Value3 ], MarkupOptions.AllowHardCodedValue = False, MarkupOptions.Required = True", root.ToDisplayString());
            Assert.AreEqual("Namespace.Enum[]", type.ToDisplayString());
            Assert.AreEqual("MyProperty", name.ToDisplayString());

            Assert.AreEqual("Namespace.Enum.Value1", element1Initializer.ToDisplayString());
            Assert.AreEqual("Namespace.Enum.Value2", element2Initializer.ToDisplayString());
            Assert.AreEqual("Namespace.Enum.Value3", element3Initializer.ToDisplayString());

            Assert.AreEqual("MarkupOptions.AllowHardCodedValue = False", att1.ToDisplayString());
            Assert.AreEqual("MarkupOptions.Required = True", att2.ToDisplayString());
        }

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
    }
}
