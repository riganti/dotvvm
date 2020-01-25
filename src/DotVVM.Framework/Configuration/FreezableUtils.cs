#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Configuration
{
    internal static class FreezableUtils
    {
        public static Exception Error(string typeName) =>
            new InvalidOperationException($"This {typeName} is frozen and can be no longer modified.");
    }
}
