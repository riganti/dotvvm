using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.IncludeInPageProperty
{
    public class IncludeInPageViewModel : DotvvmViewModelBase
    {
        public bool IncludeInPage { get; set; } = true;

        public bool Visible { get; set; } = true;

        public string Text { get; set; } = "Default text";

        public List<string> Texts { get; set; } =
            new List<string> { "Test 1", "Test 2", "Test 3" };

        public string[][] Rows { get; set; } = new[]
        {
            new []
            {
                "Red",
                "Green",
                "Blue"
            },
            new []
            {
                "Cyan",
                "Magenta",
                "Yellow",
                "Black"
            }
        };

        public GridViewDataSet<Customer> Customers { get; set; } = new GridViewDataSet<Customer>
        {
            Items =
            {
                new Customer {Id = 1, Name= "John Smith"},
                new Customer {Id = 2, Name= "Cave Johnson"},
                new Customer {Id=3, Name="Harry Callaghan"}
            }
        };

        public GridViewDataSet<Customer> EmptyCustomers { get; set; }

        public class Customer
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}

