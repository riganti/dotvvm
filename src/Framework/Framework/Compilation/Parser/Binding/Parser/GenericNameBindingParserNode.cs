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
        public List<TypeReferenceBindingParserNode> TypeArguments { get; private set; } = new List<TypeReferenceBindingParserNode>();

        public GenericNameBindingParserNode(BindingToken name, List<TypeReferenceBindingParserNode> typeArguments) : base(name)
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
