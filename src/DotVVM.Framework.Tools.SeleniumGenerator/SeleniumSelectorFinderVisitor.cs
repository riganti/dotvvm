using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class SeleniumSelectorFinderVisitor : ResolvedControlTreeVisitor
    {
        private readonly HashSet<string> selectors = new HashSet<string>();

        public override void VisitControl(ResolvedControl control)
        {
            var selector = TryGetNameFromProperty(control, UITests.NameProperty);
            if (selector != null)
            {
                selectors.Add(selector);
            }

            base.VisitControl(control);
        }

        protected string TryGetNameFromProperty(ResolvedControl control, DotvvmProperty property)
        {
            if (control.TryGetProperty(property, out var setter))
            {
                switch (setter)
                {
                    case ResolvedPropertyValue propertySetter:
                        return propertySetter.Value?.ToString();

                    case ResolvedPropertyBinding propertyBinding:
                    {
                        return propertyBinding.Binding.Value;
                    }
                }
            }
            return null;
        }

        public HashSet<string> GetResult()
        {
            return selectors;
        }
    }
}
