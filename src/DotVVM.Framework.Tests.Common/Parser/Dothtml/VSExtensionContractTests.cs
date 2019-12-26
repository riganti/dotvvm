using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.Parser.Dothtml
{
    /// Contains test cases that ensure that API exported for our VS Extension works correctly
    [TestClass]
    public class VSExtensionContractTests
    {
        [TestMethod]
        public void MutableTokens()
        {
            // for incremental updates of the tree, the Text and Position must be accessible

            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize("some not very interesting text <!-- Comment -->");
            var text = tokenizer.Tokens[0];
            var comment = tokenizer.Tokens[2];
            Assert.AreEqual(text.Type, DothtmlTokenType.Text);
            Assert.AreEqual(tokenizer.Tokens[1].Type, DothtmlTokenType.OpenComment);
            Assert.AreEqual(comment.Type, DothtmlTokenType.CommentBody);
            Assert.AreEqual(tokenizer.Tokens[3].Type, DothtmlTokenType.CloseComment);
            Assert.AreEqual(4, tokenizer.Tokens.Count);

            text.Text = "some little bit more interesting text";
            text.Length = text.Text.Length;
            comment.StartPosition = "some little bit more interesting text".Length;
        }
    }
}
