using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsClassDeclarationSyntax : TsSyntaxNode, IClassDeclarationSyntax
    {
        public TsModifier Modifier { get; }
        public IIdentifierSyntax Identifier { get; set; }
        public IList<IMemberDeclarationSyntax> Members { get; }
        public IList<IIdentifierSyntax> BaseClasses { get; }

        public TsClassDeclarationSyntax(IIdentifierSyntax identifier, IList<IMemberDeclarationSyntax> members, IList<IIdentifierSyntax> baseClasses, ISyntaxNode parent) : base(parent)
        {
            Identifier = identifier;
            Members = members;
            BaseClasses = baseClasses;
        }

        public override string ToDisplayString()
        {
            var output = $"class {Identifier} ";
            if (BaseClasses.Any())
            {
                output += $"extends {BaseClasses.Select(i => i.ToDisplayString()).StringJoin(",")}";
            }
            output += " {\n";
            foreach (var member in Members)
            {
                output += $"\t{member.ToDisplayString()}\n";
            }
            output += "\n}";
            return output;
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
