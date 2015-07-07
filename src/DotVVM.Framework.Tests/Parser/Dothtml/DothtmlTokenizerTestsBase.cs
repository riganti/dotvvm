using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Tests.Parser.Dothtml
{
    public abstract class DothtmlTokenizerTestsBase
    {

        protected void CheckForErrors(DothtmlTokenizer tokenizer, int inputLength)
        {
            // check for parsing errors
            if (tokenizer.Tokens.Any(t => t.Length != t.Text.Length))
            {
                throw new Exception("The length of the token does not match with its text content length!");
            }

            // check that the token sequence is complete
            var position = 0;
            foreach (var token in tokenizer.Tokens)
            {
                if (token.StartPosition != position)
                {
                    throw new Exception("The token sequence is not complete!");
                }
                position += token.Length;
            }

            if (position != inputLength)
            {
                throw new Exception("The parser didn't finished the file!");
            }
        }
    }
}