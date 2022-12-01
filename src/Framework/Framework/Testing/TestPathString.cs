using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Testing
{
    internal class TestPathString : IPathString
    {
        public TestPathString(string? value)
        {
            this.Value = value;
        }

        public string? Value { get; }

        public bool Equals(IPathString? other) =>
            other == this ||
            other != null && other.HasValue() == this.HasValue() && other.Value == this.Value;

        public bool HasValue() => !string.IsNullOrEmpty(Value);
    }
}
