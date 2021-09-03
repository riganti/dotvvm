using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.JavascriptTranslation
{
    public class MathMethodTranslationViewModel : DotvvmViewModelBase
    {
        public double DArg1 { get; set; } = 0.0;
        public double DArg2 { get; set; } = 0.0;

        public int IArg1 { get; set; } = 0;
        public int IArg2 { get; set; } = 0;

        public double Result { get; set; } = 0.0;
    }
}

