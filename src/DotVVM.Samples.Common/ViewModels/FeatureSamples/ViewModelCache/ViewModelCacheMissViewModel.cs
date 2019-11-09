using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ViewModelCache
{
    public class ViewModelCacheMissViewModel : DotvvmViewModelBase
    {

        public int Value { get; set; }
        
        public void Increment()
        {
            Value++;
        }

    }
}
