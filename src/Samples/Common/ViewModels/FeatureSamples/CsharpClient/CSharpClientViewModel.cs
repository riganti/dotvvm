using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CsharpClient
{
    public class CSharpClientViewModel : DotvvmViewModelBase
    {
        public int Value { get; set; } = 1;

        public int? ReadResult { get; set; }

        public string LastConsole { get; set; }
    }
}

