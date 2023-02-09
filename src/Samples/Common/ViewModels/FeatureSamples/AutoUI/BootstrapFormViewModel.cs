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
    public class BootstrapFormViewModel : DotvvmViewModelBase
    {
        public CustomerModel Customer { get; set; } = new();

        public SelectionViewModel<CountrySelection> CountrySelection { get; set; } = new();

    }

    public class CustomerModel : IValidatableObject
    {

        [Required]
        public string Name { get; set; }

        public bool IsCompany { get; set; }

        [Required]
        [Selection(typeof(CountrySelection))]
        public int? CountryId { get; set; }

        [Selection(typeof(CountrySelection))]
        public List<int> CountryIds { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsCompany)
            {
                yield return new ValidationResult("Cannot set company!", new[] { nameof(IsCompany) });
            }
            if (!CountryIds.Any())
            {
                yield return new ValidationResult("At least one country must be set!", new[] { nameof(CountryIds) });
            }
        }
    }
}

