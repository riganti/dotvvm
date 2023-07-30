using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using DotVVM.Samples.Common.Controls;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomPrimitiveTypes
{
    public class UsedInControlsViewModel : DotvvmViewModelBase
    {

        public Point Point { get; set; } = new Point() { X = 1, Y = 2 };

    }
}

