#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DotVVM.Framework.Controls
{
    public static class TextBoxTypeExtensions
    {
        private static Dictionary<TextBoxType, string> inputTypes
            = new Dictionary<TextBoxType, string> {
                { TextBoxType.Password, "password" },
                { TextBoxType.Telephone, "tel" },
                { TextBoxType.Url, "url" },
                { TextBoxType.Email, "email" },
                { TextBoxType.Date, "date" },
                { TextBoxType.Time, "time" },
                { TextBoxType.Color, "color" },
                { TextBoxType.Search, "search" },
                { TextBoxType.Number, "number" },
                { TextBoxType.Month, "month" },
                { TextBoxType.DateTimeLocal, "datetime-local" }
            };

        // Contains implicit format string for given TextBoxType.
        // Null format string means value must not be formatted.
        private static Dictionary<TextBoxType, string?> implicitFormatStrings
            = new Dictionary<TextBoxType, string?> {
                { TextBoxType.Date, "yyyy-MM-dd" },
                { TextBoxType.Time, "HH:mm" },
                { TextBoxType.Month, "yyyy-MM" },
                { TextBoxType.DateTimeLocal, "yyyy-MM-ddTHH:mm" },
                // Don't format <input type="Number" ... browsers expect
                // value in specific format and localization breaks it
                { TextBoxType.Number, null }
            };

        public static bool TryGetTagName(this TextBoxType textBoxType, [MaybeNullWhen(false)] out string tagName)
        {
            switch (textBoxType)
            {
                case TextBoxType.Normal:
                    tagName = "input";
                    return true;

                case TextBoxType.MultiLine:
                    tagName = "textarea";
                    return true;

                default:
                    tagName = null!;
                    return false;
            }
        }

        public static bool TryGetFormatString(this TextBoxType textBoxType, out string? formatString)
            => implicitFormatStrings.TryGetValue(textBoxType, out formatString);

        public static bool TryGetInputType(this TextBoxType textBoxType, [MaybeNullWhen(false)] out string? inputType)
            => inputTypes.TryGetValue(textBoxType, out inputType);
    }
}
