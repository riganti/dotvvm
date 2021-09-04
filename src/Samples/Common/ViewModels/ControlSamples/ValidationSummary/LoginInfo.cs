using System.ComponentModel.DataAnnotations;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.ValidationSummary
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