using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Validation
{
    public class CustomValidationViewModel : DotvvmViewModelBase
    {
        private readonly string[] allowedNames = new[]
        {
            "John",
            "James",
            "Ted"
        };

        public Customer Detail { get; set; } = new Customer { Name = "Ted", Age = 42 };

        public bool UseKnockoutNotation { get; set; }

        public override Task PreRender()
        {
            if (!allowedNames.Contains(Detail.Name))
            {
                if (UseKnockoutNotation)
                {
                    Context.ModelState.Errors.Add(
                        new ViewModelValidationError
                        {
                            ErrorMessage = "This error uses the Knockout JS notation.",
                            PropertyPath = "Detail/Name"
                        });
                }
                else
                {
                    this.AddModelError(t => t.Detail.Name, "This error uses C# extension method AddModelError");
                }
            }
            Context.FailOnInvalidModelState();
            return base.PreRender();
        }

        public class Customer
        {
            [Required]
            public int Age { get; set; }

            [Required]
            public string Name { get; set; }
        }
    }
}
