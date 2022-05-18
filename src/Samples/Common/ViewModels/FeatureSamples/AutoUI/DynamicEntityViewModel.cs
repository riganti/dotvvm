using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.AutoUI.Annotations;
using DotVVM.AutoUI.ViewModel;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.AutoUI
{
    public class DynamicEntityViewModel : DotvvmViewModelBase
    {

        public AddressDTO Address { get; set; } = new() { CountryId = 1 };

        public SelectorViewModel<StateSelectorItem, AddressDTO> States { get; set; }

        public SelectorViewModel<CountrySelectorItem> Countries { get; set; }

        [Display(Name = "Some additional field.")]
        [MaxLength(10)]
        public string Something { get; set; } = "test";

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
        [Selector(typeof(CountrySelectorItem))]
        public int CountryId { get; set; }
        
        [Display(GroupName = "BasicInfo")]
        [Selector(typeof(StateSelectorItem))]
        public string State { get; set; }

        [Display(GroupName = "ContactInfo")]
        public string Email { get; set; }

        [Display(GroupName = "ContactInfo")]
        public string Phone { get; set; }

        [Display(GroupName = "ContactInfo")]
        public PreferredContactMethod? PreferredContactMethod { get; set; }

    }

    public enum PreferredContactMethod
    {
        [Display(Name = "E-mail")]
        Email,

        [Display(Name = "Telephone", Description = "The old school thing which can transfer voice much more reliably than what we can do now with all the computers.")]
        Phone
    }

    public record CountrySelectorItem : SelectorItem<int>;
    public record StateSelectorItem : SelectorItem<string>;

    public class CountrySelectorDataProvider : ISelectorDataProvider<CountrySelectorItem>
    {
        public Task<List<CountrySelectorItem>> GetSelectorItems() => Task.FromResult(new List<CountrySelectorItem>()
        {
            new CountrySelectorItem() { Value = 1, DisplayName = "USA" },
            new CountrySelectorItem() { Value = 2, DisplayName = "Czech Republic" }
        });
    }

    public class StateSelectorDataProvider : ISelectorDataProvider<StateSelectorItem, AddressDTO>
    {
        public Task<List<StateSelectorItem>> GetSelectorItems(AddressDTO parameter)
        {
            if (parameter.CountryId == 1)
            {
                return Task.FromResult(new List<StateSelectorItem>() {
                    new StateSelectorItem() { Value = "AL", DisplayName = "Alabama" },
                    new StateSelectorItem() { Value = "AK", DisplayName = "Alaska" },
                    new StateSelectorItem() { Value = "AZ", DisplayName = "Arizona" },
                    new StateSelectorItem() { Value = "AZ", DisplayName = "Arkansas" },
                    new StateSelectorItem() { Value = "CA", DisplayName = "California" },
                    new StateSelectorItem() { Value = "CO", DisplayName = "Colorado" },
                    new StateSelectorItem() { Value = "CT", DisplayName = "Connecticut" },
                    new StateSelectorItem() { Value = "DE", DisplayName = "Delaware" },
                    new StateSelectorItem() { Value = "FL", DisplayName = "Florida" },
                    new StateSelectorItem() { Value = "GA", DisplayName = "Georgia" },
                    new StateSelectorItem() { Value = "HI", DisplayName = "Hawaii" },
                    new StateSelectorItem() { Value = "ID", DisplayName = "Idaho" },
                    new StateSelectorItem() { Value = "IL", DisplayName = "Illinois" },
                    new StateSelectorItem() { Value = "IN", DisplayName = "Indiana" },
                    new StateSelectorItem() { Value = "IA", DisplayName = "Iowa" },
                    new StateSelectorItem() { Value = "KS", DisplayName = "Kansas" },
                    new StateSelectorItem() { Value = "KY", DisplayName = "Kentucky" },
                    new StateSelectorItem() { Value = "LA", DisplayName = "Louisiana" },
                    new StateSelectorItem() { Value = "ME", DisplayName = "Maine" },
                    new StateSelectorItem() { Value = "MD", DisplayName = "Maryland" },
                    new StateSelectorItem() { Value = "MA", DisplayName = "Massachusetts" },
                    new StateSelectorItem() { Value = "MI", DisplayName = "Michigan" },
                    new StateSelectorItem() { Value = "MN", DisplayName = "Minnesota" },
                    new StateSelectorItem() { Value = "MS", DisplayName = "Mississippi" },
                    new StateSelectorItem() { Value = "MO", DisplayName = "Missouri" },
                    new StateSelectorItem() { Value = "MT", DisplayName = "Montana" },
                    new StateSelectorItem() { Value = "NE", DisplayName = "Nebraska" },
                    new StateSelectorItem() { Value = "NV", DisplayName = "Nevada" },
                    new StateSelectorItem() { Value = "NH", DisplayName = "New Hampshire" }
                });
            }
            else
            {
                return Task.FromResult(new List<StateSelectorItem>());
            }
        }
    }
}

