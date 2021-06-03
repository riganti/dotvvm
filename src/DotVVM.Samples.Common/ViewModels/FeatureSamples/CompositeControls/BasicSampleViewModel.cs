using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CompositeControls
{
    public class BasicSampleViewModel : DotvvmViewModelBase
    {
        public List<SampleItem> List { get; set; } = new List<SampleItem> {
            new SampleItem { EditableNumber = 43, Title = "First Number" },
            new SampleItem { EditableNumber = 12, Title = "Second Number" },
        };
        public bool Visible { get; set; }
        public bool Enabled { get; set; } = true;
    }

    public class SampleItem
    {
        public int EditableNumber { get; set; }
        public string Title { get; set; }
    }
}

