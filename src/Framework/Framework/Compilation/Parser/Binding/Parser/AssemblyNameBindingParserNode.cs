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

        public AssemblyNameBindingParserNode(string name)
        {
            Name = name;
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => Enumerable.Empty<BindingParserNode>();

        public override string ToDisplayString() => $"{Name}";
    }
}
