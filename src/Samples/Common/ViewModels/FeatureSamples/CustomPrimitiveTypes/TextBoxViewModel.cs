using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using DotVVM.Samples.Common.Controls;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomPrimitiveTypes
{
    public class TextBoxViewModel : DotvvmViewModelBase
    {

        public Point Point { get; set; } = new Point() { X = 15, Y = 32 };

        [Required]
        [RegularExpression(@"^\d+,\d+$")]
        public Point Null { get; set; }

    }
}

