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

        public SelectorViewModel<StateSelection, AddressDTO> States { get; set; }

        public SelectorViewModel<CountrySelection> Countries { get; set; }

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
        [Selector(typeof(CountrySelection))]
        public int CountryId { get; set; }
        
        [Display(GroupName = "BasicInfo")]
        [Selector(typeof(StateSelection))]
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

    public record CountrySelection : Selection<int>;
    public record StateSelection : Selection<string>;

    public class CountrySelectionProvider : ISelectionProvider<CountrySelection>
    {
        public Task<List<CountrySelection>> GetSelectorItems() => Task.FromResult(new List<CountrySelection>()
        {
            new CountrySelection() { Value = 1, DisplayName = "USA" },
            new CountrySelection() { Value = 2, DisplayName = "Czech Republic" }
        });
    }

    public class StateSelectorDataProvider : ISelectorDataProvider<StateSelection, AddressDTO>
    {
        public Task<List<StateSelection>> GetSelectorItems(AddressDTO parameter)
        {
            if (parameter.CountryId == 1)
            {
                return Task.FromResult(new List<StateSelection>() {
                    new StateSelection() { Value = "AL", DisplayName = "Alabama" },
                    new StateSelection() { Value = "AK", DisplayName = "Alaska" },
                    new StateSelection() { Value = "AZ", DisplayName = "Arizona" },
                    new StateSelection() { Value = "AZ", DisplayName = "Arkansas" },
                    new StateSelection() { Value = "CA", DisplayName = "California" },
                    new StateSelection() { Value = "CO", DisplayName = "Colorado" },
                    new StateSelection() { Value = "CT", DisplayName = "Connecticut" },
                    new StateSelection() { Value = "DE", DisplayName = "Delaware" },
                    new StateSelection() { Value = "FL", DisplayName = "Florida" },
                    new StateSelection() { Value = "GA", DisplayName = "Georgia" },
                    new StateSelection() { Value = "HI", DisplayName = "Hawaii" },
                    new StateSelection() { Value = "ID", DisplayName = "Idaho" },
                    new StateSelection() { Value = "IL", DisplayName = "Illinois" },
                    new StateSelection() { Value = "IN", DisplayName = "Indiana" },
                    new StateSelection() { Value = "IA", DisplayName = "Iowa" },
                    new StateSelection() { Value = "KS", DisplayName = "Kansas" },
                    new StateSelection() { Value = "KY", DisplayName = "Kentucky" },
                    new StateSelection() { Value = "LA", DisplayName = "Louisiana" },
                    new StateSelection() { Value = "ME", DisplayName = "Maine" },
                    new StateSelection() { Value = "MD", DisplayName = "Maryland" },
                    new StateSelection() { Value = "MA", DisplayName = "Massachusetts" },
                    new StateSelection() { Value = "MI", DisplayName = "Michigan" },
                    new StateSelection() { Value = "MN", DisplayName = "Minnesota" },
                    new StateSelection() { Value = "MS", DisplayName = "Mississippi" },
                    new StateSelection() { Value = "MO", DisplayName = "Missouri" },
                    new StateSelection() { Value = "MT", DisplayName = "Montana" },
                    new StateSelection() { Value = "NE", DisplayName = "Nebraska" },
                    new StateSelection() { Value = "NV", DisplayName = "Nevada" },
                    new StateSelection() { Value = "NH", DisplayName = "New Hampshire" }
                });
            }
            else
            {
                return Task.FromResult(new List<StateSelection>());
            }
        }
    }
}

