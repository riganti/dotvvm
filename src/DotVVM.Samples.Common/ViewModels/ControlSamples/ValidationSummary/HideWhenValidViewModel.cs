using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.ValidationSummary
{
    public class HideWhenValidViewModel
    {
        [Required]
        public string Text { get; set; }

        public void Validate()
        {

        }
    }
}
