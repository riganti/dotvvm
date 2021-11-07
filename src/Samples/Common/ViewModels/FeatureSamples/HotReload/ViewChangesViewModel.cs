using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.HotReload
{
    public class ViewChangesViewModel : DotvvmViewModelBase
    {

        public string Value { get; set; }

        public string ViewFileName => Path.Combine(Context.Configuration.ApplicationPhysicalPath, Context.View.GetValue<string>(Internal.MarkupFileNameProperty));
    }
}

