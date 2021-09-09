using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.JavascriptTranslation
{
    public class DateTimeTranslationsViewModel : DotvvmViewModelBase
    {
        public DateTime DateTimeProp { get; set; } = DateTime.UtcNow;
    }
}

