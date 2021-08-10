using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPA
{
	public class TestViewModel : SiteViewModel
	{
        [Required]
        public string Name { get; set; }
    }
}

