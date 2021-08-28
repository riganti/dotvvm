using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.NestedComboBox
{
    public class InnerViewModel
    {
        public List<SampleDto> Data { get; set; } = new List<SampleDto> {
            new SampleDto{ Label = "First", Value = 1},
            new SampleDto{ Label = "Second", Value = 2}
        };
    }
}
