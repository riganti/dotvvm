using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.BindingVariables
{
    public class StaticCommandVariablesWithServiceViewModel : DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPA.SiteViewModel
    {
        public string Message { get; set; }

    }

    public class VariablesStaticCommand
    {
        [AllowStaticCommand]
        public Task<string> GetMessage()
        {
            return Task.FromResult("text");
        }

    }

}

