using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    
    public abstract class IdentifierNameBindingParserNode : BindingParserNode
    {
        public string Name => NameToken?.Text ?? string.Empty;
        public BindingToken NameToken { get; private set; }

        public IdentifierNameBindingParserNode(BindingToken nameToken)
        {
            NameToken = nameToken;
        }
    }
}