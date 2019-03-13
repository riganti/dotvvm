using System;

namespace DotVVM.Utils.ConfigurationHost.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string source, string value, StringComparison comp)
        {
            return source?.IndexOf(value, comp) >= 0;
        }

        public static string Escape(this string source)
        {
            return source.Replace(@"\", @"\\").Replace("\"", "\\\"");
        }
    }
}