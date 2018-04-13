using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsParameterSyntax : TsSyntaxNode, IParameterSyntax
    {
        public IIdentifierSyntax Identifier { get; }
        public ITypeSyntax Type { get; }


        public TsParameterSyntax(ISyntaxNode parent, IIdentifierSyntax identifier, ITypeSyntax type) : base(parent)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitParameter(this);
        }
    }
}
