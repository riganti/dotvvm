using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsReturnStatementSyntax : TsStatementSyntax
    {
        public TsExpressionSyntax Expression { get; }

        public TsReturnStatementSyntax(TsSyntaxNode parent, TsExpressionSyntax expression) : base(parent)
        {
            Expression = expression;
        }

        public override string ToDisplayString()
        {
            var output = "return";
            if (Expression != null)
                output += Expression.ToDisplayString();
            return output;
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            throw new System.NotImplementedException();
        }

        public override void AcceptVisitor(ITsNodeVisitor visitor)
        {
            visitor.VisitReturnStatement(this);
        }
    }
}
