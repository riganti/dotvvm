using System.Collections.Generic;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.MultiSelect
{
    public class MultiSelectViewModel
    {
        public IEnumerable<string> SelectedValues { get; set; }
        public IEnumerable<string> Values => new[] { "Praha", "Brno", "Napajedla" };

        public int ChangedCount { get; set; }

        public void OnSelectionChanged()
        {
            ChangedCount++;
        }
    }
}
