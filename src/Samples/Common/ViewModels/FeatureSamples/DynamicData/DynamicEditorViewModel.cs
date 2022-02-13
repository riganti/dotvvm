using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.DynamicData.Annotations;
using DotVVM.Framework.Controls.DynamicData.ViewModel;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using SelectorItem = DotVVM.Framework.Controls.DynamicData.Annotations.SelectorItem;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.DynamicData
{
    public class DynamicEditorViewModel : DotvvmViewModelBase
    {

        public CustomerData Customer { get; set; } = new();

        public SelectorViewModel<ProductSelectorItem> Products { get; set; } = new();
    }

    public class CustomerData
    {
        public string StringProp { get; set; }

        public int IntProp { get; set; }

        [Range(0, 10)]
        public int IntRangeProp { get; set; }

        public bool BoolProp { get; set; }

        public bool DateTimeProp { get; set; }

        [Selector(typeof(ProductSelectorItem))]
        public int ProductId { get; set; }

        public ServiceType ServiceType { get; set; }

    }

    public record ProductSelectorItem : SelectorItem<Guid>;

    public class ProductSelectorDataProvider : ISelectorDataProvider<ProductSelectorItem>
    {
        public Task<List<ProductSelectorItem>> GetItems() =>
            Task.FromResult(new List<ProductSelectorItem>()
            {
                new ProductSelectorItem() { Id = new Guid("00000000-0000-0000-0000-000000000001"), DisplayName = "First product" },
                new ProductSelectorItem() { Id = new Guid("00000000-0000-0000-0000-000000000002"), DisplayName = "Second product" },
                new ProductSelectorItem() { Id = new Guid("00000000-0000-0000-0000-000000000003"), DisplayName = "Third product" }
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

