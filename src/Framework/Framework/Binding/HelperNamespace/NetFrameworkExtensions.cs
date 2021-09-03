using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding.HelperNamespace
{
    internal static class NetFrameworkExtensions
    {
        /// <summary>
        /// This is an extension method that allows using unavailable string.Split(..) overload in .NET Framework
        /// </summary>
        public static string[] Split(this string text, string delimiter, StringSplitOptions options = StringSplitOptions.None)
        {
            return text.Split(new[] { delimiter }, options);
        }

        /// <summary>
        /// This is an extension method that allows using unavailable string.Split(..) overload in .NET Framework
        /// </summary>
        public static string[] Split(this string text, char delimiter, StringSplitOptions options = StringSplitOptions.None)
        {
            return text.Split(new[] { delimiter }, options);
        }

        /// <summary>
        /// This is an extension method that allows using unavailable string.Contains(..) overload in .NET Framework
        /// </summary>
        public static bool Contains(this string haystack, string needle, StringComparison options)
        {
            return haystack.IndexOf(needle, options) != -1;
        }

        /// <summary>
        /// This is an extension method that allows using unavailable string.Trim(..) overload in .NET Framework
        /// </summary>
        public static string Trim(this string text, char character)
        {
            return text.Trim(character);
        }

        /// <summary>
        /// This is an extension method that allows using unavailable string.TrimStart(..) overload in .NET Framework
        /// </summary>
        public static string TrimStart(this string text)
        {
            return string.Concat(text.SkipWhile(c => char.IsWhiteSpace(c)));
        }

        /// <summary>
        /// This is an extension method that allows using unavailable string.TrimEnd(..) overload in .NET Framework
        /// </summary>
        public static string TrimEnd(this string text)
        {
            return string.Concat(text.Reverse().SkipWhile(c => char.IsWhiteSpace(c)).Reverse());
        }
    }
}
