using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DotVVM.Framework.Testing.Generator.Modifications;

namespace DotVVM.Framework.Testing.Generator
{
    public abstract class PageObjectDefinition
    {
        protected PageObjectDefinition()
        {

        }
        
        public string Name { get; protected set; }

        public List<string> DataContextPrefixes { get; set; } = new List<string>();

        public HashSet<string> UsedNames { get; } = new HashSet<string>();

        public HashSet<string> ExistingUsedSelectors { get; } = new HashSet<string>();

        public List<MemberDeclarationSyntax> Members { get; } = new List<MemberDeclarationSyntax>();

        public List<StatementSyntax> ConstructorStatements { get; } = new List<StatementSyntax>();

        public List<PageObjectDefinition> Children { get; } = new List<PageObjectDefinition>();

        public List<MarkupFileModification> MarkupFileModifications { get; } = new List<MarkupFileModification>();
        public string Namespace { get; protected set; }
    }
}
