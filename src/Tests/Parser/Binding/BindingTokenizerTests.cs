using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Parser.Binding
{
    [TestClass]
    public class BindingTokenizerTests
    {

        [TestMethod]
        public void BindingTokenizer_EmptyExpression_Valid()
        {
            var tokens = Tokenize("");
            Assert.AreEqual(0, tokens.Count);
        }

        [TestMethod]
        public void BindingTokenizer_IdentifierAndUnaryOperator_Valid()
        {
            var tokens = Tokenize(" - a");

            var index = 0;
            Assert.AreEqual(4, tokens.Count);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.SubtractOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
        }

        [TestMethod]
        public void BindingTokenizer_IdentifierOperatorsAndStringLiteral_Valid()
        {
            var tokens = Tokenize("a!=b+ (\"str\\\"ing\")");

            var index = 0;
            Assert.AreEqual(8, tokens.Count);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.NotEqualsOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.AddOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.OpenParenthesis, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.StringLiteralToken, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.CloseParenthesis, tokens[index++].Type);
        }

        [TestMethod]
        public void BindingTokenizer_OperatorPrecedence_NegationAfterAssignment()
        {
            var tokens = Tokenize("a=!a");

            var index = 0;
            Assert.AreEqual(4, tokens.Count);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.AssignOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.NotOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
        }

        [TestMethod]
        public void BindingTokenizer_OperatorPrecedence_MinusAfterAssignment()
        {
            var tokens = Tokenize("a=-5");

            var index = 0;
            Assert.AreEqual(4, tokens.Count);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.AssignOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.SubtractOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
        }

        [TestMethod]
        public void BindingTokenizer_AllOperators_Valid()
        {
            var tokens = Tokenize("+ - * / % < > <= >= == != ! & && | || ? : ?? . , => ^ ~");

            var index = 0;
            Assert.AreEqual(BindingTokenType.AddOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.SubtractOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.MultiplyOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.DivideOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.ModulusOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.LessThanOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.GreaterThanOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.LessThanEqualsOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.GreaterThanEqualsOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.EqualsEqualsOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.NotEqualsOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.NotOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.AndOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.AndAlsoOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.OrOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.OrElseOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.QuestionMarkOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.ColonOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.NullCoalescingOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Dot, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Comma, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.LambdaOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.ExclusiveOrOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.OnesComplementOperator, tokens[index++].Type);
            Assert.AreEqual(index, tokens.Count);
        }

        [TestMethod]
        public void BindingTokenizer_SingleEqualsToken_Valid()
        {
            var tokens = Tokenize("a =   b");

            var index = 0;
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.AssignOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(index, tokens.Count);
        }


        [TestMethod]
        public void BindingTokenizer_UnclosedStringLiteral_Invalid()
        {
            var tokens = Tokenize("a'aaa");

            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[0].Type);
            Assert.AreEqual(BindingTokenType.StringLiteralToken, tokens[1].Type);
            Assert.IsTrue(tokens[1].HasError);
        }

        [TestMethod]
        public void BindingTokenizer_UnsupportedOperator_AddEquals()
        {
            var tokens = Tokenize("Test+=3");

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[0].Type);
            Assert.AreEqual(BindingTokenType.UnsupportedOperator, tokens[1].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[2].Type);
        }

        [TestMethod]
        public void BindingTokenizer_UnsupportedOperator_TwoDifferent()
        {
            var tokens = Tokenize("Test+=3<>5");

            Assert.AreEqual(5, tokens.Count);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[0].Type);
            Assert.AreEqual(BindingTokenType.UnsupportedOperator, tokens[1].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[2].Type);
            Assert.AreEqual(BindingTokenType.UnsupportedOperator, tokens[3].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[4].Type);
        }

        [TestMethod]
        public void BindingTokenizer_UnsupportedOperator_ThreeEquals()
        {
            var tokens = Tokenize("Test===3");

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[0].Type);
            Assert.AreEqual(BindingTokenType.UnsupportedOperator, tokens[1].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[2].Type);
        }

        [TestMethod]
        public void BindingTokenizer_UnsupportedOperator_Arrow()
        {
            var tokens = Tokenize("Test=>>3");

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[0].Type);
            Assert.AreEqual(BindingTokenType.UnsupportedOperator, tokens[1].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[2].Type);
        }

        [TestMethod]
        public void BindingTokenizer_UnsupportedOperator_CarretEquals()
        {
            var tokens = Tokenize("Test^=3");

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[0].Type);
            Assert.AreEqual(BindingTokenType.UnsupportedOperator, tokens[1].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[2].Type);
        }

        [DataTestMethod]
        [DataRow("StringProp = StringProp + 1;;test", false)]
        [DataRow("StringProp = StringProp + 1; ;test", true)]
        public void BindingTokenizer_MultiblockExpression_VoidBlockMiddle_Valid(string expression, bool voidWhitespace)
        {
            var tokens = Tokenize(expression);

            var index = 0;
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.AssignOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.AddOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Semicolon, tokens[index++].Type);
            if (voidWhitespace)
            {
                Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            }
            Assert.AreEqual(BindingTokenType.Semicolon, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);

            Assert.AreEqual(index, tokens.Count);
        }

        [TestMethod]
        public void BindingTokenizer_MultiblockExpression_BunchingOperators_Valid()
        {
            var tokens = Tokenize("A();!IsDisplayed");

            var index = 0;
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.OpenParenthesis, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.CloseParenthesis, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Semicolon, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.NotOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(index, tokens.Count);
        }

        [TestMethod]
        [DataRow("$\"String {Arg}\"")]
        [DataRow("$'String {Arg}'")]
        public void BindingTokenizer_InterpolatedString_Valid(string expression)
        {
            var tokens = Tokenize(expression);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(BindingTokenType.InterpolatedStringToken, tokens[0].Type);
        }

        [TestMethod]
        [DataRow("$'String {'InnerString'}'")]
        [DataRow("$'String {$'Inner {'InnerInnerString'}'}'")]
        public void BindingTokenizer_InterpolatedString_InnerString(string expression)
        {
            var tokens = Tokenize(expression);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(BindingTokenType.InterpolatedStringToken, tokens[0].Type);
        }

        [TestMethod]
        [DataRow("$'{{'   ", "$'{{'", "   ")]
        [DataRow("$'}}'   ", "$'}}'", "   ")]
        public void BindingTokenizer_InterpolatedString_DoNotReadPastString(string expression, string text1, string text2)
        {
            var tokens = Tokenize(expression);
            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual(tokens[0].Type, BindingTokenType.InterpolatedStringToken);
            Assert.AreEqual(tokens[1].Type, BindingTokenType.WhiteSpace);
            Assert.AreEqual(text1, tokens[0].Text);
            Assert.AreEqual(text2, tokens[1].Text);
        }

        [TestMethod]
        public void BindingTokenizer_UnaryOperator_BunchingOperators_Valid()
        {
            var tokens = Tokenize("A(!IsDisplayed)");

            var index = 0;
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.OpenParenthesis, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.NotOperator, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.CloseParenthesis, tokens[index++].Type);

            Assert.AreEqual(index, tokens.Count);
        }

        [DataTestMethod]
        [DataRow("test;", false)]
        [DataRow("test; ", true)]
        public void BindingTokenizer_MultiblockExpression_VoidBlockLast_Valid(string expression, bool voidWhitespace)
        {
            var tokens = Tokenize(expression);

            var index = 0;
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Semicolon, tokens[index++].Type);

            if (voidWhitespace)
            {
                Assert.AreEqual(BindingTokenType.WhiteSpace, tokens[index++].Type);
            }

            Assert.AreEqual(index, tokens.Count);
        }

        [TestMethod]
        public void BindingTokenizer_MultiblockExpression_Simple_Valid()
        {
            var tokens = Tokenize("test;test");

            var index = 0;
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Semicolon, tokens[index++].Type);
            Assert.AreEqual(BindingTokenType.Identifier, tokens[index++].Type);
            Assert.AreEqual(index, tokens.Count);
        }

        private static List<BindingToken> Tokenize(string expression)
        {
            // tokenize
            var tokenizer = new BindingTokenizer();
            tokenizer.Tokenize(expression);
            var tokens = tokenizer.Tokens;

            // ensure that whole input was tokenized and that there are no holes
            var position = 0;
            foreach (var token in tokens)
            {
                Assert.AreEqual(position, token.StartPosition);
                position += token.Length;
            }
            Assert.AreEqual(position, expression.Length);

            return tokens;
        }
    }
}
