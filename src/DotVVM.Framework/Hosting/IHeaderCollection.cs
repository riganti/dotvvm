#nullable enable
using System.Collections.Generic;

namespace DotVVM.Framework.Hosting
{
    public interface IHeaderCollection : IDictionary<string, string[]>
    {
        new string? this[string key] { get; set; }

        void Append(string key, string value);
    }
}
