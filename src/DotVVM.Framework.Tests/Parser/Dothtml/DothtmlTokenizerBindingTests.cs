using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

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
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
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
            tokenizer.Tokenize(new StringReader(input));
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
            tokenizer.Tokenize(new StringReader(input));
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
            tokenizer.Tokenize(new StringReader(input));
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
            tokenizer.Tokenize(new StringReader(input));
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
            tokenizer.Tokenize(new StringReader(input));
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
            tokenizer.Tokenize(new StringReader(input));
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
            tokenizer.Tokenize(new StringReader(input));
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
            tokenizer.Tokenize(new StringReader(input));
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
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Invalid_BindingInPlainText_ClosedWithOneBrace()
        {
            var input = @"tr {{binding: FirstName}test"" />";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(1, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_BindingParsing_Invalid_BindingInPlainText_SingleBraces_TreatedAsText()
        {
            var input = @"tr {binding: FirstName}test"" />";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(1, tokenizer.Tokens.Count);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }
    }
}
