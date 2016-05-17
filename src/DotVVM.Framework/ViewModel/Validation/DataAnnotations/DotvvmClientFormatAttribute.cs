using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ViewModel.Validation.DataAnnotations
{
    public class DotvvmEnforceClientFormatAttribute : ValidationAttribute
    {
        public bool AllowNull { get; set; } = true;

        public bool AllowEmptyStringOrWhitespaces { get; set; } = true;
        public bool AllowEmptyString { get; set; } = true;

        public override bool IsValid(object value)
        {
            var valid = true;

            ////value required 
            //var valueToValidate = value as string;
            //if (!AllowNull && valueToValidate == null)
            //{
            //    valid = false;

            //}
            ////whitespaces
            //if (!AllowEmptyString && string.IsNullOrEmpty(valueToValidate))
            //{
            //    valid = false;
            //}

            ////whitespaces
            //if (!AllowEmptyStringOrWhitespaces && string.IsNullOrWhiteSpace(valueToValidate))
            //{
            //    valid = false;

            //}
            return valid;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //todo:rewrite
            return base.IsValid(value, validationContext);
        }
    }
}
