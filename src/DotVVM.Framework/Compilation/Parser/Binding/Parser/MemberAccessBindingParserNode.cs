using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class MemberAccessBindingParserNode : BindingParserNode
    {
        public BindingParserNode TargetExpression { get; set; }
        public IdentifierNameBindingParserNode MemberNameExpression { get; set; }

        public MemberAccessBindingParserNode(BindingParserNode targetExpression, IdentifierNameBindingParserNode memberNameExpression)
        {
            TargetExpression = targetExpression;
            MemberNameExpression = memberNameExpression;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
        {
            return
                base.EnumerateNodes()
                    .Concat(TargetExpression.EnumerateNodes())
                    .Concat(MemberNameExpression.EnumerateNodes());
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new[] { TargetExpression, MemberNameExpression };

        public override string ToDisplayString()
            => $"{TargetExpression.ToDisplayString()}.{MemberNameExpression.ToDisplayString()}";
    }
}