using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.Literal
{
    public class Literal_NumberBindingViewModel
    {
        public IEnumerable<int> Numbers { get; set; } = new List<int>() {
            1,2,3,4,5,6,7,8
        };
        public IEnumerable<string> Texts { get; set; } = new List<string>() {
            "a", "b", "c", "d"
        };
    }
}
