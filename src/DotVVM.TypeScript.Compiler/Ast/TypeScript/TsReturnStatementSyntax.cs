using System;
using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsReturnStatementSyntax : TsStatementSyntax, IReturnStatementSyntax
    {
        public IExpressionSyntax Expression { get; }

        public TsReturnStatementSyntax(ISyntaxNode parent, IExpressionSyntax expression) : base(parent)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public override string ToDisplayString()
        {
            var output = "return";
            if (Expression != null)
                output += Expression.ToDisplayString();
            return output;
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            throw new System.NotImplementedException();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitReturnStatement(this);
        }
    }
}
