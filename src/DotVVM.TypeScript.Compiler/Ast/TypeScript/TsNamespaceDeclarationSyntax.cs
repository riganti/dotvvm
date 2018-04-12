using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsNamespaceDeclarationSyntax : TsSyntaxNode, INamespaceDeclarationSyntax
    {
        public IIdentifierSyntax Identifier { get; }
        public IList<IClassDeclarationSyntax> Types { get; }

        public TsNamespaceDeclarationSyntax(ISyntaxNode parent, IIdentifierSyntax identifier,
            IList<IClassDeclarationSyntax> types) : base(parent)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Types = types ?? throw new ArgumentNullException(nameof(types));
        }

        public void AddClass(IClassDeclarationSyntax @class)
        {
            Types.Add(@class);
        }

        public override string ToDisplayString()
        {
            return $"namespace {Identifier.ToDisplayString()} {{" +
                   $"{Types.Select(t => t.ToDisplayString()).StringJoin("\n")}" +
                   $"\n}}";
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Types;
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitNamespaceDeclaration(this);
        }
    }
}
