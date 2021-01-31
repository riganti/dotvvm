#nullable enable
using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Hosting
{
    public class CustomResponsePropertiesManager
    {
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();
        public bool PropertiesSerialized { get; internal set; }
        public IReadOnlyDictionary<string, object> Properties => properties;

        public void Add(string key, object value)
        {
            if (PropertiesSerialized)
            {
                throw new InvalidOperationException("Cannot add new custom property. The properties have already been serialized into the response.");
            }
            if (properties.ContainsKey(key))
            {
                throw new InvalidOperationException($"Custom property {key} already exists.");
            }
            properties[key] = value;
        }
    }
}
