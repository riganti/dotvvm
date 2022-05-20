using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.AutoUI.Annotations;
using DotVVM.AutoUI.ViewModel;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.AutoUI
{
    public class DynamicEditorViewModel : DotvvmViewModelBase
    {

        public CustomerData Customer { get; set; } = new();

        public SelectorViewModel<ProductSelection> Products { get; set; } = new();
    }

    public class CustomerData
    {
        public string StringProp { get; set; }

        public int IntProp { get; set; }

        [Range(0, 10)]
        public int IntRangeProp { get; set; }

        public bool BoolProp { get; set; }

        public DateTime DateTimeProp { get; set; }

        public Guid ProductId { get; set; }

        public ServiceType ServiceType { get; set; }

    }

    public record ProductSelection : Selection<Guid>;

    public class ProductSelectionProvider : ISelectionProvider<ProductSelection>
    {
        public Task<List<ProductSelection>> GetSelectorItems() =>
            Task.FromResult(new List<ProductSelection>()
            {
                new ProductSelection() { Value = new Guid("00000000-0000-0000-0000-000000000001"), DisplayName = "First product" },
                new ProductSelection() { Value = new Guid("00000000-0000-0000-0000-000000000002"), DisplayName = "Second product" },
                new ProductSelection() { Value = new Guid("00000000-0000-0000-0000-000000000003"), DisplayName = "Third product" }
            });
    }

    public enum ServiceType
    {
        [Display(Name = "Development work")]
        Development,

        [Display(Name = "Services & maintenance")]
        Support
    }

}

