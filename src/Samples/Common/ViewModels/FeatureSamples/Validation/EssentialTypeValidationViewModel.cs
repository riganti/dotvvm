using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Validation
{
	public class EssentialTypeValidationViewModel
    {
        public int IntegerProperty { get; set; }
        public double FloatProperty { get; set; }
        [Required]
        public int? NullableIntegerProperty { get; set; }
        public double? NullableFloatProperty { get; set; }

        public List<EssentialTypeValidationViewModel> Collection { get; set; }
        public EssentialTypeValidationViewModel NestedVM { get; set; }

        public void Postback()
        {
        }

        public void AddNestedVMs()
        {
            NestedVM = new EssentialTypeValidationViewModel();
            Collection = new List<EssentialTypeValidationViewModel>
            {
                new EssentialTypeValidationViewModel(),
                new EssentialTypeValidationViewModel()
            };
        }
    }       
}

