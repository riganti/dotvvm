using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Binding.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.Binding
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
