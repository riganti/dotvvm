using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime;
using Newtonsoft.Json;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public class ControlResolverMetadata : ControlResolverMetadataBase
    {
        private readonly ControlType controlType;

        public new Type Type => controlType.Type;

        [JsonIgnore]
        public new DotvvmProperty DefaultContentProperty => (DotvvmProperty) base.DefaultContentProperty;

        [JsonIgnore]
        public new Type DataContextConstraint => controlType.DataContextRequirement;

        public ControlResolverMetadata(ControlType controlType) : base(controlType)
        {
            this.controlType = controlType;

            DataContextChangeAttributes = Type.GetCustomAttributes<DataContextChangeAttribute>(true).ToArray();
            DataContextManipulationAttribute = Type.GetCustomAttribute<DataContextStackManipulationAttribute>(true);
            if (DataContextManipulationAttribute != null && DataContextChangeAttributes.Any())
                throw new Exception($"{nameof(DataContextChangeAttributes)} and {nameof(DataContextManipulationAttribute)} can not be set at the same time at control '{controlType.Type.FullName}'.");
        }

        public ControlResolverMetadata(Type type) : this(new ControlType(type))
        {
        }

        [JsonIgnore]
        public override sealed DataContextChangeAttribute[] DataContextChangeAttributes { get; }
        [JsonIgnore]
        public override sealed DataContextStackManipulationAttribute DataContextManipulationAttribute { get; }


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

        protected override void LoadPropertyGroups(List<PropertyGroupMatcher> result)
        {
            result.AddRange(DotvvmPropertyGroup .GetPropertyGroups(Type)
                .SelectMany(g => g.Prefixes.Select(p => new PropertyGroupMatcher(p, g))));
            result.Sort((a, b) => b.Prefix.Length.CompareTo(a.Prefix.Length));
        }
    }
}
