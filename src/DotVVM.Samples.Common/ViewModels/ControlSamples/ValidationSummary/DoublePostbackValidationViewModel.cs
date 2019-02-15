using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.ValidationSummary
{
    public class MessagesRendering : DotvvmViewModelBase
    {
        [Required]
        public string StringValue { get; set; }
        public DateTime DateTimeValue { get; set; }
        public DateTime? NullableDateTimeValue { get; set; }
        public int IntValue { get; set; }
        public int? NullableIntValue { get; set; }
    }
}

