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
@viewmodel Redwood.Samples.Sample1.IndexViewModel
@masterpage ~/Site.rwhtml

this is a test content";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);


            // first line
            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(RwHtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);
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
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
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
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);


            // first line
            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }



        [TestMethod]
        public void RwHtmlTokenizer_DirectiveParsing_Invalid_OnlyAtSymbol()
        {
            var input = @"@";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);
            
            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_DirectiveParsing_Invalid_AtSymbol_DirectiveName()
        {
            var input = @"@viewmodel";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("viewmodel", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_DirectiveParsing_Invalid_AtSymbol_WhiteSpace_DirectiveName()
        {
            var input = @"@ viewmodel";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("viewmodel", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_DirectiveParsing_Invalid_AtSymbol_NewLine_Content()
        {
            var input = @"@
test";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);
            
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_DirectiveParsing_Invalid_AtSymbol_DirectiveName_NewLine_Content()
        {
            var input = @"@viewmodel
test";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("viewmodel", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("\r\n", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("test", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_DirectiveParsing_Invalid_AtSymbol_DirectiveName_Space_NewLine_Content()
        {
            var input = @"@viewmodel  
test";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("viewmodel", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(2, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("\r\n", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("test", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }


        [TestMethod]
        public void RwHtmlTokenizer_DirectiveParsing_Invalid_AtSymbol_Space_NewLine_Content()
        {
            var input = @"@  
test";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(2, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("\r\n", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("test", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void RwHtmlTokenizer_DirectiveParsing_Invalid_AtSymbol_AtSymbol_Content()
        {
            var input = @"@@test";

            // parse
            var tokenizer = new RwHtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(RwHtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(RwHtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("@test", tokenizer.Tokens[i].Text);
            Assert.AreEqual(RwHtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);
        }

    }
}
