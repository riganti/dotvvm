using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redwood.Framework.Parser;
using Redwood.Framework.Parser.RwHtml.Tokenizer;

namespace Redwood.Framework.Tests.Parser.RwHtml
{
    [TestClass]
    public class RwHtmlTokenizerHtmlSpecialElementsTests : RwHtmlTokenizerTestsBase
    {

        [TestMethod]
        public void RwHtmlTokenizer_DoctypeParsing_Valid()
        {
            var input = @"test <!DOCTYPE html> test2";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("test ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<!DOCTYPE", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.OpenDoctype, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" html", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.DoctypeBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(">", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.CloseDoctype, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" test2", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_DoctypeParsing_Valid_Begin()
        {
            var input = @"<!DOCTYPE html> test";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("<!DOCTYPE", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.OpenDoctype, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" html", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.DoctypeBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(">", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.CloseDoctype, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" test", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_DoctypeParsing_Valid_End()
        {
            var input = @"test <!DOCTYPE html>";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("test ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<!DOCTYPE", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.OpenDoctype, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" html", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.DoctypeBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(">", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.CloseDoctype, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_DoctypeParsing_Valid_Empty()
        {
            var input = @"<!DOCTYPE>";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("<!DOCTYPE", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.OpenDoctype, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.DoctypeBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(">", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.CloseDoctype, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_DoctypeParsing_Invalid_Incomplete_WithValue()
        {
            var input = @"<!DOCTYPE html";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("<!DOCTYPE", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.OpenDoctype, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" html", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.DoctypeBody, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.CloseDoctype, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_DoctypeParsing_Invalid_Incomplete_NoValue()
        {
            var input = @"<!DOCTYPE";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("<!DOCTYPE", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.OpenDoctype, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.DoctypeBody, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.CloseDoctype, tokenizer.Tokens[i++].Type);
        }



        [TestMethod]
        public void RwHtmlTokenizer_XmlProcessingInstructionParsing_Valid()
        {
            var input = @"test <?xml version=""1.0"" encoding=""utf-8"" ?> test2";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("test ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<?", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.OpenXmlProcessingInstruction, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(@"xml version=""1.0"" encoding=""utf-8"" ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.XmlProcessingInstructionBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("?>", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.CloseXmlProcessingInstruction, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" test2", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_CDataParsing_Valid()
        {
            var input = @"test <![CDATA[ this is a text < > "" ' ]]> test2";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("test ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<![CDATA[", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.OpenCData, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(@" this is a text < > "" ' ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.CDataBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("]]>", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.CloseCData, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" test2", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_CommentParsing_Valid()
        {
            var input = @"test <!-- this is a text < > "" ' --> test2";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input), null);
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("test ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<!--", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.OpenComment, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(@" this is a text < > "" ' ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.CommentBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("-->", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.CloseComment, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" test2", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

    }
}
