#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotVVM.Framework.Utils
{
    public static class NamingUtils
    {

        // It would be nice to have validation methods for all named things here

        public static bool IsValidResourceName(string name)
        {
            return Regex.IsMatch(name, @"^[a-zA-Z0-9]+([._-][a-zA-Z0-9]+)*$");
        }

        public static bool IsValidConcurrencyQueueName(string name)
        {
            return name.Length > 0
                && (char.IsLetter(name[0]) || name[0] == '_')
                && name.Skip(1).All(l => char.IsLetterOrDigit(l) || l == '_' || l == '-' || l == '.');
        }
    }
}
