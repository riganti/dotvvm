using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ArrayInitializerExpression : BindingParserNode
    {
        public IList<BindingParserNode> ElementInitializers { get; private set; }

        public ArrayInitializerExpression(IList<BindingParserNode> elementInitializers)
        {
            ElementInitializers = elementInitializers;
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes() => base.EnumerateNodes().Concat(ElementInitializers);

        public override string ToDisplayString() => $"[ {(string.Join(", ", ElementInitializers.Select(arg => arg.ToDisplayString())))} ]";
    }
}
