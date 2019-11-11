#nullable enable
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    
    public abstract class IdentifierNameBindingParserNode : BindingParserNode
    {
        public string Name => NameToken.Text;
        /// <summary> The token from which the identifier was parsed. </summary>
        public BindingToken NameToken { get; private set; }

        public IdentifierNameBindingParserNode(BindingToken nameToken)
        {
            NameToken = nameToken;
        }
    }
}
