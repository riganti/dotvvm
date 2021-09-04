using System;

namespace DotVVM.Samples.Tests
{
 public static class Extensions
    {
        public static bool Contains(this string text, string value, StringComparison comparison)
        {

            return text.IndexOf(value, comparison) > -1;
        }
    }
}
