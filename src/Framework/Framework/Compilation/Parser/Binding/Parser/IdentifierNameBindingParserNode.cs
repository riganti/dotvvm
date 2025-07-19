using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{

    public abstract class IdentifierNameBindingParserNode : BindingParserNode
    {
        public string Name { get; }
        /// <summary> The token from which the identifier was parsed. </summary>
        public BindingToken NameToken { get; }
        public bool IsEscapedKeyword => NameToken.Type == BindingTokenType.EscapedIdentifier;

        public IdentifierNameBindingParserNode(BindingToken nameToken)
        {
            NameToken = nameToken;
            Name = nameToken.Type == BindingTokenType.EscapedIdentifier ? nameToken.Text.Substring(1) : nameToken.Text;
        }
    }
}
