using System.Collections.Generic;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.Repeater
{
    public class NestedRepeaterEntry
    {
        public string Name { get; set; }

        public List<NestedRepeaterEntry> Children { get; set; }

        public NestedRepeaterEntry Entry { get; set; }
    }
}
