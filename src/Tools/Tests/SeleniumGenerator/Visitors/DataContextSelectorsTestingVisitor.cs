using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Testing.SeleniumGenerator.Tests.Helpers;

namespace DotVVM.Testing.SeleniumGenerator.Tests.Visitors
{
    public class DataContextSelectorsTestingVisitor : SeleniumGeneratorTestsVisitor
    {
        public List<string> DataContextPrefixes { get; set; } = new List<string>();
        private readonly List<(string dataContext, string controlName, string selector)> controlSelectors = new List<(string, string, string)>();

        public override void VisitControl(ResolvedControl control)
        {
            var dataContextNameSet = false;
            if (control.TryGetProperty(DotvvmBindableObject.DataContextProperty, out var property))
            {
                if (property is ResolvedPropertyBinding dataContextProperty)
                {
                    var dataContextName = dataContextProperty.Binding.Value;
                    DataContextPrefixes.Add(dataContextName);
                    dataContextNameSet = true;
                }
            }

            var selector = VisitorHelper.TryGetNameFromProperty(control, UITests.NameProperty);
            if (selector != null)
            {
                controlSelectors.Add((string.Join("_", DataContextPrefixes), control.Metadata.Name, selector));
            }

            base.VisitControl(control);

            if (dataContextNameSet)
            {
                DataContextPrefixes.RemoveAt(DataContextPrefixes.Count - 1);
            }
        }

        public IList<DataContextControlSelectorResult> GetResult()
        {
            return controlSelectors
                .Where(c => !string.IsNullOrEmpty(c.dataContext))
                .Select(c => new DataContextControlSelectorResult
                {
                    DataContext = c.dataContext,
                    ControlName = c.controlName,
                    Selector = c.selector
                })
                .ToList();
        }
    }

    public class DataContextControlSelectorResult
    {
        public string DataContext { get; set; }

        public string ControlName { get; set; }

        public string Selector { get; set; }
    }
}
