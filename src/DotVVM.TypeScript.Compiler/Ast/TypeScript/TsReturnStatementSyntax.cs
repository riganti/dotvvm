using System;
using System.Collections.Generic;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsReturnStatementSyntax : TsStatementSyntax, IReturnStatementSyntax
    {
        public IExpressionSyntax Expression { get; }

        public TsReturnStatementSyntax(ISyntaxNode parent, IExpressionSyntax expression) : base(parent)
        {
            Expression = expression;
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
