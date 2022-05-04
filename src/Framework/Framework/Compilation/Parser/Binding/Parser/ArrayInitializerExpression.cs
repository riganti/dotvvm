using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ArrayInitializerExpression : BindingParserNode
    {
        public List<BindingParserNode> ElementInitializers { get; }

        public ArrayInitializerExpression(List<BindingParserNode> elementInitializers)
        {
            ElementInitializers = elementInitializers;
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes() => base.EnumerateNodes().Concat(ElementInitializers);

        public override string ToDisplayString() => $"[ {(string.Join(", ", ElementInitializers.Select(arg => arg.ToDisplayString())))} ]";
    }
}
