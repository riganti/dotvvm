using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Runtime
{
    public class ControlResolverMetadata
    {

        public string Namespace { get; set; }

        public string Name { get; set; }

        public Type Type { get; set; }

        public Type ControlBuilderType { get; set; }

        public bool HasHtmlAttributesCollection { get; set; }

        public Dictionary<string, DotvvmProperty> Properties { get; set; }
        
        public bool IsContentAllowed { get; set; }

        public DotvvmProperty DefaultContentProperty { get; set; }

        public string VirtualPath { get; internal set; }
        public Type DataContextConstraint { get; set; }



        /// <summary>
        /// Finds the property.
        /// </summary>
        public DotvvmProperty FindProperty(string name)
        {
            DotvvmProperty result;
            return Properties.TryGetValue(name, out result) ? result : null;
        }
    }
}