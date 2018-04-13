using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsLocalVariableDeclarationSyntax : TsStatementSyntax, ILocalVariableDeclarationSyntax
    {
        public IList<IVariableDeclaratorSyntax> Declarators { get; }

        public TsLocalVariableDeclarationSyntax(ISyntaxNode parent, IList<IVariableDeclaratorSyntax> declarators) :
            base(parent)
        {
            Declarators = declarators ?? throw new ArgumentNullException(nameof(declarators));
        }
        
        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Declarators;
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitLocalVariableDeclaration(this);
        }
    }
}
