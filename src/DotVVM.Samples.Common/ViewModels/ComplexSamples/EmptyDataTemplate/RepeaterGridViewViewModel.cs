using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.EmptyDataTemplate
{
    public class RepeaterGridViewViewModel : DotvvmViewModelBase
    {
        public List<Customer> Null => null;

        public List<Customer> Empty => new List<Customer>();

        public bool Visible { get; set; } = true;

        public List<Customer> NonEmpty => new List<Customer>()
        {
            new Customer() { FirstName = "Tomas" }
        };
    }

    public class Customer
    {
        public string FirstName { get; set; }
    }
}
