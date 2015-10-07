using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample38ViewModel : DotvvmViewModelBase
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