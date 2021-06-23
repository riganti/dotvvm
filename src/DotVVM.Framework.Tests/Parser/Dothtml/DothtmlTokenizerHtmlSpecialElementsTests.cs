using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

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
            tokenizer.Tokenize(input);
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
            tokenizer.Tokenize(input);
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
            tokenizer.Tokenize(input);
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
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("<!DOCTYPE", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenDoctype, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.DoctypeBody, tokenizer.Tokens[i++].Type);
            
            Assert.AreEqual(">", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CloseDoctype, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_DoctypeParsing_Invalid_Incomplete_WithValue()
        {
            var input = @"<!DOCTYPE html";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
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
            tokenizer.Tokenize(input);
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
            tokenizer.Tokenize(input);
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
            tokenizer.Tokenize(input);
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
            tokenizer.Tokenize(input);
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

        [TestMethod]
        public void DothtmlTokenizer_CommentParsing_Valid_2()
        {
            var input = @"test <!--<a href=""test1"">test2</a>--> test3 <img />";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("test ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<!--", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenComment, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(@"<a href=""test1"">test2</a>", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CommentBody, tokenizer.Tokens[i++].Type);
            
            Assert.AreEqual("-->", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CloseComment, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" test3 ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("img", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("/", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);
            
            Assert.AreEqual(">", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }


        [TestMethod]
        public void DothtmlTokenizer_ServerCommentParsing_Valid()
        {
            var input = @"test <%-- this is a text < > "" ' --%> test2";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("test ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<%--", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenServerComment, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(@" this is a text < > "" ' ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CommentBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("--%>", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CloseComment, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" test2", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_ServerCommentParsing_Valid_2()
        {
            var input = @"test <%--<a href=""test1"">test2</a>--%> test3 <img />";

            // parse
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(input);
            CheckForErrors(tokenizer, input.Length);

            // first line
            var i = 0;
            Assert.AreEqual("test ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<%--", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenServerComment, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(@"<a href=""test1"">test2</a>", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CommentBody, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("--%>", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CloseComment, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" test3 ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("<", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.OpenTag, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("img", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Text, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(" ", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokenizer.Tokens[i++].Type);

            Assert.AreEqual("/", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.Slash, tokenizer.Tokens[i++].Type);

            Assert.AreEqual(">", tokenizer.Tokens[i].Text);
            Assert.AreEqual(DothtmlTokenType.CloseTag, tokenizer.Tokens[i++].Type);
        }

        [TestMethod]
        public void DothtmlTokenizer_CommentInsideElement()
        {
            var input = "<a <!-- comment --> href='so'>";
            var tokens = Tokenize(input);

            Assert.AreEqual(DothtmlTokenType.OpenTag, tokens[0].Type);
            Assert.AreEqual(@"<", tokens[0].Text);

            Assert.AreEqual(DothtmlTokenType.Text, tokens[1].Type);
            Assert.AreEqual(@"a", tokens[1].Text);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokens[2].Type);
            Assert.AreEqual(@" ", tokens[2].Text);

            Assert.AreEqual(DothtmlTokenType.OpenComment, tokens[3].Type);
            Assert.AreEqual(@"<!--", tokens[3].Text);

            Assert.AreEqual(DothtmlTokenType.CommentBody, tokens[4].Type);
            Assert.AreEqual(@" comment ", tokens[4].Text);

            Assert.AreEqual(DothtmlTokenType.CloseComment, tokens[5].Type);
            Assert.AreEqual(@"-->", tokens[5].Text);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokens[6].Type);
            Assert.AreEqual(@" ", tokens[6].Text);

            Assert.AreEqual(DothtmlTokenType.Text, tokens[7].Type);
            Assert.AreEqual(@"href", tokens[7].Text);

            Assert.AreEqual(DothtmlTokenType.Equals, tokens[8].Type);
            Assert.AreEqual(@"=", tokens[8].Text);

            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokens[9].Type);
            Assert.AreEqual(@"'", tokens[9].Text);

            Assert.AreEqual(DothtmlTokenType.Text, tokens[10].Type);
            Assert.AreEqual(@"so", tokens[10].Text);

            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokens[11].Type);
            Assert.AreEqual(@"'", tokens[11].Text);

            Assert.AreEqual(DothtmlTokenType.CloseTag, tokens[12].Type);
            Assert.AreEqual(@">", tokens[12].Text);
        }

        [TestMethod]
        public void DothtmlTokenizer_ServerCommentInsideElement()
        {
            var input = "<a <%-- comment --%> href=''>";
            var tokens = Tokenize(input);

            Assert.AreEqual(DothtmlTokenType.OpenTag, tokens[0].Type);
            Assert.AreEqual(@"<", tokens[0].Text);

            Assert.AreEqual(DothtmlTokenType.Text, tokens[1].Type);
            Assert.AreEqual(@"a", tokens[1].Text);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokens[2].Type);
            Assert.AreEqual(@" ", tokens[2].Text);

            Assert.AreEqual(DothtmlTokenType.OpenServerComment, tokens[3].Type);
            Assert.AreEqual(@"<%--", tokens[3].Text);

            Assert.AreEqual(DothtmlTokenType.CommentBody, tokens[4].Type);
            Assert.AreEqual(@" comment ", tokens[4].Text);

            Assert.AreEqual(DothtmlTokenType.CloseComment, tokens[5].Type);
            Assert.AreEqual(@"--%>", tokens[5].Text);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokens[6].Type);
            Assert.AreEqual(@" ", tokens[6].Text);

            Assert.AreEqual(DothtmlTokenType.Text, tokens[7].Type);
            Assert.AreEqual(@"href", tokens[7].Text);

            Assert.AreEqual(DothtmlTokenType.Equals, tokens[8].Type);
            Assert.AreEqual(@"=", tokens[8].Text);

            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokens[9].Type);
            Assert.AreEqual(@"'", tokens[9].Text);

            Assert.AreEqual(DothtmlTokenType.Text, tokens[10].Type);
            Assert.AreEqual(@"", tokens[10].Text);

            Assert.AreEqual(DothtmlTokenType.SingleQuote, tokens[11].Type);
            Assert.AreEqual(@"'", tokens[11].Text);

            Assert.AreEqual(DothtmlTokenType.CloseTag, tokens[12].Type);
            Assert.AreEqual(@">", tokens[12].Text);
        }

        [TestMethod]
        public void DothtmlTokenizer_Valid_CommentBeforeDirective()
        {
            var input = "<!-- my comment --> @viewModel TestDirective\r\nTest";
            var tokens = Tokenize(input);

            Assert.AreEqual(DothtmlTokenType.OpenComment, tokens[0].Type);
            Assert.AreEqual(@"<!--", tokens[0].Text);

            Assert.AreEqual(DothtmlTokenType.CommentBody, tokens[1].Type);
            Assert.AreEqual(@" my comment ", tokens[1].Text);

            Assert.AreEqual(DothtmlTokenType.CloseComment, tokens[2].Type);
            Assert.AreEqual(@"-->", tokens[2].Text);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokens[3].Type);
            Assert.AreEqual(@" ", tokens[3].Text);

            Assert.AreEqual(DothtmlTokenType.DirectiveStart, tokens[4].Type);
            Assert.AreEqual(@"@", tokens[4].Text);

            Assert.AreEqual(DothtmlTokenType.DirectiveName, tokens[5].Type);
            Assert.AreEqual(@"viewModel", tokens[5].Text);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokens[6].Type);
            Assert.AreEqual(@" ", tokens[6].Text);

            Assert.AreEqual(DothtmlTokenType.DirectiveValue, tokens[7].Type);
            Assert.AreEqual(@"TestDirective", tokens[7].Text);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokens[8].Type);
            Assert.AreEqual("\r\n", tokens[8].Text);

            Assert.AreEqual(DothtmlTokenType.Text, tokens[9].Type);
            Assert.AreEqual(@"Test", tokens[9].Text);
        }
		[TestMethod]
		public void DothtmlTokenizer_NestedServerComment()
		{
			var input = "<%-- my <div> <%-- <p> </p> --%> comment --%>";
			var tokens = Tokenize(input);
			Assert.AreEqual(DothtmlTokenType.OpenServerComment, tokens[0].Type);
			Assert.AreEqual(@"<%--", tokens[0].Text);

			Assert.AreEqual(DothtmlTokenType.CommentBody, tokens[1].Type);
			Assert.AreEqual(@" my <div> <%-- <p> </p> --%> comment ", tokens[1].Text);

			Assert.AreEqual(DothtmlTokenType.CloseComment, tokens[2].Type);
			Assert.AreEqual(@"--%>", tokens[2].Text);
		}



        [TestMethod]
        public void DothtmlTokenizer_CommentWithMoreMinuses()
        {
            var input = "<!-- my comment ---> @viewModel TestDirective\r\nTest";
            var tokens = Tokenize(input);

            Assert.AreEqual(DothtmlTokenType.OpenComment, tokens[0].Type);
            Assert.AreEqual(@"<!--", tokens[0].Text);

            Assert.AreEqual(DothtmlTokenType.CommentBody, tokens[1].Type);
            Assert.AreEqual(@" my comment -", tokens[1].Text);

            Assert.AreEqual(DothtmlTokenType.CloseComment, tokens[2].Type);
            Assert.AreEqual(@"-->", tokens[2].Text);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokens[3].Type);
            Assert.AreEqual(@" ", tokens[3].Text);

            Assert.AreEqual(DothtmlTokenType.DirectiveStart, tokens[4].Type);
            Assert.AreEqual(@"@", tokens[4].Text);

            Assert.AreEqual(DothtmlTokenType.DirectiveName, tokens[5].Type);
            Assert.AreEqual(@"viewModel", tokens[5].Text);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokens[6].Type);
            Assert.AreEqual(@" ", tokens[6].Text);

            Assert.AreEqual(DothtmlTokenType.DirectiveValue, tokens[7].Type);
            Assert.AreEqual(@"TestDirective", tokens[7].Text);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokens[8].Type);
            Assert.AreEqual("\r\n", tokens[8].Text);

            Assert.AreEqual(DothtmlTokenType.Text, tokens[9].Type);
            Assert.AreEqual(@"Test", tokens[9].Text);
        }

        [TestMethod]
        public void DothtmlTokenizer_Valid_ServerCommentBeforeDirective()
        {
            var input = "  <%-- my comment --%>@viewModel TestDirective\r\nTest";
            var tokens = Tokenize(input);

            Assert.AreEqual(DothtmlTokenType.Text, tokens[0].Type);
            Assert.AreEqual(@"  ", tokens[0].Text);

            Assert.AreEqual(DothtmlTokenType.OpenServerComment, tokens[1].Type);
            Assert.AreEqual(@"<%--", tokens[1].Text);

            Assert.AreEqual(DothtmlTokenType.CommentBody, tokens[2].Type);
            Assert.AreEqual(@" my comment ", tokens[2].Text);

            Assert.AreEqual(DothtmlTokenType.CloseComment, tokens[3].Type);
            Assert.AreEqual(@"--%>", tokens[3].Text);

            Assert.AreEqual(DothtmlTokenType.DirectiveStart, tokens[4].Type);
            Assert.AreEqual(@"@", tokens[4].Text);

            Assert.AreEqual(DothtmlTokenType.DirectiveName, tokens[5].Type);
            Assert.AreEqual(@"viewModel", tokens[5].Text);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokens[6].Type);
            Assert.AreEqual(@" ", tokens[6].Text);

            Assert.AreEqual(DothtmlTokenType.DirectiveValue, tokens[7].Type);
            Assert.AreEqual(@"TestDirective", tokens[7].Text);

            Assert.AreEqual(DothtmlTokenType.WhiteSpace, tokens[8].Type);
            Assert.AreEqual("\r\n", tokens[8].Text);

            Assert.AreEqual(DothtmlTokenType.Text, tokens[9].Type);
            Assert.AreEqual(@"Test", tokens[9].Text);
        }

        [TestMethod]
        public void BindingTokenizer_UnclosedHtmlComment_Error()
        {
            var tokens = Tokenize(@"<!--REEEEEEEEE");

            Assert.AreEqual(3, tokens.Count);

            Assert.AreEqual(DothtmlTokenType.OpenComment,   tokens[0].Type);
            Assert.AreEqual(DothtmlTokenType.CommentBody,          tokens[1].Type);
            Assert.AreEqual(DothtmlTokenType.CloseComment,  tokens[2].Type);

            Assert.AreEqual(null, tokens[0].Error, "This token is just opening token nothing wrong here.");
            Assert.AreEqual(null, tokens[1].Error, "This token is just body nothing wrong with that.");
            Assert.IsTrue(tokens[2].Error.ErrorMessage.Contains("not closed"),"This token had to be created artificialy.");
        }

        [TestMethod]
        public void BindingTokenizer_UnclosedServerComment_Error()
        {
            var tokens = Tokenize(@"<%--REEEEEEEEE");

            Assert.AreEqual(3, tokens.Count);

            Assert.AreEqual(DothtmlTokenType.OpenServerComment, tokens[0].Type);
            Assert.AreEqual(DothtmlTokenType.CommentBody, tokens[1].Type);
            Assert.AreEqual(DothtmlTokenType.CloseComment, tokens[2].Type);

            Assert.AreEqual(null, tokens[0].Error, "This token is just opening token nothing wrong here.");
            Assert.AreEqual(null, tokens[1].Error, "This token is just body nothing wrong with that.");
            Assert.IsTrue(tokens[2].Error.ErrorMessage.Contains("not closed"), "This token had to be created artificialy.");
        }
    }
}
