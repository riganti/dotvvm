using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Dothtml;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Tests.Parser.Dothtml
{
    [TestClass]
    public class DothtmlTokenizerDirectivesTests : DothtmlTokenizerTestsBase
    {

        [TestMethod]
        public void DothtmlTokenizer_DirectiveParsing_Valid_TwoDirectives()
        {
            var input = @"
@viewmodel DotVVM.Samples.Sample1.IndexViewModel
@masterpage ~/Site.dothtml

this is a test content";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);


            // first line
            var i = 0;
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_DirectiveParsing_Valid_NoDirectives_WhiteSpaceOnStart()
        {
            var input = @"
this is a test content";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);


            // first line
            var i = 0;
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_DirectiveParsing_Valid_NoDirectives_NoWhiteSpaceOnStart()
        {
            var input = @"this is a test content";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);


            // first line
            var i = 0;
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }



        [TestMethod]
        public void DothtmlTokenizer_DirectiveParsing_Invalid_OnlyAtSymbol()
        {
            var input = @"@";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);
            
            var i = 0;
            Assert.AreEqual(DothtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_DirectiveParsing_Invalid_AtSymbol_DirectiveName()
        {
            var input = @"@viewmodel";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("viewmodel", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_DirectiveParsing_Invalid_AtSymbol_WhiteSpace_DirectiveName()
        {
            var input = @"@ viewmodel";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("viewmodel", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_DirectiveParsing_Invalid_AtSymbol_NewLine_Content()
        {
            var input = @"@
test";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);
            
            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_DirectiveParsing_Invalid_AtSymbol_DirectiveName_NewLine_Content()
        {
            var input = @"@viewmodel
test";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("viewmodel", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("\r\n", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("test", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_DirectiveParsing_Invalid_AtSymbol_DirectiveName_Space_NewLine_Content()
        {
            var input = @"@viewmodel  
test";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("viewmodel", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(2, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("\r\n", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("test", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }


        [TestMethod]
        public void DothtmlTokenizer_DirectiveParsing_Invalid_AtSymbol_Space_NewLine_Content()
        {
            var input = @"@  
test";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(2, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("\r\n", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("test", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_DirectiveParsing_Invalid_AtSymbol_AtSymbol_Content()
        {
            var input = @"@@test";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(new StringReader(input));
            CheckForErrors(tokenizer, input.Length);

            var i = 0;
            Assert.AreEqual(DothtmlTokenType.DirectiveStart, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.DirectiveName, tokenizer.Tokens[i++].Type);

            Assert.IsTrue(tokenizer.Tokens[i].HasError);
            Assert.AreEqual(0, tokenizer.Tokens[i].Length);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("@test", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.DirectiveValue, tokenizer.Tokens[i++].Type);
        }

    }
}
