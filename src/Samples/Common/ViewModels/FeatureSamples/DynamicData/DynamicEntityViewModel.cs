using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.Common.Resources;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.DynamicData
{
    public class DynamicEntityViewModel : DotvvmViewModelBase
    {

        public AddressDTO Address { get; set; } = new();

        public SelectorViewModel<StateSelectorItem, AddressDTO> States { get; set; }

        public SelectorViewModel<CountrySelectorItem> Countries { get; set; }

        public DynamicEntityViewModel()
        {
            States = new(() => Address);
            Countries = new();
        }
    }

    public class AddressDTO
    {

        [Display(GroupName = "BasicInfo")]
        public string Name { get; set; }

        [Display(GroupName = "BasicInfo")]
        public bool IsCompany { get; set; }

        [Display(GroupName = "BasicInfo")]
        public string Street { get; set; }

        [Display(GroupName = "BasicInfo")]
        public string City { get; set; }

        [Display(GroupName = "BasicInfo")]
        [Selector(typeof(StateSelectorItem))]
        public string State { get; set; }

        [Display(GroupName = "BasicInfo")]
        [Selector(typeof(CountrySelectorItem))]
        public int CountryId { get; set; }



        [Display(GroupName = "ContactInfo")]
        public string Email { get; set; }

        [Display(GroupName = "ContactInfo")]
        public string Phone { get; set; }

    }

    public record CountrySelectorItem : SelectorItem<int>;
    public record StateSelectorItem : SelectorItem<string>;

    public class CountrySelectorDataProvider : ISelectorDataProvider<CountrySelectorItem>
    {
        public Task<List<CountrySelectorItem>> GetItems() => Task.FromResult(new List<CountrySelectorItem>()
        {
            new CountrySelectorItem() { Id = 1, DisplayName = "USA" },
            new CountrySelectorItem() { Id = 2, DisplayName = "Czech Republic" }
        });
    }

    public class StateSelectorDataProvider : ISelectorDataProvider<StateSelectorItem, AddressDTO>
    {
        public Task<List<StateSelectorItem>> GetItems(AddressDTO parameter)
        {
            if (parameter.CountryId == 1)
            {
                return Task.FromResult(new List<StateSelectorItem>() {
                    new StateSelectorItem() { Id = "AL", DisplayName = "Alabama" },
                    new StateSelectorItem() { Id = "AK", DisplayName = "Alaska" },
                    new StateSelectorItem() { Id = "AZ", DisplayName = "Arizona" },
                    new StateSelectorItem() { Id = "AZ", DisplayName = "Arkansas" },
                    new StateSelectorItem() { Id = "CA", DisplayName = "California" }
                });
            }
            else
            {
                return Task.FromResult(new List<StateSelectorItem>());
            }
        }
    }
}

