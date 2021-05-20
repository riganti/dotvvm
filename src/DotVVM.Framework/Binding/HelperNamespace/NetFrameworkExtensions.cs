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
    }
}
