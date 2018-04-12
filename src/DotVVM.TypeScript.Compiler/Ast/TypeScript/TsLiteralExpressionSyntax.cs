using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsLiteralExpressionSyntax : TsExpressionSyntax, ILiteralExpressionSyntax
    {
        public string Value { get; }

        public TsLiteralExpressionSyntax(ISyntaxNode parent, string value) : base(parent)
        {
            Value = value;
        }

        public override string ToDisplayString()
        {
            return Value;
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitLiteral(this);
        }
    }
}
