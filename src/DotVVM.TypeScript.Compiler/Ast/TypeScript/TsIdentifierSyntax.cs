using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsIdentifierSyntax : TsSyntaxNode, IIdentifierSyntax
    {
        public string Value { get; }

        public TsIdentifierSyntax(string value, ISyntaxNode parent) : base(parent)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
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
            visitor.VisitIdentifier(this);
        }
    }
}
