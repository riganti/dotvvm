using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample18ViewModel : DotvvmViewModelBase
    {

        public Sample18ChildViewModel Child { get; set; }

        public Sample18ViewModel()
        {
            Child = new Sample18ChildViewModel();
        }

        public void Test()
        {

        }
    }

    public class Sample18ChildViewModel
    {
        [Required]
        public string Text { get; set; }
    }
}