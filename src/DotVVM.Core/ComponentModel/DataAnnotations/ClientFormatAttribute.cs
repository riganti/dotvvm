using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Checks whether the value matches the format specified by DotVVM control (e.g. TextBox).
    /// The value can be empty string or null. If it is not, it must match the FormatString that the control specifies.
    /// </summary>
    [Obsolete("The client format is enforced by default. If you need to disable it use ClientFormatAttribute. It also allows you to configure behavior of validation.", true)]
    public class DotvvmEnforceClientFormatAttribute : ValidationAttribute
    {
    }

    /// <summary>
    /// Checks whether the value matches the format specified by DotVVM control (e.g. TextBox).
    /// The value can be empty string or null. If it is not, it must match the FormatString that the control specifies.
    /// </summary>
    public class DotvvmClientFormatAttribute : ValidationAttribute
    {
        public bool Disable { get; set; }
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
