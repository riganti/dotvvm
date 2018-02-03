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
        public DotvvmPropertyGroup  PropertyGroup { get; private set; }
        public string GroupMemberName { get; private set; }

        public static GroupedDotvvmProperty Create(DotvvmPropertyGroup  group, string name)
        {
            var propname = group.Name + ":" + name;
            var prop = new GroupedDotvvmProperty
            {
                PropertyGroup = group,
                GroupMemberName = name,
                PropertyType = group.PropertyType,
                DeclaringType = group.DeclaringType,
                DefaultValue = group.DefaultValue,
                IsValueInherited = false,
                Name = propname
            };
            if (group.PropertyGroupMode == PropertyGroupMode.ValueCollection) prop.IsVirtual = true;

            DotvvmProperty.InitializeProperty(prop, (MemberInfo)group.DescriptorField ?? group.PropertyInfo);
            return prop;
        }
    }
}
