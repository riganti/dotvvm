using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Helpers
{
    internal static class SelectorStringHelper
    {
        internal static string GetTextFromContent(IEnumerable<ResolvedControl> controls)
        {
            var sb = new StringBuilder();

            foreach (var control in controls)
            {
                if (control.Metadata.Type == typeof(Literal))
                {
                    sb.Append(TryGetNameFromProperty(control, Literal.TextProperty));
                }
                else if (control.Metadata.Type == typeof(HtmlGenericControl))
                {
                    sb.Append(TryGetNameFromProperty(control, HtmlGenericControl.InnerTextProperty));
                }
            }

            // ensure the text is not too long
            var text = RemoveNonIdentifierCharacters(sb.ToString());
            if (text?.Length > 20)
            {
                text = text.Substring(0, 20);
            }
            return text;
        }

        internal static string RemoveNonIdentifierCharacters(string value)
        {
            var sb = new StringBuilder();
            var isLastLetterWhitespace = false;

            for (var i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    if (i == 0)
                    {
                        c = char.ToUpper(c);
                    }
                    else if (isLastLetterWhitespace)
                    {
                        c = char.ToUpper(c);
                        isLastLetterWhitespace = false;
                    }

                    sb.Append(c);
                }
                else if (char.IsWhiteSpace(c))
                {
                    isLastLetterWhitespace = true;
                }
            }

            if (sb.Length == 0)
            {
                return null;
            }

            if (char.IsDigit(sb[0]))
            {
                sb.Insert(0, '_');
            }

            return sb.ToString();
        }

        internal static string TryGetNameFromProperty(ResolvedControl control, DotvvmProperty property)
        {
            if (control.TryGetProperty(property, out IAbstractPropertySetter setter))
            {
                switch (setter)
                {
                    case ResolvedPropertyValue propertySetter:
                        return propertySetter.Value?.ToString();

                    case ResolvedPropertyBinding propertyBinding:
                        return propertyBinding.Binding.Value;
                }
            }
            return null;
        }

        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        internal static string AddDataContextPrefixesToName(IList<string> dataContextPrefixes, string uniqueName)
        {
            if (dataContextPrefixes.Any())
            {
                return $"{string.Join("_", dataContextPrefixes)}_{SetFirstLetterUp(uniqueName)}";
            }

            return SetFirstLetterUp(uniqueName);
        }

        internal static string NormalizeUniqueName(string uniqueName)
        {
            var normalizedName = RemoveDiacritics(uniqueName);
            var firstLetterOfName = normalizedName[0];

            // if first letter is numeric add underscore to the name
            if (char.IsDigit(firstLetterOfName))
            {
                normalizedName = normalizedName.Insert(0, "_");
            }
            else
            {
                char[] a = normalizedName.ToCharArray();
                a[0] = char.ToUpper(a[0]);
                normalizedName = new string(a);
            }

            return normalizedName;
        }

        internal static string SetFirstLetterUp(string uniqueName)
        {
            return uniqueName.First().ToString().ToUpper() + uniqueName.Substring(1);
        }
    }
}
