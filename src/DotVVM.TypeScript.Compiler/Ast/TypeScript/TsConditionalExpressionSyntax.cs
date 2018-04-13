using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsConditionalExpressionSyntax : TsExpressionSyntax, IConditionalExpressionSyntax
    {
        public IExpressionSyntax Condition { get; }
        public IExpressionSyntax WhenTrue { get; }
        public IExpressionSyntax WhenFalse { get; }

        public TsConditionalExpressionSyntax(ISyntaxNode parent, IExpressionSyntax condition,
            IExpressionSyntax whenTrue, IExpressionSyntax whenFalse) : base(parent)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            WhenTrue = whenTrue ?? throw new ArgumentNullException(nameof(whenTrue));
            WhenFalse = whenFalse ?? throw new ArgumentNullException(nameof(whenFalse));
        }
        
        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitConditionalExpression(this);
        }
    }
}
