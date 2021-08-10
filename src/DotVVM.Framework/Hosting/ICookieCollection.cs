#nullable enable
using System.Collections.Generic;

namespace DotVVM.Framework.Hosting
{
    public interface ICookieCollection : IEnumerable<KeyValuePair<string, string>>
    {
        string this[string key] { get; }
    }
}
