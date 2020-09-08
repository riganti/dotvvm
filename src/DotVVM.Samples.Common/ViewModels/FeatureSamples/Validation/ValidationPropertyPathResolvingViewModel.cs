using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Validation
{
    public class ValidationPropertyPathResolvingViewModel : DotvvmViewModelBase
    {
        [Fail]
        public string Text { get; set; }

        public Data Data { get; set; } = new Data();
        public Data Data2 { get; set; } = new Data();

        public List<Data> Col { get; set; } = new List<Data>() {
            new Data(),
            new Data(),
            new Data()
        };
    }

    public class Fail : ValidationAttribute
    {
        public override bool IsValid(object value) => false;
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) => new ValidationResult("FAIL");
    }

    public class Data
    {
        [Fail]
        public string Text { get; set; }

    }
}

