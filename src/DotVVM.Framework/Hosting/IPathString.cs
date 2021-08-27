using System;

namespace DotVVM.Framework.Hosting
{
    public interface IPathString : IEquatable<IPathString>
    {
        string? Value { get; }
        bool HasValue();
    }
}
