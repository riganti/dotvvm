using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;

namespace DotVVM.Framework.Compilation.Binding
{
    public class BindingCompilationException : Exception
    {
        public IEnumerable<TokenBase> Tokens { get; set; }
        public string Expression { get; set; }

        public BindingCompilationException(string message, Exception innerException, BindingParserNode node)
            : this(message, innerException, node.Tokens)
        {
        }

        public BindingCompilationException(string message, Exception innerException, IEnumerable<TokenBase> tokens)
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
