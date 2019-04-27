using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Testing.SeleniumGenerator
{
    public class PageObjectDefinition
    {
        public string Name { get; set; }

        public List<string> DataContextPrefixes { get; set; } = new List<string>();

        public HashSet<string> UsedNames { get; } = new HashSet<string>();

        public HashSet<string> ExistingUsedSelectors { get; } = new HashSet<string>();

        public List<MemberDeclarationSyntax> Members { get; } = new List<MemberDeclarationSyntax>();

        public List<StatementSyntax> ConstructorStatements { get; } = new List<StatementSyntax>();

        public List<PageObjectDefinition> Children { get; } = new List<PageObjectDefinition>();

        public List<MarkupFileModification> MarkupFileModifications { get; } = new List<MarkupFileModification>();
    }

    public class MasterPageObjectDefinition : PageObjectDefinition
    {
        public string MasterPageFullPath { get; set; }
    }
}
