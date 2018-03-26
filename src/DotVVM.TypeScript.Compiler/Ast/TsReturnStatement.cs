using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsReturnStatement : TsStatementSyntax
    {
        TsExpressionSyntax Expression { get; }

        public TsReturnStatement(TsSyntaxNode parent, TsExpressionSyntax expression = null) : base(parent)
        {
            Expression = expression;
        }

        public override string ToDisplayString()
        {
            return $"return { (Expression != null ? Expression.ToDisplayString() : string.Empty)}";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            throw new System.NotImplementedException();
        }
    }
}