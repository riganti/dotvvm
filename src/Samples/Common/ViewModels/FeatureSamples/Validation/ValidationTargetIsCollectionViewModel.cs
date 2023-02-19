using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Validation
{
    public class ValidationTargetIsCollectionViewModel : DotvvmViewModelBase
    {
        public List<Customer> Customers { get; set; }

        public ValidationTargetIsCollectionViewModel()
        {
            Customers = new List<Customer>()
            {
                new Customer() { Id = 0, Name = "Alice" },
                new Customer() { Id = 1 },
                new Customer() { Id = 2, Name = "Charlie" },
                new Customer() { Id = 3 },
                new Customer() { Id = 4, Name = "Erin" }
            };
        }

        public void Method()
        {
            /* Trigger automatic validation */
        }

        public class Customer
        {
            [Required]
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }
        }
    }
}
