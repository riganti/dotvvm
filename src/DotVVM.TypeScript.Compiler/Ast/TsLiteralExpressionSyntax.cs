using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsLiteralExpressionSyntax : TsExpressionSyntax
    {
        string Value { get; }

        public TsLiteralExpressionSyntax(TsSyntaxNode parent, string value) : base(parent)
        {
            Value = value;
        }

        public override string ToDisplayString()
        {
            return Value;
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }
    }
}