using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using System.Diagnostics;
using System.Collections.Immutable;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AssemblyNameBindingParserNode : BindingParserNode
    {
        public string Name { get; }
        public AssemblyNameBindingParserNode(List<BindingToken> tokens)
        {
            Tokens = tokens;
            Name = string.Concat(tokens.Select(t => t.Text));
        }

        public AssemblyNameBindingParserNode(string name)
            : this(new List<BindingToken>() { new BindingToken(name, BindingTokenType.Identifier, 0, 0, 0, 0)})
        { }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => Enumerable.Empty<BindingParserNode>();

        public override string ToDisplayString() => $"{Name}";
    }
}
