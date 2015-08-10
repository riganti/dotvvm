using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample26ViewModel : DotvvmViewModelBase
    {

        public Sample26ChildViewModel Child { get; set; }

        public Sample26ViewModel()
        {
            Child = new Sample26ChildViewModel();
        }

        public void Test()
        {

        }
    }

    public class Sample26ChildViewModel
    {
        [Required]
        public string Text { get; set; }

        [Required]
        [EmailAddress]
        public string Text2 { get; set; }
    }
}