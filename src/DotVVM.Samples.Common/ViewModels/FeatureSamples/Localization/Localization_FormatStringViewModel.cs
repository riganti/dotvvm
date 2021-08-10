using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using Microsoft.CodeAnalysis;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Localization
{
    public class Localization_FormatStringViewModel : DotvvmViewModelBase
    {
        public string CultureCode { get; set; }
        public override Task PreRender()
        {
            CultureCode = CultureInfo.CurrentCulture.Name;
            return base.PreRender();
        }
    }
}

