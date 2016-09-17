using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators
{
    public interface ISeleniumGenerator
    {

        IEnumerable<MemberDeclarationSyntax> GetDeclarations(SeleniumGeneratorContext context);

    }
}