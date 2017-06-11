using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Samples.Common.ViewModels.ComplexSamples.CustomValidationError
{
    public class TargetOnValidationSummaryViewModel : CustomValidationErrorViewModel
    {
        public override string GetPropertyPath()
        {
            return nameof(Login);
        }
    }
}
