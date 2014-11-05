using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redwood.Framework.Parser;
using Redwood.Framework.Parser.RwHtml;
using Redwood.Framework.Parser.RwHtml.Tokenizer;

namespace Redwood.Framework.Tests.Parser.RwHtml
{
    [TestClass]
    public class RwHtmlTokenizerElementsTests : RwHtmlTokenizerTestsBase
    {

        [TestMethod]
        public void RwHtmlTokenizer_ElementParsing_Valid_OpenClose_NoAttributes()
        {
            var input = @"<html>aaa</html>";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_ElementParsing_Valid_SelfClosing_NoAttributes_TextOnEnd()
        {
            var input = @"<html/>aaa";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_ElementParsing_Valid_SelfClosing_NonQuotedAttribute_TextOnBegin()
        {
            var input = @" tr <html xmlns=hello />";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_ElementParsing_Valid_NestedTags_MultipleAttributes()
        {
            var input = @"<html><body lang=cs><h1 class=alert size=big>Test</h1>  </body></html>";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_ElementParsing_Valid_SelfClosing_SingleQuotedAttribute()
        {
            var input = @" tr <html xmlns='hello dolly' />";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_ElementParsing_Valid_SelfClosing_DoubleQuotedAttribute()
        {
            var input = @" tr <html xmlns=""hello dolly"" />";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
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
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_ElementParsing_Valid_SelfClosing_BindingAttribute()
        {
            var input = @" tr <input value=""{binding: FirstName}"" />";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
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
        public void RwHtmlTokenizer_ElementParsing_Valid_BindingInPlainText()
        {
            var input = @"tr {{binding: FirstName}}"" />";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(3, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.OpenBinding, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.CloseBinding, tokenizer.Tokens[i++].Type);
        }
    }
}
