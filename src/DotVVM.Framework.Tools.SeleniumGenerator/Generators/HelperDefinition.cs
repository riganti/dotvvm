using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators
{
    public class HelperDefinition
    {

        public string Name { get; set; }

        public List<MemberDeclarationSyntax> Members { get; } = new List<MemberDeclarationSyntax>();

        public List<StatementSyntax> ConstructorStatements { get; } = new List<StatementSyntax>();
        
        public List<HelperDefinition> Children { get; } = new List<HelperDefinition>();

    }
}
