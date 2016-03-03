using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public class ControlResolverMetadata : ControlResolverMetadataBase
    {
        private readonly ControlType controlType;

        public new Type Type => controlType.Type;

        public Type ControlBuilderType => controlType.ControlBuilderType;
        
        public new DotvvmProperty DefaultContentProperty => (DotvvmProperty) base.DefaultContentProperty;

        public new Type DataContextConstraint => controlType.DataContextRequirement;


        public ControlResolverMetadata(ControlType controlType) : base(controlType)
        {
            this.controlType = controlType;

            DataContextChangeAttributes = Type.GetCustomAttributes<DataContextChangeAttribute>(true).ToArray();
        }

        public override DataContextChangeAttribute[] DataContextChangeAttributes { get; }

        protected override void LoadProperties(Dictionary<string, IPropertyDescriptor> result)
        {
            foreach (var property in DotvvmProperty.ResolveProperties(controlType.Type).Concat(DotvvmProperty.GetVirtualProperties(controlType.Type)))
            {
                result.Add(property.Name, property);
            }
        }
        
        /// <summary>
        /// Finds the property.
        /// </summary>
        public DotvvmProperty FindProperty(string name)
        {
            IPropertyDescriptor result;
            return Properties.TryGetValue(name, out result) ? (DotvvmProperty)result : null;
        }

    }
}