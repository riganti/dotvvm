using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Serialization
{
    public class ByteArrayViewModel : DotvvmViewModelBase
    {
        public byte[] Bytes { get; set; } = new byte[] { 1, 2, 3, 4, 5 };
    }
}

