#nullable enable
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class GenericNameBindingParserNode : IdentifierNameBindingParserNode
    {
        public List<BindingParserNode> TypeArguments { get; private set; } = new List<BindingParserNode>();

        public GenericNameBindingParserNode(BindingToken name, List<BindingParserNode> typeArguments) : base(name)
        {
            TypeArguments = typeArguments;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
        {
            return
                base.EnumerateNodes()
                    .Concat(TypeArguments.SelectMany(a => a.EnumerateNodes()));
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => TypeArguments;

        public override string ToDisplayString()
           => $"{Name}<{string.Join(", ", TypeArguments.Select(e => e.ToDisplayString()))}>";
    }
}
