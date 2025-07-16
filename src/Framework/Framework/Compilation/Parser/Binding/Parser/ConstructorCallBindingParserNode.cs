using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ConstructorCallBindingParserNode : BindingParserNode
    {
        public BindingParserNode TypeExpression { get; private set; }
        public List<BindingParserNode> ArgumentExpressions { get; private set; }

        public ConstructorCallBindingParserNode(BindingParserNode typeExpression, List<BindingParserNode> argumentExpressions)
        {
            TypeExpression = typeExpression;
            ArgumentExpressions = argumentExpressions;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
        {
            return
                base.EnumerateNodes()
                    .Concat(TypeExpression.EnumerateNodes())
                    .Concat(ArgumentExpressions.SelectMany(a => a.EnumerateNodes()));
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new[] { TypeExpression }.Concat(ArgumentExpressions);

        public override string ToDisplayString() 
            => $"new {TypeExpression.ToDisplayString()}({string.Join(", ", ArgumentExpressions.Select(e => e.ToDisplayString()))})";
    }
}
