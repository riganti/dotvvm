using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.ValidationSummary
{
    public class RecursiveValidationSummaryViewModel
    {

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


        public RecursiveValidationSummaryViewModel()
        {
            Child1 = new RecursiveValidationSummaryViewModelChild() {
                Child = new RecursiveValidationSummaryViewModelChild()
            };
            Child2 = new RecursiveValidationSummaryViewModelChild() {
                Child = new RecursiveValidationSummaryViewModelChild()
            };
        }

        public void Validate()
        {
            Validated = true;
        }
    }

    public class RecursiveValidationSummaryViewModelChild
    {

        [Required]
        public string Text { get; set; }

        public RecursiveValidationSummaryViewModelChild Child { get; set; }

    }
}