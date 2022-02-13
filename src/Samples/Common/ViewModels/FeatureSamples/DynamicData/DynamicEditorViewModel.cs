using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.DependencyInjection;

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


    public class SelectorViewModel<TItem> : DotvvmViewModelBase, ISelectorViewModel<TItem>
        where TItem : SelectorItem
    {

        public List<TItem>? Items { get; set; }

        public override async Task PreRender()
        {
            if (Items == null)
            {
                await LoadItems();
            }
            await base.PreRender();
        }

        protected virtual async Task LoadItems()
        {
            var selectorDataProvider = Context.Services.GetService<ISelectorDataProvider<TItem>>();
            if (selectorDataProvider != null)
            {
                Items = await selectorDataProvider.GetItems();
            }
            else
            {
                throw new DotvvmControlException($"Cannot resolve ISelectorDataProvider<{typeof(TItem).FullName}> service! Either load data into {GetType()}.Items collection manually, or register a service which can provide the selector items.");
            }
        }
    }

    public class SelectorViewModel<TItem, TParam> : SelectorViewModel<TItem>
        where TItem : SelectorItem
    {
        private readonly Func<TParam> parameterProvider;

        public SelectorViewModel(Func<TParam> parameterProvider)
        {
            this.parameterProvider = parameterProvider;
        }

        protected override async Task LoadItems()
        {
            var selectorDataProvider = Context.Services.GetService<ISelectorDataProvider<TItem, TParam>>();
            if (selectorDataProvider != null)
            {
                var parameter = parameterProvider();
                Items = await selectorDataProvider.GetItems(parameter);
            }
            else
            {
                throw new DotvvmControlException($"Cannot resolve ISelectorDataProvider<{typeof(TItem).FullName}> service! Either load data into {GetType()}.Items collection manually, or register a service which can provide the selector items.");
            }
        }
    }

    public interface ISelectorDataProvider<TItem>
    {
        Task<List<TItem>> GetItems();
    }

    public interface ISelectorDataProvider<TItem, TParam>
    {
        Task<List<TItem>> GetItems(TParam parameter);
    }

    public interface ISelectorViewModel<TItem>
        where TItem : SelectorItem
    {
        List<TItem> Items { get; }
    }

    public abstract record SelectorItem<TKey> : SelectorItem
    {
        public TKey Id { get; set; }

        private protected override void SorryWeCannotAllowYouToInheritThisClass() => throw new NotImplementedException("Mischief managed.");
    }

    public abstract record SelectorItem
    {
        public string DisplayName { get; set; }

        private protected abstract void SorryWeCannotAllowYouToInheritThisClass();
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SelectorAttribute : System.Attribute
    {
        public Type PropertyType { get; }

        public SelectorAttribute(Type propertyType)
        {
            PropertyType = propertyType;
        }
    }
}

