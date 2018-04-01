using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsParenthesizedExpressionSyntax : TsExpressionSyntax
    {
        public TsExpressionSyntax Expression { get; }

        public TsParenthesizedExpressionSyntax(TsSyntaxNode parent, TsExpressionSyntax expression) : base(parent)
        {
            Expression = expression;
        }

        public override string ToDisplayString()
        {
            return $"({Expression.ToDisplayString()})";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }
    }
}
