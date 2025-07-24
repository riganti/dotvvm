using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ArrayConstructionBindingParserNode : BindingParserNode
    {
        public BindingParserNode? ElementType { get; }
        public List<BindingParserNode> Size { get; }
        public List<BindingParserNode>? Initializers { get; }

        public ArrayConstructionBindingParserNode(BindingParserNode? elementType, List<BindingParserNode> size, List<BindingParserNode>? initializer)
        {
            ElementType = elementType;
            Size = size;
            Initializers = initializer;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
        {
            var nodes = base.EnumerateNodes();

            if (ElementType != null)
                nodes = nodes.Concat(ElementType.EnumerateNodes());

            nodes = nodes.Concat(Size.SelectMany(s => s.EnumerateNodes()));

            if (Initializers != null)
                nodes = nodes.Concat(Initializers.SelectMany(e => e.EnumerateNodes()));

            return nodes;
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
        {
            if (ElementType is {})
                yield return ElementType;
            foreach (var s in Size)
                yield return s;
            if (Initializers is {})
                foreach (var initializer in Initializers)
                    yield return initializer;
        }

        public override string ToDisplayString()
        {
            return "new" + (ElementType is null ? "" : " " + ElementType.ToDisplayString())
                + "[" + string.Join(", ", Size.SelectMany(s => s.ToDisplayString())) + "]"
                + (Initializers is null ? "" :
                   " {" + string.Join(", ", Initializers.Select(e => e.ToDisplayString())) + "}");
        }
    }
}
