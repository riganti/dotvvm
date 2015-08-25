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
        public BindingParserNode Node { get; set; }

        public BindingCompilationException(string message, Exception innerException, BindingParserNode node)
            : base(message, innerException)
        {
            Node = node;
        }

        public BindingCompilationException(string message, BindingParserNode node)
            : this(message, null, node)
        {
        }
    }
}
