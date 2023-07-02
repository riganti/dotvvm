using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;

namespace DotVVM.Framework.Compilation.Binding
{
    public class BindingCompilationException : Exception
    {
        public IEnumerable<TokenBase> Tokens { get; set; }
        public string? Expression { get; set; }

        public BindingCompilationException(string message, Exception? innerException, BindingParserNode node)
            : this(message, innerException, node.Tokens)
        {
        }

        public BindingCompilationException(string message, Exception? innerException, IEnumerable<BindingToken> tokens)
            : base(message, innerException)
        {
            // trim leading and trailing whitespace (unless the whole expression is whitespace)
            var tokensList = new List<BindingToken>(tokens);
            var leadingWhitespace = tokensList.FindIndex(t => t.Type != BindingTokenType.WhiteSpace);
            var trailingWhitespace = tokensList.FindLastIndex(t => t.Type != BindingTokenType.WhiteSpace);
            if (leadingWhitespace >= 0 && trailingWhitespace >= 0)
            {
                tokensList.RemoveRange(trailingWhitespace + 1, tokensList.Count - trailingWhitespace - 1);
                tokensList.RemoveRange(0, leadingWhitespace);
            }
            Tokens = tokensList;
        }

        public BindingCompilationException(string message, Exception? innerException, IEnumerable<TokenBase> tokens)
            : base(message, innerException)
        {
            Tokens = tokens;
        }

        public BindingCompilationException(string message, BindingParserNode node)
            : this(message, null, node)
        {
        }
    }
}
