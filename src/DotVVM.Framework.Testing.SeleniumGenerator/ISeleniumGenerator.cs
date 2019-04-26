using System;

namespace DotVVM.Framework.Testing.SeleniumGenerator
{
    public interface ISeleniumGenerator
    {
        Type ControlType { get; }

        void AddDeclarations(PageObjectDefinition pageObject, SeleniumGeneratorContext context);

        bool CanAddDeclarations(PageObjectDefinition pageObject, SeleniumGeneratorContext context);
    }
}