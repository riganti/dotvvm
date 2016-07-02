using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Checks whether the value matches the format specified by DotVVM control (e.g. TextBox).
    /// The value can be empty string or null. If it is not, it must match the FormatString that the control specifies.
    /// </summary>
    public class DotvvmEnforceClientFormatAttribute : ValidationAttribute
    {
        public bool AllowNull { get; set; } = true;

        public bool AllowEmptyStringOrWhitespaces { get; set; } = true;

        public bool AllowEmptyString { get; set; } = true;


        public override bool IsValid(object value)
        {
            return true;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }
    }
}
