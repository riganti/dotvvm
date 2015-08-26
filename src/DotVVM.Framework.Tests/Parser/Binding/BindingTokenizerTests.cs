using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Binding.Tokenizer;
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
        public void BindingTokenizer_AllOperators_Valid()
        {
            var tokens = Tokenize("+ - * / % < > <= >= == != ! & && | || ? : ?? . ,");

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
            Assert.AreEqual(index, tokens.Count);
        }

        [TestMethod]
        public void BindingTokenizer_SingleEqualsToken_Invalid()
        {
            var tokens = Tokenize("=");

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(BindingTokenType.EqualsEqualsOperator, tokens[0].Type);
            Assert.IsTrue(tokens[0].HasError);
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




        private static List<BindingToken> Tokenize(string expression)
        {
            // tokenize
            var tokenizer = new BindingTokenizer();
            tokenizer.Tokenize(new StringReader(expression));
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
