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

        public void SetNull() => SampleItem = null;
        public void SetFirst() => SampleItem = SampleEnum.First;
        public void SetSecond() => SampleItem = SampleEnum.Second;
    }
}
