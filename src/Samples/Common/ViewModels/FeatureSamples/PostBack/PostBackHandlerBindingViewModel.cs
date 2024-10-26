using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.PostBack
{
    public class PostBackHandlerBindingViewModel : DotvvmViewModelBase
    {
        public bool Enabled { get; set; } = false;

        public int Counter { get; set; } = 0;

        public string[] Items { get; set; } = new string[] { "Item 1", "Item 2", "Item 3" };
    }
}

