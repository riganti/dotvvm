#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DotVVM.Framework.Hosting
{
    public interface IQueryCollection : IEnumerable<KeyValuePair<string, string>>
    {
        string this[string key] { get; }

        bool TryGetValue(string key, [MaybeNullWhen(false)] out string value);

        bool ContainsKey(string key);
    }
}
