using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsIncrementOrDecrementSyntax : TsExpressionSyntax
    {
        public TsExpressionSyntax Target { get; }
        public bool IsPostfix { get; }
        public bool IsIncrement { get; }

        public TsIncrementOrDecrementSyntax(TsSyntaxNode parent, TsExpressionSyntax target, bool isPostfix, bool isIncrement) : base(parent)
        {
            Target = target;
            IsPostfix = isPostfix;
            IsIncrement = isIncrement;
        }

        public override string ToDisplayString()
        {
            string @operator = GetOperatorString();
            if (IsPostfix)
            {
                return $"{Target.ToDisplayString()}{@operator}";
            }
            else
            {
                return $"{@operator}{Target.ToDisplayString()}";
            }
        }

        private string GetOperatorString()
        {
            return IsIncrement ? "++" : "--";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(ITsNodeVisitor visitor)
        {
            visitor.VisitIncrementOrDecrementOperation(this);
        }
    }
}
