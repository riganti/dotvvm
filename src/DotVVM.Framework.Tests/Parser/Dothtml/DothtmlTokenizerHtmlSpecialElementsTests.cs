using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Tests.Parser.Dothtml
{
    [TestClass]
    public class DothtmlTokenizerHtmlSpecialElementsTests : DothtmlTokenizerTestsBase
    {

        [TestMethod]
        public void DothtmlTokenizer_DoctypeParsing_Valid()
        {
            var input = @"test <!DOCTYPE html> test2";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("test ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<!DOCTYPE", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenDoctype, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" html", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.DoctypeBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(">", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CloseDoctype, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" test2", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_DoctypeParsing_Valid_Begin()
        {
            var input = @"<!DOCTYPE html> test";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("<!DOCTYPE", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenDoctype, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" html", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.DoctypeBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(">", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CloseDoctype, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" test", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_DoctypeParsing_Valid_End()
        {
            var input = @"test <!DOCTYPE html>";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("test ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<!DOCTYPE", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenDoctype, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" html", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.DoctypeBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(">", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CloseDoctype, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_DoctypeParsing_Valid_Empty()
        {
            var input = @"<!DOCTYPE>";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("<!DOCTYPE", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenDoctype, tokenizer.Tokens[i++].Type);
            
            Assert.AreEqual(">", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CloseDoctype, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_DoctypeParsing_Invalid_Incomplete_WithValue()
        {
            var input = @"<!DOCTYPE html";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("<!DOCTYPE", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenDoctype, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" html", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.DoctypeBody, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.CloseDoctype, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_DoctypeParsing_Invalid_Incomplete_NoValue()
        {
            var input = @"<!DOCTYPE";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("<!DOCTYPE", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenDoctype, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.DoctypeBody, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.CloseDoctype, tokenizer.Tokens[i++].Type);
        }



        [TestMethod]
        public void DothtmlTokenizer_XmlProcessingInstructionParsing_Valid()
        {
            var input = @"test <?xml version=""1.0"" encoding=""utf-8"" ?> test2";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("test ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<?", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenXmlProcessingInstruction, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(@"xml version=""1.0"" encoding=""utf-8"" ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.XmlProcessingInstructionBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("?>", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CloseXmlProcessingInstruction, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" test2", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_CDataParsing_Valid()
        {
            var input = @"test <![CDATA[ this is a text < > "" ' ]]> test2";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("test ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<![CDATA[", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenCData, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(@" this is a text < > "" ' ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CDataBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("]]>", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CloseCData, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" test2", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_CommentParsing_Valid()
        {
            var input = @"test <!-- this is a text < > "" ' --> test2";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("test ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<!--", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenComment, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(@" this is a text < > "" ' ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CommentBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("-->", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CloseComment, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" test2", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

    }
}
