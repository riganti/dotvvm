using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample40ViewModel
    {

        public Sample40ViewModelChild Child1 { get; set; }

        public Sample40ViewModelChild Child2 { get; set; }


        [Bind(Direction.ServerToClient)]
        public bool Validated { get; set; }


        public Sample40ViewModel()
        {
            Child1 = new Sample40ViewModelChild()
            {
                Child = new Sample40ViewModelChild()
            };
            Child2 = new Sample40ViewModelChild()
            {
                Child = new Sample40ViewModelChild()
            };
        }

        public void Validate()
        {
            Validated = true;
        }
    }

    public class Sample40ViewModelChild
    {

        [Required]
        public string Text { get; set; }

        public Sample40ViewModelChild Child { get; set; }

    }
}