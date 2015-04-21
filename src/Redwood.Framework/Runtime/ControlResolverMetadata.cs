using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Runtime
{
    public class ControlResolverMetadata
    {

        public string Namespace { get; set; }

        public string Name { get; set; }

        public Type Type { get; set; }

        public Type ControlBuilderType { get; set; }

        public bool HasHtmlAttributesCollection { get; set; }

        public Dictionary<string, RedwoodProperty> Properties { get; set; }

        public bool IsContentAllowed { get; set; }

        public string VirtualPath { get; internal set; }



        /// <summary>
        /// Finds the property.
        /// </summary>
        public RedwoodProperty FindProperty(string name)
        {
            RedwoodProperty result;
            return Properties.TryGetValue(name, out result) ? result : null;
        }
    }
}