using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class InvocationBindingParserNode : BindingParserNode
    {
        public List<BindingParserNode> ArgumentExpressions { get; private set; }
        public BindingParserNode MethodIdentifierExpression { get; private set; }

        public InvocationBindingParserNode(BindingParserNode methodIdentifier, List<BindingParserNode> arguments)
        {
            MethodIdentifierExpression = methodIdentifier;
            ArgumentExpressions = arguments;
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => base.EnumerateNodes().Concat(MethodIdentifierExpression.EnumerateNodes()
                .Concat(ArgumentExpressions.SelectRecursively(param => param.EnumerateNodes())));

        public override string ToDisplayString()
            => $"{MethodIdentifierExpression.ToDisplayString()}({ArgumentExpressions.Select(p => p.ToDisplayString()).StringJoin(", ")})";
    }
}
