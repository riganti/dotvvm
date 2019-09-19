using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Parser.Dothtml
{
    [TestClass]
    public class DothtmlTokenizerElementsTests : DothtmlTokenizerTestsBase
    {

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Valid_OpenClose_NoAttributes()
        {
            var input = @"<html>aaa</html>";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Valid_SelfClosing_NoAttributes_TextOnEnd()
        {
            var input = @"<html/>aaa";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Valid_SelfClosing_NonQuotedAttribute_TextOnBegin()
        {
            var input = @" tr <html xmlns=hello />";

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
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Valid_NestedTags_MultipleAttributes()
        {
            var input = @"<html><body lang=cs><h1 class=alert size=big>Test</h1>  </body></html>";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Valid_SelfClosing_SingleQuotedAttribute()
        {
            var input = @" tr <html xmlns='hello dolly' />";

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
            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Valid_SelfClosing_DoubleQuotedAttribute()
        {
            var input = @" tr <html xmlns=""hello dolly"" />";

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
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Valid_SelfClosing_EmptyAttribute()
        {
            var input = @" tr <html xmlns="""" />";

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
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }


        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Valid_AttributeWithoutValue()
        {
            var input = @" tr <input type=""checkbox"" checked />";

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
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_Invalid_OpenBraceInText()
        {
            var input = "inline < script";
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);

            Assert.IsTrue(tokenizer.Tokens.All(t => t.Type == DothtmlTokenType.Text));
            Assert.AreEqual(string.Concat(tokenizer.Tokens.Select(t => t.Text)), input);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Incomplete_OpenTag_InvalidTagName_AnotherTag()
        {
            var input = @"<'<a/>";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Incomplete_OpenTag()
        {
            var input = @"<html";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Incomplete_OpenTag_AnotherTag()
        {
            var input = @"<html<a/>";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Incomplete_OpenTag_WhiteSpace()
        {
            var input = @"<html ";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Incomplete_OpenTag_WhiteSpace_AnotherTag()
        {
            var input = @"<html <a/>";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Incomplete_OpenTag_AttributeName()
        {
            var input = @"<html attr";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Incomplete_OpenTag_AttributeName_AnotherTag()
        {
            var input = @"<html attr<a/>";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Incomplete_OpenTag_AttributeName_Equals()
        {
            var input = @"<html attr=";

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

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Incomplete_OpenTag_AttributeName_Equals_AnotherTag()
        {
            var input = @"<html attr=<a/>";

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

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Incomplete_OpenTag_AttributeName_Equals_Quote()
        {
            var input = @"<html attr='";

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
            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Incomplete_OpenTag_AttributeName_Equals_Quote_AnotherTag()
        {
            var input = @"<html attr='<a/>";

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
            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Incomplete_OpenTag_AttributeName_Equals_Quote_Quote()
        {
            var input = @"<html attr=''";

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
            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Incomplete_OpenTag_AttributeName_Equals_Quote_Quote_AnotherTag()
        {
            var input = @"<html attr=''<a/>";

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
            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Invalid_ElementName_MissingTagPrefix()
        {
            var input = @"<:name/>";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Invalid_ElementName_MissingTagName()
        {
            var input = @"<prefix:/>";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Invalid_AttributeName_MissingTagPrefix()
        {
            var input = @"<a :name=''/>";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_AttributeNameStartWithSquareBracket()
        {
            var input = "<a [text]=\"'This is a attribute with special characters ' + (show ? 'true' : 'false')\"/>";

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
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_AttributeNameStartWithRoundBracket()
        {
            var input = "<a (text)=\"'This is a attribute with special characters ' + (show ? 'true' : 'false')\"/>";

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
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DoubleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Invalid_AttributeName_MissingTagName()
        {
            var input = @"<a prefix:=''/>";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Colon, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Equals, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Invalid_OpenTag_OpenTag()
        {
            var input = @"<<";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            for (int i = 0; i < 2; i++)
            {
                Assert.IsTrue(tokenizer.Tokens[i].HasError);
                Assert.AreEqual("<", tokenizer.Tokens[i].Text);
                Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            }
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Invalid_OpenTag_Equals()
        {
            var input = @"<=";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[0].Type);
            Assert.IsTrue(tokenizer.Tokens[0].HasError);
            Assert.AreEqual("<", tokenizer.Tokens[0].Text);

            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[1].Type);
            Assert.AreEqual("=", tokenizer.Tokens[1].Text);
        }

        [TestMethod]
        public void DothtmlTokenizer_ElementParsing_Invalid_CloseTag_TreatedAsText()
        {
            var input = @">";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

    }
}
