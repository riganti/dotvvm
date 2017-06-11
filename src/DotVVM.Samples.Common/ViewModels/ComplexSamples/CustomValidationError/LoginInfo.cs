using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DotVVM.Samples.Common.ViewModels.ComplexSamples.CustomValidationError
{
    public class LoginInfo
    {
        public bool IsLoggedIn { get; set; }

        [Required]
        public string Nick { get; set; }

        [Required]
        public string Password { get; set; }

    }
}
