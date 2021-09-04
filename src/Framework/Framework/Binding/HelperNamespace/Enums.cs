using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Binding.HelperNamespace
{
    public static class Enums
    {
        public static string[] GetNames<TEnum>()
        {
            return Enum.GetNames(typeof(TEnum));
        }
    }
}
