using System;
using System.Collections.Generic;
using System.Linq;
using RecordExceptions;

namespace DotVVM.Framework.Configuration
{
    internal static class FreezableUtils
    {
        public static Exception Error(string typeName) =>
            new ObjectIsFrozenException(typeName);

        public record ObjectIsFrozenException(string ObjectTypeName)
            : RecordException($"This {ObjectTypeName} is frozen and can be no longer modified.");
    }
}
