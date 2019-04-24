using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators
{
    public interface ISeleniumGenerator
    {
        Type ControlType { get; }

        void AddDeclarations(PageObjectDefinition pageObject, SeleniumGeneratorContext context);

        bool CanAddDeclarations(PageObjectDefinition pageObject, SeleniumGeneratorContext context);
    }
}