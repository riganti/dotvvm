using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.Repeater
{
    public class DataSourceNullViewModel : DotvvmViewModelBase
    {
        public List<string> Collection { get; set; }

        public void SetCollection()
        {
            Collection = new List<string>
            {
                "First",
                "Second",
                "Third",
            };
        }
    }
}
