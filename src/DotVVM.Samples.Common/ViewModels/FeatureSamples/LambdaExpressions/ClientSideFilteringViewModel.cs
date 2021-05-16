using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.LambdaExpressions
{
    public class ClientSideFilteringViewModel : DotvvmViewModelBase
    {
        public string Filter { get; set; }

        public List<string> AllCategories { get; set; } = new List<string>() { "Red", "Green", "Blue" };

        public List<string> SelectedCategories { get; set; } = new List<string>();

        public List<CustomerData> Customers { get; } = new List<CustomerData>()
        {
            new CustomerData() {Id = 1, Name = "Alice", Category = "Red"},
            new CustomerData() {Id = 2, Name = "Dean", Category = "Green"},
            new CustomerData() {Id = 3, Name = "Everett", Category = "Blue"},
            new CustomerData() {Id = 4, Name = "Jenny", Category = "Blue"},
            new CustomerData() {Id = 5, Name = "Carl", Category = "Green"},
            new CustomerData() {Id = 6, Name = "Karen", Category = "Red"},
            new CustomerData() {Id = 7, Name = "John", Category = "Red"},
            new CustomerData() {Id = 8, Name = "Johnny", Category = "Red"},
            new CustomerData() {Id = 9, Name = "Robert", Category = "Green"},
            new CustomerData() {Id = 10, Name = "Roger", Category = "Blue"}
        };


    }

    public class CustomerData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
    }
}

