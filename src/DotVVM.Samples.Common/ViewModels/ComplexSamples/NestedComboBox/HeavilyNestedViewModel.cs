using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.NestedComboBox
{
    public class HeavilyNestedViewModel
    {
        public InnerViewModel Inner { get; set; } = new InnerViewModel();

        public int? SelectedValue { get; set; }
    }
}
