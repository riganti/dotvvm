using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsNamespaceDeclarationSyntax : TsSyntaxNode
    {
        public TsIdentifierSyntax Identifier { get; }
        public IList<TsClassDeclarationSyntax> Types { get; }

        public TsNamespaceDeclarationSyntax(TsSyntaxNode parent, TsIdentifierSyntax identifier, IList<TsClassDeclarationSyntax> types) : base(parent)
        {
            Identifier = identifier;
            Types = types;
        }

        public void AddClass(TsClassDeclarationSyntax @class)
        {
            Types.Add(@class);
        }

        public override string ToDisplayString()
        {
            return $"namespace {Identifier.ToDisplayString()} {{" +
                   $"{Types.Select(t => t.ToDisplayString()).StringJoin("\n")}" +
                   $"\n}}";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Types;
        }

        public override void AcceptVisitor(ITsNodeVisitor visitor)
        {
            visitor.VisitNamespaceDeclaration(this);
        }
    }
}
