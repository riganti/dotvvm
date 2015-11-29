using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample46ViewModel
    {
        public List<Sample46Customer> Null => null;

        public List<Sample46Customer> Empty => new List<Sample46Customer>();

        public List<Sample46Customer> NonEmpty => new List<Sample46Customer>()
        {
            new Sample46Customer() { FirstName = "Tomas" }
        };

    }

    public class Sample46Customer
    {
        public string FirstName { get; set; }
    }
}