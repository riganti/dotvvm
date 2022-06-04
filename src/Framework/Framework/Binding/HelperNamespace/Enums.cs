using System;
using System.Collections.Generic;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding.HelperNamespace
{
    public static class Enums
    {
        public static string[] GetNames<TEnum>()
        {
            return Enum.GetNames(typeof(TEnum));
        }

        public static string? ToEnumString<T>(T? instance) where T : struct, Enum
        {
            return ReflectionUtils.ToEnumString(instance);
        }

        public static string ToEnumString<T>(this T instance) where T : struct, Enum
        {
            return ReflectionUtils.ToEnumString(instance);
        }
    }
}
