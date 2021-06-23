using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Parser.Dothtml
{
    [TestClass]
    public class DothtmlTokenizerBindingTests : DothtmlTokenizerTestsBase
    {

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Valid_SelfClosing_BindingAttribute()
        {
            var input = @" tr <input value=""{binding: FirstName}"" />";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Valid_BindingInPlainText()
        {
            var input = @"tr {{binding: FirstName}}"" />";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Incomplete_OpenBinding()
        {
            var input = @"<input value=""{";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Incomplete_OpenBinding_CloseBinding()
        {
            var input = @"<input value=""{}";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Incomplete_OpenBinding_Text_CloseBinding()
        {
            var input = @"<input value=""{value}";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Incomplete_OpenBinding_Text_WhiteSpace_Text_CloseBinding()
        {
            var input = @"<input value=""{value value}";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Incomplete_OpenBinding_Colon_Text_CloseBinding()
        {
            var input = @"<input value=""{:value}";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Incomplete_OpenBinding_Colon_CloseBinding()
        {
            var input = @"<input value=""{:value}";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Incomplete_OpenBinding_Text_Colon_CloseBinding()
        {
            var input = @"<input value=""{name:}";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Invalid_BindingInPlainText_NotClosed()
        {
            var input = @"tr {{binding: FirstName"" />";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Invalid_BindingInPlainText_ClosedWithOneBrace()
        {
            var input = @"tr {{binding: FirstName}test"" />";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("FirstName", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(1, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Valid_CurlyBraceInStringInBinding()
        {
            var input = @"tr {{binding: ""{"" + FirstName + ""}""}}";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(@"""{"" + FirstName + ""}""", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Valid_CurlyBraceInStringInBinding_EscapedQuotes()
        {
            var input = @"tr {{binding: ""\""{"" + FirstName + '\'""{'}}";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(@"""\""{"" + FirstName + '\'""{'", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Valid_Tag_CurlyBraceInStringInBinding()
        {
            var input = @"<a href=""{binding: ""{"" + FirstName + ""}""}"">";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(@"""{"" + FirstName + ""}""", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Valid_Tag_CurlyBraceInStringInBinding_EscapedQuotes()
        {
            var input = @"<a href=""{binding: ""\""{"" + FirstName + '\'""{'}"">";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(@"""\""{"" + FirstName + '\'""{'", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Invalid_BindingInPlainText_SingleBraces_TreatedAsText()
        {
            var input = @"tr {binding: FirstName}test"" />";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(1, tokenizer.Tokens.Count);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_TokenizeBinding_Valid_OneBrace()
        {
            var input = @"{binding: FirstName}";

            // parse
            var tokenizer = new DothtmlTokenizer();
            var result = tokenizer.TokenizeBinding(input, false);

            Assert.IsTrue(result);
            CheckForErrors(tokenizer, input.Length);

            Assert.AreEqual(6, tokenizer.Tokens.Count);
            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[0].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[1].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[4].Type);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[5].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_TokenizeBinding_Valid_DoubleBrace_NotStated()
        {
            var input = @"{{binding: FirstName}}";

            // parse
            var tokenizer = new DothtmlTokenizer();
            var result =tokenizer.TokenizeBinding(input, false);

            Assert.IsTrue(result);
            CheckForErrors(tokenizer, input.Length);

            Assert.AreEqual(6, tokenizer.Tokens.Count);
            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[0].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[1].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[4].Type);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[5].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_TokenizeBinding_Valid_InvalidTextAround()
        {
            var input = @"dfds dsfsffds {binding: FirstName}fdsdsf";

            // parse
            var tokenizer = new DothtmlTokenizer();
            var result = tokenizer.TokenizeBinding(input, false);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DothtmlTokenizer_TokenizeBinding_Invalid_InvalidTextEnd()
        {
            var input = @"{binding: FirstName}fdsdsf";

            // parse
            var tokenizer = new DothtmlTokenizer();
            var result = tokenizer.TokenizeBinding(input, false);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DothtmlTokenizer_TokenizeBinding_Invalid_UnfinishedText()
        {
            var input = @"{binding: ""FirstName}fdsdsf";

            // parse
            var tokenizer = new DothtmlTokenizer();
            var result = tokenizer.TokenizeBinding(input, false);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DothtmlTokenizer_TokenizeBinding_Invalid_Unclosed()
        {
            var input = @"{binding: FirstName";

            // parse
            var tokenizer = new DothtmlTokenizer();
            var result = tokenizer.TokenizeBinding(input, false);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DothtmlTokenizer_TokenizeBinding_Valid_StringInside()
        {
            var input = @"{binding: FirstName + ""{not: Binding}""}";

            // parse
            var tokenizer = new DothtmlTokenizer();
            var result = tokenizer.TokenizeBinding(input, false);

            Assert.IsTrue(result);
            CheckForErrors(tokenizer, input.Length);
            Assert.AreEqual(6, tokenizer.Tokens.Count);
            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[0].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[1].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[4].Type);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[5].Type);
        }
    }
}
