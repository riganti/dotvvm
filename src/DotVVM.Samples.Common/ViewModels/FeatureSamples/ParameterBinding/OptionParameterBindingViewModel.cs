using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ParameterBinding
{
    public class OptionParameterBindingViewModel : DotvvmViewModelBase
    {
        public override Task Init()
        {
            Param = string.Join("|", Context.Parameters.Select(s => s.Key + ":" + s.Value).ToList());
            return base.Init();
        }

        public string Param { get; set; }
    }
}

