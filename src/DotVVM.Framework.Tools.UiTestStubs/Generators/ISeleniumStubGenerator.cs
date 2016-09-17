using System.Collections.Generic;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.UiTestStubs.Handlers
{
    public interface ISeleniumStubGenerator<TControl> where TControl : DotvvmControl
    {

        IEnumerable<StubDeclaration> GetDeclarations(StubGeneratorContext context);

    }
}