using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using System.Diagnostics;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SimpleNameBindingParserNode : IdentifierNameBindingParserNode
    {
        public SimpleNameBindingParserNode(BindingToken nameToken) : base(nameToken)
        {
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => Enumerable.Empty<BindingParserNode>();

        public override string ToDisplayString() => $"{Name}";
    }
}
