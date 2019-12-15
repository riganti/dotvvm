#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    public class MultiExpressionBindingParserNode : BindingParserNode
    {
        public IReadOnlyList<BindingParserNode> Expressions { get; private set; }

        public MultiExpressionBindingParserNode(List<BindingParserNode> expressions)
        {
            Expressions = expressions;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
        {
            return
                base.EnumerateNodes()
                    .Concat(Expressions.SelectMany(e => e.EnumerateNodes()));
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => Expressions;

        public override string ToDisplayString()
            => string.Join(" ",Expressions.Select(e => e.ToDisplayString()));
    }
}
