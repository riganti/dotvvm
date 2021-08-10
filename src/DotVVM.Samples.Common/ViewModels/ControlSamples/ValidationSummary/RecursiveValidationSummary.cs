using DotVVM.Framework.ViewModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.ValidationSummary
{
    public class RecursiveValidationSummaryViewModel
    {
        public RecursiveValidationSummaryViewModel()
        {
            Child1 = new RecursiveValidationSummaryViewModelChild()
            {
                Child = new RecursiveValidationSummaryViewModelChild()
            };
            Child2 = new RecursiveValidationSummaryViewModelChild()
            {
                Child = new RecursiveValidationSummaryViewModelChild()
            };
        }

        public RecursiveValidationSummaryViewModelChild Child1 { get; set; }

        public RecursiveValidationSummaryViewModelChild Child2 { get; set; }

        public List<RecursiveValidationSummaryViewModelChild> Children { get; set; } = new List<RecursiveValidationSummaryViewModelChild> {
            new RecursiveValidationSummaryViewModelChild {
                Child = new RecursiveValidationSummaryViewModelChild(),
            },
            new RecursiveValidationSummaryViewModelChild(),
        };

        [Bind(Direction.ServerToClient)]
        public bool Validated { get; set; }

        public void Validate()
        {
            Validated = true;
        }
    }

    public class RecursiveValidationSummaryViewModelChild
    {
        public RecursiveValidationSummaryViewModelChild Child { get; set; }

        [Required]
        public string Text { get; set; }
    }
}