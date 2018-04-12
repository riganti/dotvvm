using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsIncrementOrDecrementSyntax : TsExpressionSyntax, IIncrementOrDecrementSyntax
    {
        public IExpressionSyntax Target { get; }
        public bool IsPostfix { get; }
        public bool IsIncrement { get; }

        public TsIncrementOrDecrementSyntax(ISyntaxNode parent, IExpressionSyntax target, bool isPostfix,
            bool isIncrement) : base(parent)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
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

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitIncrementOrDecrementOperation(this);
        }
    }
}
