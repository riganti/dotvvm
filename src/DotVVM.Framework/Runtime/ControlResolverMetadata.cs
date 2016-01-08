using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Runtime
{
    public class ControlResolverMetadata : ControlResolverMetadataBase
    {
        private readonly ControlType controlType;

        public new Type Type => controlType.Type;

        public Type ControlBuilderType => controlType.ControlBuilderType;

        public Dictionary<string, DotvvmProperty> Properties { get; set; }

        public new DotvvmProperty DefaultContentProperty => (DotvvmProperty) base.DefaultContentProperty;

        public new Type DataContextConstraint => controlType.DataContextRequirement;


        public ControlResolverMetadata(ControlType controlType) : base(controlType)
        {
            this.controlType = controlType;
            LoadProperties();
        }

        private void LoadProperties()
        {
            foreach (var property in DotvvmProperty.ResolveProperties(controlType.Type).Concat(DotvvmProperty.GetVirtualProperties(controlType.Type)))
            {
                properties.Add(property.Name, property);
            }
        }


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