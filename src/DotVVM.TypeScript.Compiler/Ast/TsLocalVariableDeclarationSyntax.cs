using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsLocalVariableDeclarationSyntax : TsStatementSyntax
    {
        public IList<TsVariableDeclaratorSyntax> Declarators { get; }

        public TsLocalVariableDeclarationSyntax(TsSyntaxNode parent, IList<TsVariableDeclaratorSyntax> declarators) : base(parent)
        {
            Declarators = declarators;
        }

        public override string ToDisplayString()
        {
            return $"let {Declarators.Select(d => d.ToDisplayString()).StringJoin(",")}";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Declarators;
        }

        public override void AcceptVisitor(ITsNodeVisitor visitor)
        {
            visitor.VisitLocalVariableDeclaration(this);
        }
    }
}
