using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Utils;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsMethodCallSyntax : TsExpressionSyntax
    {
        public TsIdentifierSyntax Name { get; }
        public ImmutableList<TsExpressionSyntax> Parameters { get;  }

        public TsMethodCallSyntax(TsSyntaxNode parent, TsIdentifierSyntax name, ImmutableList<TsExpressionSyntax> parameters) : base(parent)
        {
            Name = name;
            Parameters = parameters;
        }

        public override string ToDisplayString()
        {
            return $"{Name.ToDisplayString()}({Parameters.Select(p => p.ToDisplayString()).StringJoin(",")})";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(ITsNodeVisitor visitor)
        {
            visitor.AcceptMethodCall(this);
        }
    }
}
