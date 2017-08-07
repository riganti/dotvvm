namespace DotVVM.Framework.Controls
{
    public static class TextBoxTypeExtensions
    {
        public static bool TryGetTagName(this TextBoxType textBoxType, out string tagName)
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
                    tagName = null;
                    return false;
            }
        }

        public static bool TryGetInputType(this TextBoxType textBoxType, out string inputType)
        {
            switch (textBoxType)
            {
                case TextBoxType.Password:
                    inputType = "password";
                    return true;

                case TextBoxType.Telephone:
                    inputType = "tel";
                    return true;

                case TextBoxType.Url:
                    inputType = "url";
                    return true;

                case TextBoxType.Email:
                    inputType = "email";
                    return true;

                case TextBoxType.Date:
                    inputType = "date";
                    return true;

                case TextBoxType.Time:
                    inputType = "time";
                    return true;

                case TextBoxType.Color:
                    inputType = "color";
                    return true;

                case TextBoxType.Search:
                    inputType = "search";
                    return true;

                case TextBoxType.Number:
                    inputType = "number";
                    return true;

                default:
                    inputType = null;
                    return false;
            }
        }
    }
}