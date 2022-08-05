using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Utils
{
    public static class NamingUtils
    {
        // It would be nice to have validation methods for all named things here

        public static bool IsValidResourceName(string name)
        {
            // return Regex.IsMatch(name, @"^[a-zA-Z0-9]+([._-][a-zA-Z0-9]+)*$", RegexOptions.CultureInvariant);
            // but the regex is slow, so:

            bool allowedFirstLetter(char ch) =>
                HtmlWriter.IsInRange(ch, 'a', 'z') ||
                HtmlWriter.IsInRange(ch, 'A', 'Z') ||
                HtmlWriter.IsInRange(ch, '0', '9');
            bool allowedLetter(char ch) =>
                allowedFirstLetter(ch) || ch is '.' or '_' or '-';

            if (name.Length == 0)
                return false;
            if (!allowedLetter(name[0]))
                return false;
            for (int i = 1; i < name.Length; i++)
            {
                if (!allowedLetter(name[i]))
                    return false;
                if (name[i] is '.' or '_' or '-')
                {
                    // allowed only once and not at the end
                    if (name.Length <= i + 1 || !allowedFirstLetter(name[i + 1]))
                        return false;
                }
            }
            return true;
        }

        public static bool IsValidConcurrencyQueueName(string name)
        {
            return name.Length > 0
                && (char.IsLetter(name[0]) || name[0] == '_')
                && name.Skip(1).All(l => char.IsLetterOrDigit(l) || l == '_' || l == '-' || l == '.');
        }
    }
}
