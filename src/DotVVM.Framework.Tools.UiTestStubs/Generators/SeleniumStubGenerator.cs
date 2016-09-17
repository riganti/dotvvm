using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.UiTestStubs.Handlers
{
    public class SeleniumStubGenerator<TControl> : ISeleniumStubGenerator<TControl> where TControl : DotvvmControl
    {

        public IEnumerable<StubDeclaration> GetDeclarations(StubGeneratorContext context)
        {
            var requestedName = GetRequestedName(context.Control);
            yield break;
        }

        private string GetRequestedName(ResolvedControl control)
        {
            IAbstractPropertySetter setter;
            if (control.TryGetProperty(UITests.NameProperty, out setter)
                && setter is ResolvedPropertyValue)
            {
                return ((ResolvedPropertyValue)setter).Value as string;
            }
            return null;
        }
    }
}