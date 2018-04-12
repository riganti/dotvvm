using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsAssignmentSyntax : TsStatementSyntax, IAssignmentSyntax
    {
        public IReferenceSyntax Reference { get;  }
        public IExpressionSyntax Expression { get; }
        
        public TsAssignmentSyntax(ISyntaxNode parent, IReferenceSyntax reference, IExpressionSyntax expression) : base(parent)
        {
            Reference = reference ?? throw new ArgumentNullException(nameof(reference));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public override string ToDisplayString()
        {
            return $"{Reference.ToDisplayString()} = {Expression.ToDisplayString()}";
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitAssignmentStatement(this);
        }
    }
}
