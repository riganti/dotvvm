using System;
using System.ComponentModel.DataAnnotations;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Validation
{
    public class ValidatorValueComplexExpressionsViewModel : DotvvmViewModelBase
    {
        [Required]
        public DateTime DateTime { get; set; }

        public TestDto[] Collection { get; set; } = new[]
        {
            new TestDto() { Id = 1, Description = "DateTime is not null", DateTime = DateTime.Now },
            new TestDto() { Id = 1, Description = "DateTime is null", DateTime = null }
        };
    }

    public class TestDto
    {
        public int Id { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime? DateTime { get; set; }
    }
}

