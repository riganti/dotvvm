using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.CheckBox
{
    public class CheckBoxCheckedValueUpdatesViewModel : DotvvmViewModelBase
    {
        public List<int> SelectedValues { get; set; } = new() { 2 };

        public int FirstValue { get; set; } = 1;
    }
}

