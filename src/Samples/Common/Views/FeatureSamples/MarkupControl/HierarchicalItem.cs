using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Samples.Common.Views.FeatureSamples.MarkupControl
{
    public class HierarchicalItem
    {
        public string Title { get; set; }
        public List<HierarchicalItem> Children { get; set; } = new List<HierarchicalItem>();
    }
}
