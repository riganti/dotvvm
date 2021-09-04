using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.MarkupControl
{
    public static class TestService
    {
        [AllowStaticCommand]
        public static Task<string> Ok()
        {
            return Task.FromResult("Command result.");
        }
    }

    public class CommandPropertiesInMarkupControlViewModel : DotvvmViewModelBase
    {
        public string Property { get; set; }
    }
}

