using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators
{
    public interface ISeleniumGenerator
    {

        void AddDeclarations(HelperDefinition helper, SeleniumGeneratorContext context);

        bool CanAddDeclarations(HelperDefinition helperDefinition, SeleniumGeneratorContext context);
    }
}