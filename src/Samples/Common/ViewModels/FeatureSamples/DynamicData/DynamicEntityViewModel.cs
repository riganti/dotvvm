using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls.DynamicData.Annotations;
using DotVVM.Framework.Controls.DynamicData.ViewModel;
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
                    new StateSelectorItem() { Id = "CA", DisplayName = "California" },
                    new StateSelectorItem() { Id = "CO", DisplayName = "Colorado" },
                    new StateSelectorItem() { Id = "CT", DisplayName = "Connecticut" },
                    new StateSelectorItem() { Id = "DE", DisplayName = "Delaware" },
                    new StateSelectorItem() { Id = "FL", DisplayName = "Florida" },
                    new StateSelectorItem() { Id = "GA", DisplayName = "Georgia" },
                    new StateSelectorItem() { Id = "HI", DisplayName = "Hawaii" },
                    new StateSelectorItem() { Id = "ID", DisplayName = "Idaho" },
                    new StateSelectorItem() { Id = "IL", DisplayName = "Illinois" },
                    new StateSelectorItem() { Id = "IN", DisplayName = "Indiana" },
                    new StateSelectorItem() { Id = "IA", DisplayName = "Iowa" },
                    new StateSelectorItem() { Id = "KS", DisplayName = "Kansas" },
                    new StateSelectorItem() { Id = "KY", DisplayName = "Kentucky" },
                    new StateSelectorItem() { Id = "LA", DisplayName = "Louisiana" },
                    new StateSelectorItem() { Id = "ME", DisplayName = "Maine" },
                    new StateSelectorItem() { Id = "MD", DisplayName = "Maryland" },
                    new StateSelectorItem() { Id = "MA", DisplayName = "Massachusetts" },
                    new StateSelectorItem() { Id = "MI", DisplayName = "Michigan" },
                    new StateSelectorItem() { Id = "MN", DisplayName = "Minnesota" },
                    new StateSelectorItem() { Id = "MS", DisplayName = "Mississippi" },
                    new StateSelectorItem() { Id = "MO", DisplayName = "Missouri" },
                    new StateSelectorItem() { Id = "MT", DisplayName = "Montana" },
                    new StateSelectorItem() { Id = "NE", DisplayName = "Nebraska" },
                    new StateSelectorItem() { Id = "NV", DisplayName = "Nevada" },
                    new StateSelectorItem() { Id = "NH", DisplayName = "New Hampshire" }
                });
            }
            else
            {
                return Task.FromResult(new List<StateSelectorItem>());
            }
        }
    }
}

