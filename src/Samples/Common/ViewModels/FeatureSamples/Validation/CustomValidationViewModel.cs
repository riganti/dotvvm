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
                    Context.AddModelError("/Detail/Name", "This error uses a hand-written property path.");
                }
                else
                {
                    this.AddModelError(t => t.Detail.Name, "This error uses C# extension method AddModelError that generates property path based on provided lambda.");
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
