using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Testing.SeleniumGenerator.Tests.Helpers;
using DotVVM.Testing.SeleniumGenerator.Tests.Visitors;

namespace DotVVM.Testing.SeleniumGenerator.Tests.Visitors
{
    public class UiNamesTestingVisitor : SeleniumGeneratorTestsVisitor
    {
        private readonly List<(string, string)> controlSelectors = new List<(string, string)>();

        public override void VisitControl(ResolvedControl control)
        {
            var selector = VisitorHelper.TryGetNameFromProperty(control, UITests.NameProperty);
            if (selector != null)
            {
                controlSelectors.Add((control.Metadata.Name, selector));
            }

            base.VisitControl(control);
        }

        public List<(string, string)> GetResult()
        {
            return controlSelectors;
        }
    }
}
