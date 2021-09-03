using System;
using Microsoft.Owin;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHttpPathString : IPathString
    {
        public DotvvmHttpPathString(PathString originalPath)
        {
            OriginalPath = originalPath;
        }

        public PathString OriginalPath { get; }

        public bool Equals(IPathString other)
        {
            return Equals(other, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(IPathString other, StringComparison comparison)
        {
            if (!HasValue() && !other.HasValue())
            {
                return true;
            }
            return string.Equals(Value, other.Value, comparison);
        }

        public string Value => OriginalPath.Value;
        public bool HasValue()
        {
            return OriginalPath.HasValue;
        }
    }
}