using DotVVM.Framework.Compilation.ControlTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding
{
    public class GroupedDotvvmProperty : DotvvmProperty
    {
        public DotvvmPropertyGroup PropertyGroup { get; }

        public string GroupMemberName { get; }

        public GroupedDotvvmProperty(string groupMemberName, DotvvmPropertyGroup propertyGroup)
        {
            this.GroupMemberName = groupMemberName;
            this.PropertyGroup = propertyGroup;
        }


        public static GroupedDotvvmProperty Create(DotvvmPropertyGroup group, string name)
        {
            var propname = group.Name + ":" + name;
            var prop = new GroupedDotvvmProperty(name, group) {
                PropertyType = group.PropertyType,
                DeclaringType = group.DeclaringType,
                DefaultValue = group.DefaultValue,
                IsValueInherited = false,
                Name = propname,
                ObsoleteAttribute = group.ObsoleteAttribute
            };
            if (group.PropertyGroupMode == PropertyGroupMode.ValueCollection) prop.IsVirtual = true;

            DotvvmProperty.InitializeProperty(prop, group.AttributeProvider);
            return prop;
        }
    }
}
