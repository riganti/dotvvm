using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class TypeInferredConstructorCallBindingParserNode : BindingParserNode
    {
        public List<BindingParserNode> ArgumentExpressions { get; private set; }

        public TypeInferredConstructorCallBindingParserNode(List<BindingParserNode> argumentExpressions)
        {
            ArgumentExpressions = argumentExpressions;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
        {
            return base.EnumerateNodes()
                .Concat(ArgumentExpressions.SelectMany(a => a.EnumerateNodes()));
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => ArgumentExpressions;

        public override string ToDisplayString() 
            => $"new({string.Join(", ", ArgumentExpressions.Select(e => e.ToDisplayString()))})";
    }
}
