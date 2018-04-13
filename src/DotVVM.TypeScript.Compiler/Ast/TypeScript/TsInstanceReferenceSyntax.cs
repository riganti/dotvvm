using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    class TsInstanceReferenceSyntax : TsReferenceSyntax,  IInstanceReferenceSyntax
    {
        public TsInstanceReferenceSyntax(ISyntaxNode parent) : base(parent)
        {
            Identifier = new TsIdentifierSyntax("this", parent);
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<ISyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitInstanceReference(this);
        }

    }
}
