using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class IdentifierNameBindingParserNode : BindingParserNode
    {
        public string Name { get; private set; }

        public IdentifierNameBindingParserNode(string name)
        {
            Name = name;
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => Enumerable.Empty<BindingParserNode>();

        public override string ToDisplayString() => $"{Name}";
    }
}