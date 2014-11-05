using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redwood.Framework.Parser;
using Redwood.Framework.Parser.RwHtml;
using Redwood.Framework.Parser.RwHtml.Tokenizer;

namespace Redwood.Framework.Tests.Parser.RwHtml
{
    [TestClass]
    public class RwHtmlTokenizerDirectivesTests : RwHtmlTokenizerTestsBase
    {

        [TestMethod]
        public void RwHtmlTokenizer_DirectiveParsing_Valid_TwoDirectives()
        {
            var input = @"
@viewmodel: Redwood.Samples.Sample1.IndexViewModel
@masterpage: ~/Site.rwhtml

this is a test content";

            // parse
            var tokenizer = new RwHtmlTokenizer(new StringReader(input), null);
            tokenizer.Tokenize();
            CheckForErrors(tokenizer, input.Length);


            // first line
            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Colon, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_DirectiveParsing_Valid_NoDirectives_WhiteSpaceOnStart()
        {
            var input = @"
this is a test content";

            // parse
            var tokenizer = new RwHtmlTokenizer(new StringReader(input), null);
            tokenizer.Tokenize();
            CheckForErrors(tokenizer, input.Length);


            // first line
            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_DirectiveParsing_Valid_NoDirectives_NoWhiteSpaceOnStart()
        {
            var input = @"this is a test content";

            // parse
            var tokenizer = new RwHtmlTokenizer(new StringReader(input), null);
            tokenizer.Tokenize();
            CheckForErrors(tokenizer, input.Length);


            // first line
            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }




        // TODO: directives with invalid syntax








    }
}
