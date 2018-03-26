using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsClassDeclarationSyntax : TsSyntaxNode
    {
        public TsIdentifierSyntax Identifier { get; }
        public IList<TsMemberDeclarationSyntax> Members { get; }
        public IList<TsIdentifierSyntax> BaseClasses { get; }

        public TsClassDeclarationSyntax(TsIdentifierSyntax identifier, IList<TsMemberDeclarationSyntax> members, IList<TsIdentifierSyntax> baseClasses, TsSyntaxNode parent) : base(parent)
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

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Members;
        }

        public void AddMember(TsMemberDeclarationSyntax member)
        {
            Members.Add(member);
        }
    }
}
