using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast.Visitors;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsTypeSyntax : TsSyntaxNode, ITypeSyntax
    {
        public ITypeSymbol EquivalentSymbol { get; }

        public TsTypeSyntax(ITypeSymbol equivalentSymbol, ISyntaxNode parent) : base(parent)
        {
            EquivalentSymbol = equivalentSymbol ?? throw new ArgumentNullException(nameof(equivalentSymbol));
        }
        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitType(this);
        }
    }
}
