using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace DotVVM.Framework.Hosting
{
    public interface IQueryCollection : IEnumerable<KeyValuePair<string, string>>
    {
        string this[string key] { get; }

        bool TryGetValue(string key, out string value);

        bool ContainsKey(string key);
    }
}