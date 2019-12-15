using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView
{
    public class InvalidCssClassViewModel
    {
        public GridViewDataSet<SampleDto> Samples { get; set; }
            = new GridViewDataSet<SampleDto> {
                Items = {
                    new SampleDto { Name = "one", Value = "1", Is = true },
                    new SampleDto { Name = "two", Value = "2", Is = true },
                },
                RowEditOptions = {
                    PrimaryKeyPropertyName = nameof(SampleDto.Value)
                }
            };

        public class SampleDto : IValidatableObject
        {
            public bool Is { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            public string Value { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext _)
            {
                if (!Is)
                {
                    yield return new ValidationResult("The sample must Be.", new[] { nameof(Is) });
                }
            }
        }
    }
}
