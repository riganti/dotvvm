using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redwood.Framework.Parser;
using Redwood.Framework.Parser.RwHtml.Tokenizer;

namespace Redwood.Framework.Tests.Parser.RwHtml
{
    [TestClass]
    public class RwHtmlTokenizerBindingTests : RwHtmlTokenizerTestsBase
    {

        [TestMethod]
        public void RwHtmlTokenizer_BindingParsing_Valid_SelfClosing_BindingAttribute()
        {
            var input = @" tr <input value=""{binding: FirstName}"" />";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_BindingParsing_Valid_BindingInPlainText()
        {
            var input = @"tr {{binding: FirstName}}"" />";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_BindingParsing_Incomplete_OpenBinding()
        {
            var input = @"<input value=""{";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_BindingParsing_Incomplete_OpenBinding_CloseBinding()
        {
            var input = @"<input value=""{}";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_BindingParsing_Incomplete_OpenBinding_Text_CloseBinding()
        {
            var input = @"<input value=""{value}";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_BindingParsing_Incomplete_OpenBinding_Text_WhiteSpace_Text_CloseBinding()
        {
            var input = @"<input value=""{value value}";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_BindingParsing_Incomplete_OpenBinding_Colon_Text_CloseBinding()
        {
            var input = @"<input value=""{:value}";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_BindingParsing_Incomplete_OpenBinding_Colon_CloseBinding()
        {
            var input = @"<input value=""{:value}";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_BindingParsing_Incomplete_OpenBinding_Text_Colon_CloseBinding()
        {
            var input = @"<input value=""{name:}";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.IsFalse(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }
    
        [TestMethod]
        public void RwHtmlTokenizer_BindingParsing_Invalid_BindingInPlainText_NotClosed()
        {
            var input = @"tr {{binding: FirstName"" />";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_BindingParsing_Invalid_BindingInPlainText_ClosedWithOneBrace()
        {
            var input = @"tr {{binding: FirstName}test"" />";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(1, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(RwHtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_BindingParsing_Invalid_BindingInPlainText_SingleBraces_TreatedAsText()
        {
            var input = @"tr {binding: FirstName}test"" />";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(1, tokenizer.Tokens.Count);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }
    }
}
