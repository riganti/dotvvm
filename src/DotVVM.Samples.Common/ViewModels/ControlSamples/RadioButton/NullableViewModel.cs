using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.RadioButton
{
    public class NullableViewModel
    {

        public enum SampleEnum
        {
            First,
            Second
        }

        public SampleEnum? SampleItem { get; set; }
    }
}
