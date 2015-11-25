using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ViewModelDeserialization
{
    public class DoesNotDropObjectViewModel : DotvvmViewModelBase
    {
        public DataContextObject Object { get; set; } = new DataContextObject();
        public void Postback()
        {
            Object.Property++;
        }
        public class DataContextObject
        {
            public int Property { get; set; }
        }
    }
}
