using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    public class GenericTypeBindingParserNode : BindingParserNode
    {
        public BindingParserNode TargetExpression { get; private set; }
        public List<BindingParserNode> TypeArguments { get; private set; } = new List<BindingParserNode>();

        public GenericTypeBindingParserNode(BindingParserNode targetExpresion, List<BindingParserNode> typeArguments)
        {
            TargetExpression = targetExpresion;
            TypeArguments = typeArguments;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
        {
            return
                base.EnumerateNodes()
                    .Concat(TargetExpression.EnumerateNodes())
                    .Concat(TypeArguments.SelectMany(a => a.EnumerateNodes()));
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new[] { TargetExpression }.Concat(TypeArguments);
    }
}
