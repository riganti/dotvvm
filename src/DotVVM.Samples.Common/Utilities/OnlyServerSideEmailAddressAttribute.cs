using System.ComponentModel.DataAnnotations;

namespace DotVVM.Samples.BasicSamples.Utilities
{
    public sealed class OnlyServerSideEmailAddressAttribute : DataTypeAttribute
    {
        public OnlyServerSideEmailAddressAttribute() : base(DataType.EmailAddress)
        {
            ErrorMessage = "The {0} field is not a valid e-mail address.";
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            if (!(value is string valueAsString))
            {
                return false;
            }

            // only return true if there is only 1 '@' character
            // and it is neither the first nor the last character
            bool found = false;
            for (int i = 0; i < valueAsString.Length; i++)
            {
                if (valueAsString[i] == '@')
                {
                    if (found || i == 0 || i == valueAsString.Length - 1)
                    {
                        return false;
                    }
                    found = true;
                }
            }

            return found;
        }
    }
}
