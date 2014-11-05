using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Runtime
{
    public class ControlResolverMetadata
    {

        public string Namespace { get; set; }

        public string Name { get; set; }

        public Type Type { get; set; }

        public bool HasHtmlAttributesCollection { get; set; }

        public Dictionary<string, ControlResolverPropertyMetadata> Properties { get; set; }

        /// <summary>
        /// Finds the property.
        /// </summary>
        public ControlResolverPropertyMetadata FindProperty(string name)
        {
            ControlResolverPropertyMetadata result;
            return Properties.TryGetValue(name, out result) ? result : null;
        }
    }
}