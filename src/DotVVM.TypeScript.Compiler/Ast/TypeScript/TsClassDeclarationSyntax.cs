using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsClassDeclarationSyntax : TsSyntaxNode, IClassDeclarationSyntax
    {
        public IIdentifierSyntax Identifier { get; set; }
        public IList<IMemberDeclarationSyntax> Members { get; }
        public IList<IIdentifierSyntax> BaseClasses { get; }


        public TsClassDeclarationSyntax(ISyntaxNode parent, IIdentifierSyntax identifier, IList<IMemberDeclarationSyntax> members, IList<IIdentifierSyntax> baseClasses) : base(parent)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Members = members ?? throw new ArgumentNullException(nameof(members));
            BaseClasses = baseClasses ?? throw new ArgumentNullException(nameof(baseClasses));
        }
        
        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Members;
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitClassDeclaration(this);
        }

        public void AddMember(TsMemberDeclarationSyntax member)
        {
            Members.Add(member);
        }
    }
}
